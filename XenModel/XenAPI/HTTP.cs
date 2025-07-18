/*
 * Copyright (c) Cloud Software Group, Inc.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 *   1) Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 *
 *   2) Redistributions in binary form must reproduce the above
 *      copyright notice, this list of conditions and the following
 *      disclaimer in the documentation and/or other materials
 *      provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
 * COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
 * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
 * OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.Serialization;

namespace XenAPI
{
    public partial class HTTP
    {
        #region Exceptions

        [Serializable]
        public class TooManyRedirectsException : Exception
        {
            private readonly int redirect;
            private readonly Uri uri;

            public TooManyRedirectsException(int redirect, Uri uri)
            {
                this.redirect = redirect;
                this.uri = uri;
            }

            public TooManyRedirectsException() : base() { }

            public TooManyRedirectsException(string message) : base(message) { }

            public TooManyRedirectsException(string message, Exception exception) : base(message, exception) { }

            protected TooManyRedirectsException(SerializationInfo info, StreamingContext context)
                : base(info, context)
            {
                redirect = info.GetInt32("redirect");
                uri = (Uri)info.GetValue("uri", typeof(Uri));
            }

            public override void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                    throw new ArgumentNullException(nameof(info));

                info.AddValue("redirect", redirect);
                info.AddValue("uri", uri, typeof(Uri));

                base.GetObjectData(info, context);
            }
        }

        [Serializable]
        public class BadServerResponseException : Exception
        {
            public BadServerResponseException() : base() { }

            public BadServerResponseException(string message) : base(message) { }

            public BadServerResponseException(string message, Exception exception) : base(message, exception) { }

            protected BadServerResponseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }

        [Serializable]
        public class CancelledException : Exception
        {
            public CancelledException() : base() { }

            public CancelledException(string message) : base(message) { }

            public CancelledException(string message, Exception exception) : base(message, exception) { }

            protected CancelledException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }

        [Serializable]
        public class ProxyServerAuthenticationException : Exception
        {
            public ProxyServerAuthenticationException() : base() { }

            public ProxyServerAuthenticationException(string message) : base(message) { }

            public ProxyServerAuthenticationException(string message, Exception exception) : base(message, exception) { }

            protected ProxyServerAuthenticationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }

        #endregion

        public delegate bool FuncBool();
        public delegate void UpdateProgressDelegate(int percent);
        public delegate void DataCopiedDelegate(long bytes);

        // Size of byte buffer used for GETs and PUTs
        // (not the socket rx buffer)
        public const int BUFFER_SIZE = 32 * 1024;
        public const int MAX_REDIRECTS = 10;

        public const int DEFAULT_HTTPS_PORT = 443;
        private const int NONCE_LENGTH = 16;

        public enum ProxyAuthenticationMethod
        {
            Basic = 0,
            Digest = 1
        }

        /// <summary>
        /// The authentication scheme to use for authenticating to a proxy server.
        /// Defaults to Digest.
        /// </summary>
        public static ProxyAuthenticationMethod CurrentProxyAuthenticationMethod = ProxyAuthenticationMethod.Digest;

        #region Helper functions

        private static void WriteLine(String txt, Stream stream)
        {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(String.Format("{0}\r\n", txt));
            stream.Write(bytes, 0, bytes.Length);
        }

        private static void WriteLine(Stream stream)
        {
            WriteLine("", stream);
        }

        // Stream.ReadByte() is used because using StreamReader in its place causes the reading to become stuck,
        // as it seems the Stream object has trouble recognizing the end of the stream. This seems to be a common
        // problem, of which a common solution is to read each byte until an EndOfStreamException is thrown, as is
        // done here.
        private static string ReadLine(Stream stream)
        {
            System.Text.StringBuilder result = new StringBuilder();
            while (true)
            {
                int b = stream.ReadByte();
                if (b == -1)
                    throw new EndOfStreamException();
                char c = Convert.ToChar(b);
                result.Append(c);
                if (c == '\n')
                    return result.ToString();
            }
        }

        /// <summary>
        /// Read HTTP headers, doing any redirects as necessary
        /// </summary>
        /// <returns>True if a redirect has occurred - headers will need to be resent.</returns>
        private static bool ReadHttpHeaders(ref Stream stream, IWebProxy proxy, bool nodelay, int timeout_ms, List<string> headers = null)
        {
            // read headers/fields
            string line = ReadLine(stream), initialLine = line, transferEncodingField = null;
            if (string.IsNullOrEmpty(initialLine)) // sanity check
                return false;
            if (headers == null)
                headers = new List<string>();
            while (!string.IsNullOrWhiteSpace(line)) // IsNullOrWhiteSpace also checks for empty string
            {
                line = line.TrimEnd('\r', '\n');
                headers.Add(line);
                if (line == "Transfer-Encoding: Chunked")
                    transferEncodingField = line;
                line = ReadLine(stream);
            }

            // read chunks
            string entityBody = "";
            if (!string.IsNullOrEmpty(transferEncodingField))
            {
                int lastChunkSize = -1;
                do
                {
                    // read chunk size
                    string chunkSizeStr = ReadLine(stream);
                    chunkSizeStr = chunkSizeStr.TrimEnd('\r', '\n');
                    int chunkSize = 0;
                    int.TryParse(chunkSizeStr, System.Globalization.NumberStyles.HexNumber,
                        System.Globalization.CultureInfo.InvariantCulture, out chunkSize);

                    // read <chunkSize> number of bytes from the stream
                    int totalNumberOfBytesRead = 0;
                    int numberOfBytesRead;
                    byte[] bytes = new byte[chunkSize];
                    do
                    {
                        numberOfBytesRead = stream.Read(bytes, totalNumberOfBytesRead, chunkSize - totalNumberOfBytesRead);
                        totalNumberOfBytesRead += numberOfBytesRead;
                    } while (numberOfBytesRead > 0 && totalNumberOfBytesRead < chunkSize);

                    string str = System.Text.Encoding.ASCII.GetString(bytes);
                    string[] split = str.Split(new string[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
                    headers.AddRange(split);

                    entityBody += str;

                    line = ReadLine(stream); // empty line in the end of chunk

                    lastChunkSize = chunkSize;
                } while (lastChunkSize != 0);

                entityBody = entityBody.TrimEnd('\r', '\n');
                headers.Add(entityBody); // keep entityBody if it's needed for Digest authentication (when qop="auth-int")
            }
            else
            {
                // todo: handle other transfer types, in case "Transfer-Encoding: Chunked" isn't used
            }

            // handle server response
            int code = getResultCode(initialLine);
            switch (code)
            {
                case 407: // authentication error; caller must handle this case
                case 200:
                    break;

                case 302:
                    var header = headers.FirstOrDefault(h => h.StartsWith("Location:", StringComparison.InvariantCultureIgnoreCase));
                    string url = header == null ? "" : header.Substring(9).Trim();
                    Uri redirect = new Uri(url);
                    stream.Close();
                    stream = ConnectStream(redirect, proxy, nodelay, timeout_ms);
                    return true; // headers need to be sent again

                default:
                    stream.Close();
                    throw new BadServerResponseException(string.Format("Received error code {0} from the server", initialLine));
            }

            return false;
        }

        private static int getResultCode(string line)
        {
            string[] bits = line.Split(new char[] { ' ' });
            return (bits.Length < 2 ? 0 : Int32.Parse(bits[1]));
        }

        public static bool UseSSL(Uri uri)
        {
            return uri.Scheme == "https" || uri.Port == DEFAULT_HTTPS_PORT;
        }

        private static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// Returns a secure MD5 hash of the given input string.
        /// </summary>
        /// <param name="str">The string to hash.</param>
        /// <returns>The secure hash as a hex string.</returns>
        private static string _MD5Hash(string str)
        {
            return ComputeHash(str, "MD5");
        }

        /// <summary>
        /// Returns a secure SHA256 hash of the given input string.
        /// </summary>
        /// <param name="str">The string to hash.</param>
        /// <returns>The secure hash as a hex string.</returns>
        private static string Sha256Hash(string str)
        {
            return ComputeHash(str, "SHA256");
        }

        private static string ComputeHash(string input, string method)
        {
            if (input == null)
                return null;

            var enc = new UTF8Encoding();
            byte[] bytes = enc.GetBytes(input);

            using (var hasher = HashAlgorithm.Create(method))
            {
                if (hasher != null)
                {
                    byte[] hash = hasher.ComputeHash(bytes);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }

            return null;
        }

        private static string GenerateNonce()
        {
            using (var rngCsProvider = new RNGCryptoServiceProvider())
            {
                var nonceBytes = new byte[NONCE_LENGTH];
                rngCsProvider.GetBytes(nonceBytes);
                return Convert.ToBase64String(nonceBytes);
            }
        }

        public static long CopyStream(Stream inStream, Stream outStream,
            DataCopiedDelegate progressDelegate, FuncBool cancellingDelegate)
        {
            long bytesWritten = 0;
            byte[] buffer = new byte[BUFFER_SIZE];
            DateTime lastUpdate = DateTime.Now;

            while (cancellingDelegate == null || !cancellingDelegate())
            {
                int bytesRead = inStream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;
                outStream.Write(buffer, 0, bytesRead);
                bytesWritten += bytesRead;

                if (progressDelegate != null &&
                    DateTime.Now - lastUpdate > TimeSpan.FromMilliseconds(500))
                {
                    progressDelegate(bytesWritten);
                    lastUpdate = DateTime.Now;
                }
            }

            if (cancellingDelegate != null && cancellingDelegate())
                throw new CancelledException();

            if (progressDelegate != null)
                progressDelegate(bytesWritten);

            return bytesWritten;
        }

        /// <summary>
        /// Build a URI from a hostname, a path, and some query arguments
        /// </summary>
        /// <param name="args">An even-length array, alternating argument names and values</param>
        /// <param name="hostname"></param>
        /// <param name="path"></param>
        public static Uri BuildUri(string hostname, string path, params object[] args)
        {
            // The last argument may be an object[] in its own right, in which case we need
            // to flatten the array.
            List<object> flatargs = new List<object>();
            foreach (object arg in args)
            {
                if (arg is IEnumerable<object>)
                    flatargs.AddRange((IEnumerable<object>)arg);
                else
                    flatargs.Add(arg);
            }

            var query = new StringBuilder();
            for (int i = 0; i < flatargs.Count - 1; i += 2)
            {
                if (flatargs[i + 1] == null)//skip null arguments
                    continue;

                if (query.Length != 0)
                    query.Append('&');

                query.Append(flatargs[i]).Append("=");

                if (flatargs[i + 1] is bool)
                    query.Append((bool)flatargs[i + 1] ? "true" : "false");
                else
                    query.Append(Uri.EscapeDataString(flatargs[i + 1].ToString()));
            }

            UriBuilder uri = new UriBuilder
            {
                Scheme = "https",
                Port = DEFAULT_HTTPS_PORT,
                Host = hostname,
                Path = path,
                Query = query.ToString()
            };

            return uri.Uri;
        }

        private static string GetPartOrNull(string str, int partIndex)
        {
            string[] parts = str.Split(new char[] { ' ' }, partIndex + 2, StringSplitOptions.RemoveEmptyEntries);
            return partIndex < parts.Length - 1 ? parts[partIndex] : null;
        }

        #endregion

        private static NetworkStream ConnectSocket(Uri uri, bool nodelay, int timeoutMs)
        {
            AddressFamily addressFamily = uri.HostNameType == UriHostNameType.IPv6
                                              ? AddressFamily.InterNetworkV6
                                              : AddressFamily.InterNetwork;
            Socket socket =
                new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = nodelay;
            socket.ReceiveTimeout = timeoutMs;
            socket.SendTimeout = timeoutMs;
            socket.Connect(uri.Host, uri.Port);

            return new NetworkStream(socket, true);
        }

        /// <summary>
        /// This function will connect a stream to a uri (host and port),
        /// negotiating proxies and SSL
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="proxy"></param>
        /// <param name="nodelay"></param>
        /// <param name="timeoutMs">Timeout, in ms. 0 for no timeout.</param>
        public static Stream ConnectStream(Uri uri, IWebProxy proxy, bool nodelay, int timeoutMs)
        {
            IMockWebProxy mockProxy = proxy as IMockWebProxy;
            if (mockProxy != null)
                return mockProxy.GetStream(uri);

            Stream stream;
            bool useProxy = proxy != null && !proxy.IsBypassed(uri);

            if (useProxy)
            {
                Uri proxyURI = proxy.GetProxy(uri);
                stream = ConnectSocket(proxyURI, nodelay, timeoutMs);
            }
            else
            {
                stream = ConnectSocket(uri, nodelay, timeoutMs);
            }

            try
            {
                if (useProxy)
                {
                    string line = string.Format("CONNECT {0}:{1} HTTP/1.0", uri.Host, uri.Port);
                    WriteLine(line, stream);
                    WriteLine(stream);

                    List<string> initialResponse = new List<string>();
                    ReadHttpHeaders(ref stream, proxy, nodelay, timeoutMs, initialResponse);

                    AuthenticateProxy(ref stream, uri, proxy, nodelay, timeoutMs, initialResponse, line);
                }

                if (UseSSL(uri))
                {
                    SslStream sslStream = new SslStream(stream, false,
                        new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                    sslStream.AuthenticateAsClient("", null, SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12, true);

                    stream = sslStream;
                }

                return stream;
            }
            catch
            {
                stream.Close();
                throw;
            }
        }

        private static void AuthenticateProxy(ref Stream stream, Uri uri, IWebProxy proxy, bool nodelay, int timeoutMs, List<string> initialResponse, string header)
        {
            // perform authentication only if proxy requires it
            List<string> fields = initialResponse.FindAll(str => str.StartsWith("Proxy-Authenticate:", StringComparison.InvariantCultureIgnoreCase));
            if (fields.Count <= 0)
                return;

            // clean up (if initial server response specifies "Proxy-Connection: Close" then stream cannot be re-used)
            string field = initialResponse.Find(str => str.StartsWith("Proxy-Connection: Close", StringComparison.InvariantCultureIgnoreCase));
            if (!string.IsNullOrEmpty(field))
            {
                stream.Close();
                Uri proxyURI = proxy.GetProxy(uri);
                stream = ConnectSocket(proxyURI, nodelay, timeoutMs);
            }

            if (proxy.Credentials == null)
                throw new BadServerResponseException(string.Format("Received error code {0} from the server", initialResponse[0]));

            NetworkCredential credentials = proxy.Credentials.GetCredential(uri, null);

            string basicField = fields.Find(str => str.StartsWith("Proxy-Authenticate: Basic", StringComparison.InvariantCultureIgnoreCase));
            var digestFields = fields.FindAll(str => str.StartsWith("Proxy-Authenticate: Digest", StringComparison.InvariantCultureIgnoreCase));

            if (CurrentProxyAuthenticationMethod == ProxyAuthenticationMethod.Basic)
            {
                if (string.IsNullOrEmpty(basicField))
                    throw new ProxyServerAuthenticationException("Basic authentication scheme is not supported/enabled by the proxy server.");

                string authenticationFieldReply = string.Format("Proxy-Authorization: Basic {0}",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials.UserName + ":" + credentials.Password)));
                WriteLine(header, stream);
                WriteLine(authenticationFieldReply, stream);
                WriteLine(stream);
            }
            else if (CurrentProxyAuthenticationMethod == ProxyAuthenticationMethod.Digest)
            {
                var digestField = digestFields.FirstOrDefault(f => f.ToLowerInvariant().Contains("sha-256")) ?? digestFields.FirstOrDefault();

                if (string.IsNullOrEmpty(digestField))
                    throw new ProxyServerAuthenticationException("Digest authentication scheme is not supported/enabled by the proxy server.");

                string authenticationFieldReply = string.Format(
                    "Proxy-Authorization: Digest username=\"{0}\", uri=\"{1}:{2}\"",
                    credentials.UserName, uri.Host, uri.Port);

                int len = "Proxy-Authorization: Digest".Length;
                string directiveString = digestField.Substring(len, digestField.Length - len);
                string[] directives = directiveString.Split(new[] {", ", "\""}, StringSplitOptions.RemoveEmptyEntries);

                string algorithm = null; // optional
                string opaque = null;    // optional
                string qop = null;       // optional
                string realm = null;
                string nonce = null;

                for (int i = 0; i < directives.Length; ++i)
                {
                    switch (directives[i].ToLowerInvariant())
                    {
                        case "stale=":
                            if (directives[++i].ToLowerInvariant() == "true")
                                throw new ProxyServerAuthenticationException("Stale nonce in Digest authentication attempt.");
                            break;
                        case "realm=":
                            authenticationFieldReply += string.Format(", realm=\"{0}\"", directives[++i]);
                            realm = directives[i];
                            break;
                        case "nonce=":
                            authenticationFieldReply += string.Format(", nonce=\"{0}\"", directives[++i]);
                            nonce = directives[i];
                            break;
                        case "opaque=":
                            authenticationFieldReply += string.Format(", opaque=\"{0}\"", directives[++i]);
                            opaque = directives[i];
                            break;
                        case "algorithm=":
                            authenticationFieldReply += string.Format(", algorithm={0}", directives[++i]); //unquoted; see RFC7616-3.4
                            algorithm = directives[i];
                            break;
                        case "qop=":
                            var qops = directives[++i].Split(',');
                            if (qops.Length > 0)
                            {
                                qop = qops.FirstOrDefault(q => q.ToLowerInvariant() == "auth") ??
                                      qops.FirstOrDefault(q => q.ToLowerInvariant() == "auth-int");
                                if (qop == null)
                                      throw new ProxyServerAuthenticationException(
                                          "Digest authentication's quality-of-protection directive is not supported.");
                                authenticationFieldReply += string.Format(", qop={0}", qop); //unquoted; see RFC7616-3.4
                            }
                            break;
                    }
                }

                string clientNonce = GenerateNonce();
                if (qop != null)
                    authenticationFieldReply += string.Format(", cnonce=\"{0}\"", clientNonce);

                string nonceCount = "00000001"; // todo: track nonces and their corresponding nonce counts
                if (qop != null)
                    authenticationFieldReply += string.Format(", nc={0}", nonceCount); //unquoted; see RFC7616-3.4

                Func<string, string> algFunc;
                var scratch1 = string.Join(":", credentials.UserName, realm, credentials.Password);
                string HA1;
                algorithm = algorithm ?? "md5";

                switch (algorithm.ToLowerInvariant())
                {
                    case "sha-256-sess":
                        algFunc = Sha256Hash;
                        HA1 = algFunc(string.Join(":", algFunc(scratch1), nonce, clientNonce));
                        break;
                    case "md5-sess":
                        algFunc = _MD5Hash;
                        HA1 = algFunc(string.Join(":", algFunc(scratch1), nonce, clientNonce));
                        break;
                    case "sha-256":
                        algFunc = Sha256Hash;
                        HA1 = algFunc(scratch1);
                        break;
                    case "md5":
                    default:
                        algFunc = _MD5Hash;
                        HA1 = algFunc(scratch1);
                        break;
                }

                var scratch2 = string.Join(":", GetPartOrNull(header, 0) ?? "CONNECT", uri.Host, uri.Port);
                string HA2 = qop == null || qop.ToLowerInvariant() == "auth"
                    ? algFunc(scratch2)
                    : algFunc(string.Join(":", scratch2, algFunc(initialResponse[initialResponse.Count - 1])));

                string[] array3 = qop == null
                    ? new[] {HA1, nonce, HA2}
                    : new[] {HA1, nonce, nonceCount, clientNonce, qop, HA2};
                var response = algFunc(string.Join(":", array3));

                authenticationFieldReply += string.Format(", response=\"{0}\"", response);

                WriteLine(header, stream);
                WriteLine(authenticationFieldReply, stream);
                WriteLine(stream);
            }
            else
            {
                string authType = GetPartOrNull(fields[0], 1);
                throw new ProxyServerAuthenticationException(
                    string.Format("Proxy server's {0} authentication method is not supported.", authType ?? "chosen"));
            }

            // handle authentication attempt response
            List<string> authenticatedResponse = new List<string>();
            ReadHttpHeaders(ref stream, proxy, nodelay, timeoutMs, authenticatedResponse);
            if (authenticatedResponse.Count == 0)
                throw new BadServerResponseException("No response from the proxy server after authentication attempt.");

            switch (getResultCode(authenticatedResponse[0]))
            {
                case 200:
                    break;
                case 407:
                    throw new ProxyServerAuthenticationException("Proxy server denied access due to wrong credentials.");
                default:
                    throw new BadServerResponseException(string.Format(
                        "Received error code {0} from the server", authenticatedResponse[0]));
            }
        }


        private static Stream DoHttp(Uri uri, IWebProxy proxy, bool nodelay, int timeout_ms, params string[] headers)
        {
            Stream stream = ConnectStream(uri, proxy, nodelay, timeout_ms);

            int redirects = 0;

            do
            {
                if (redirects > MAX_REDIRECTS)
                    throw new TooManyRedirectsException(redirects, uri);

                redirects++;

                foreach (string header in headers)
                    WriteLine(header, stream);
                WriteLine(stream);

                stream.Flush();
            }
            while (ReadHttpHeaders(ref stream, proxy, nodelay, timeout_ms));

            return stream;
        }

        /// <summary>
        /// Adds HTTP CONNECT headers returning the stream ready for use
        /// </summary>
        public static Stream HttpConnectStream(Uri uri, IWebProxy proxy, String session, int timeoutMs)
        {
            return DoHttp(uri, proxy, true, timeoutMs,
                string.Format("CONNECT {0} HTTP/1.0", uri.PathAndQuery),
                string.Format("Host: {0}", uri.Host),
                string.Format("Cookie: session_id={0}", session));
        }

        /// <summary>
        /// Adds HTTP PUT headers returning the stream ready for use
        /// </summary>
        public static Stream HttpPutStream(Uri uri, IWebProxy proxy, long contentLength, int timeoutMs)
        {
            return DoHttp(uri, proxy, false, timeoutMs,
                string.Format("PUT {0} HTTP/1.0", uri.PathAndQuery),
                string.Format("Host: {0}", uri.Host),
                string.Format("Content-Length: {0}", contentLength));
        }

        /// <summary>
        /// Adds HTTP GET headers returning the stream ready for use
        /// </summary>
        public static Stream HttpGetStream(Uri uri, IWebProxy proxy, int timeoutMs)
        {
            return DoHttp(uri, proxy, false, timeoutMs,
                string.Format("GET {0} HTTP/1.0", uri.PathAndQuery),
                string.Format("Host: {0}", uri.Host));
        }


        /// <summary>
        /// A general HTTP PUT method, with delegates for progress and cancelling. May throw various exceptions.
        /// </summary>
        /// <param name="progressDelegate">Delegate called periodically (500ms) with percent complete</param>
        /// <param name="cancellingDelegate">Delegate called periodically to see if need to cancel</param>
        /// <param name="uri">URI to PUT to</param>
        /// <param name="proxy">A proxy to handle the HTTP connection</param>
        /// <param name="path">Path to file to put</param>
        /// <param name="timeoutMs">Timeout for the connection in ms. 0 for no timeout.</param>
        public static void Put(UpdateProgressDelegate progressDelegate, FuncBool cancellingDelegate,
            Uri uri, IWebProxy proxy, string path, int timeoutMs)
        {
            using (Stream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read),
                requestStream = HttpPutStream(uri, proxy, fileStream.Length, timeoutMs))
            {
                long len = fileStream.Length;
                DataCopiedDelegate dataCopiedDelegate = delegate(long bytes)
                    {
                        if (progressDelegate != null && len > 0)
                            progressDelegate((int)((bytes * 100) / len));
                    };

                CopyStream(fileStream, requestStream, dataCopiedDelegate, cancellingDelegate);
            }
        }

        /// <summary>
        /// A general HTTP GET method, with delegates for progress and cancelling. May throw various exceptions.
        /// </summary>
        /// <param name="dataCopiedDelegate">Delegate called periodically (500 ms) with the number of bytes transferred</param>
        /// <param name="cancellingDelegate">Delegate called periodically to see if need to cancel</param>
        /// <param name="uri">URI to GET from</param>
        /// <param name="proxy">A proxy to handle the HTTP connection</param>
        /// <param name="path">Path to file to receive the data</param>
        /// <param name="timeoutMs">Timeout for the connection in ms. 0 for no timeout.</param>
        public static void Get(DataCopiedDelegate dataCopiedDelegate, FuncBool cancellingDelegate,
            Uri uri, IWebProxy proxy, string path, int timeoutMs)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException(nameof(path));

            var tmpFile = Path.GetTempFileName();

            if (Path.GetPathRoot(path) != Path.GetPathRoot(tmpFile))
            {
                //CA-365905: if the target path is under a root different from
                //the temp file, use instead a temp file under the target root,
                //otherwise there may not be enough space for the download

                var dir = Path.GetDirectoryName(path);
                if (dir == null) //path is root directory
                    throw new ArgumentException(nameof(path));

                tmpFile = Path.Combine(dir, Path.GetRandomFileName());
                File.Delete(tmpFile);
            }

            try
            {
                using (Stream fileStream = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.None),
                       downloadStream = HttpGetStream(uri, proxy, timeoutMs))
                {
                    CopyStream(downloadStream, fileStream, dataCopiedDelegate, cancellingDelegate);
                    fileStream.Flush();
                }

                File.Delete(path);
                MoveFileWithRetry(tmpFile, path);
            }
            finally
            {
                File.Delete(tmpFile);
            }
        }

        private const int FILE_MOVE_MAX_RETRIES = 5;
        private const int FILE_MOVE_SLEEP_BETWEEN_RETRIES = 100;

        /// <summary>
        /// Move a file, retrying a few times with a short sleep between retries.
        /// If it still fails after these retries, then throw the error.
        /// </summary>
        public static void MoveFileWithRetry(string sourceFileName, string destFileName)
        {
            int retriesRemaining = FILE_MOVE_MAX_RETRIES;
            do
            {
                try
                {
                    File.Move(sourceFileName, destFileName);
                    break;
                }
                catch (IOException)
                {
                    if (retriesRemaining <= 0)
                        throw;
                    System.Threading.Thread.Sleep(FILE_MOVE_SLEEP_BETWEEN_RETRIES);
                }
            } while (retriesRemaining-- > 0);
        }
    }
}
