﻿/* Copyright (c) Cloud Software Group, Inc. 
 * 
 * Redistribution and use in source and binary forms, 
 * with or without modification, are permitted provided 
 * that the following conditions are met: 
 * 
 * *   Redistributions of source code must retain the above 
 *     copyright notice, this list of conditions and the 
 *     following disclaimer. 
 * *   Redistributions in binary form must reproduce the above 
 *     copyright notice, this list of conditions and the 
 *     following disclaimer in the documentation and/or other 
 *     materials provided with the distribution. 
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND 
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF 
 * SUCH DAMAGE.
 */

using System;
using System.IO;

namespace XenCenterLib.Archive
{
    /// <summary>
    /// A base abstract class to iterate over an archived file type
    /// </summary>
    public abstract class ArchiveIterator : IDisposable
    {
        /// <summary>
        /// Helper function to extract all contents of this iterating class to a path
        /// </summary>
        /// <param name="pathToExtractTo">The path to extract the archive to</param>
        /// <param name="cancellingDelegate"></param>
        /// <exception cref="ArgumentNullException">If null path is passed in</exception>
        /// <exception cref="NullReferenceException">If while combining path and current file name a null arises</exception>
        public void ExtractAllContents(string pathToExtractTo, Action cancellingDelegate = null)
        {
            if (string.IsNullOrEmpty(pathToExtractTo))
                throw new ArgumentNullException();

            while (HasNext())
            {
                //make the path Windows friendly
                var fileName = CurrentFileName();
                var isDirectory = IsDirectory();

                var sanitizedName = fileName.Replace('/', Path.DirectorySeparatorChar);
                var conflatedPath = Path.Combine(pathToExtractTo, sanitizedName);

                var dir = isDirectory ? conflatedPath : Path.GetDirectoryName(conflatedPath);
                dir = StringUtility.ToLongWindowsPath(dir, true);

                //Create directory - empty one will be made too
                Directory.CreateDirectory(dir);

                //If we have a file extract the contents
                if (!isDirectory)
                {
                    conflatedPath = StringUtility.ToLongWindowsPath(conflatedPath, false);

                    using (var fs = File.Create(conflatedPath))
                        ExtractCurrentFile(fs, cancellingDelegate);
                }
            }
        }

        /// <summary>
        /// Hook to allow the base stream to be wrapped by this class's archive mechanism
        /// </summary>
        /// <param name="stream">base stream</param>
        public virtual void SetBaseStream(Stream stream)
        {
            throw new NotImplementedException();
        }

        public virtual bool VerifyCurrentFileAgainstDigest(string algorithmName, byte[] digest)
        {
            throw new NotImplementedException();
        }

        public abstract bool HasNext();
        public abstract void ExtractCurrentFile(Stream extractedFileContents, Action cancellingDelegate);
        public abstract string CurrentFileName();
        public abstract long CurrentFileSize();
        public abstract DateTime CurrentFileModificationTime();
        public abstract bool IsDirectory();

        /// <summary>
        /// Dispose hook - overload and clean up IO
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
