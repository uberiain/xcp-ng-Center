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
using XenCenterLib;
using XenCenterLib.Archive;


namespace XenAdmin.Actions
{
    public class ZipStatusReportAction : StatusReportAction
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The folder containing the raw files as downloaded from the server
        /// </summary>
        private readonly string _inputTempFolder;

        /// <summary>
        /// Temporary folder in which we assemble the log files from the server
        /// before repackaging them in a single zip file.
        /// </summary>
        private string _extractTempDir;

        /// <summary>
        /// The destination zip file for the repackaged server log files
        /// </summary>
        private readonly string _destFile;

        /// <summary>
        ///  Creates a new instance of the action for zipping downloaded server log files
        /// </summary>
        /// <param name="tempFolder">Temporary folder to store the downloaded logs</param>
        /// <param name="destFile">The target file to store the compressed result</param>
        /// <param name="timeString">Time string used when running action as <see cref="StatusReportAction"/>. Can be omitted otherwise.</param>
        /// <param name="suppressHistory">Whether to suppress history in the Events TabPage</param>
        public ZipStatusReportAction(string tempFolder, string destFile, string timeString = null, bool suppressHistory = true)
            : base(null, Messages.BUGTOOL_SAVING, destFile, timeString, suppressHistory)
        {
            _inputTempFolder = StringUtility.ToLongWindowsPathUnchecked(tempFolder);
            _destFile = destFile;
        }

        protected override void Run()
        {
            Status = ReportStatus.inProgress;
            do
            {
                _extractTempDir = StringUtility.ToLongWindowsPathUnchecked(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            } while (Directory.Exists(_extractTempDir));

            Directory.CreateDirectory(_extractTempDir);

            try
            {
                // Calculate total bytes to save
                long bytesToCompress = 0, bytesToExtract = 0, bytesExtracted = 0;

                var files = Directory.GetFiles(_inputTempFolder);

                foreach (string inputFile in files)
                    bytesToExtract += new FileInfo(inputFile).Length;

                foreach (string inputFile in files)
                {
                    if (inputFile.ToLowerInvariant().EndsWith(".tar"))
                    {
                        // Sanitize and un-tar each of the raw server tars to the temp extraction directory

                        string outFilename = inputFile.Substring(0, inputFile.Length - 4);
                        if (outFilename.Length == 0)
                            outFilename = Path.GetRandomFileName();
                        string outputDir = Path.Combine(_extractTempDir, Path.GetFileName(outFilename));

                        string sanitizedTar = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                        TarSanitization.SanitizeTarForWindows(inputFile, sanitizedTar, CheckCancellation);

                        using (FileStream fs = File.OpenRead(sanitizedTar))
                        using (ArchiveIterator tarIterator = ArchiveFactory.Reader(ArchiveFactory.Type.Tar, fs))
                        {
                            Directory.CreateDirectory(outputDir);
                            tarIterator.ExtractAllContents(outputDir);
                            bytesToCompress += Core.Helpers.GetDirSize(new DirectoryInfo(outputDir));
                        }

                        try
                        {
                            File.Delete(sanitizedTar);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                    else
                    {
                        // Just copy vanilla input files unmodified to the temp directory
                        string outputFile = Path.Combine(_extractTempDir, Path.GetFileName(inputFile));
                        File.Copy(inputFile, outputFile);
                        bytesToCompress += new FileInfo(outputFile).Length;
                    }

                    bytesExtracted += new FileInfo(inputFile).Length;
                    File.Delete(inputFile);
                    PercentComplete = (int)(50.0 * bytesExtracted / bytesToExtract);
                    CheckCancellation();
                }

                // Now zip up all the temporarily extracted files into a single zip file for the user
                log.DebugFormat("Packing {0} of bug report files into zip file {1}", 
                    Util.DiskSizeString(bytesToCompress), _destFile);

                ZipToOutputFile(_extractTempDir, CheckCancellation, p => PercentComplete = 50 + p / 2);
                CleanupFiles();
                PercentComplete = 100;
            }
            catch (CancelledException)
            {
                CleanupFiles(true);
                log.Info("Packaging system status cancelled");
                Tick(100, Messages.ACTION_SAVE_STATUS_REPORT_CANCELLED);
                Status = ReportStatus.cancelled;
                throw;
            }
            catch (Exception exn)
            {
                try
                {
                    log.Error("Failed to package sanitized server status report: ", exn);
                    log.Info("Attempting to package raw downloaded server files.");
                    ZipToOutputFile(_inputTempFolder, CheckCancellation);
                }
                catch (CancelledException)
                {
                    log.Info("Packaging raw downloaded server cancelled");
                    Tick(100, Messages.ACTION_SAVE_STATUS_REPORT_CANCELLED);
                    Status = ReportStatus.cancelled;
                    CleanupFiles(true);
                    throw;
                }
                catch (Exception exception)
                {
                    log.Error("Failed to package raw downloaded server files.", exception);
                }

                Tick(100, Messages.ACTION_SAVE_STATUS_REPORT_FAILED);
                Status = ReportStatus.failed;
                throw new Exception(Messages.ACTION_SAVE_STATUS_REPORT_FAILED_PARTIAL);
            }

            Tick(100, Messages.COMPLETED);
            Status = ReportStatus.succeeded;
        }

        private void CheckCancellation()
        {
            if (Cancelling)
                throw new CancelledException();
        }

        private void ZipToOutputFile(string folderToZip, Action cancellingDelegate = null, Action<int> progressDelegate = null)
        {
            using (ArchiveWriter zip = ArchiveFactory.Writer(ArchiveFactory.Type.Zip, File.OpenWrite(_destFile)))
                zip.CreateArchive(folderToZip, cancellingDelegate, progressDelegate);
        }

        private void CleanupFiles(bool deleteDestFile = false)
        {
            try
            {
                log.Debug("Deleting temporary directory with raw downloaded server files");
                Directory.Delete(_inputTempFolder, true);
            }
            catch (Exception exn)
            {
                log.Warn("Could not delete temporary directory with raw downloaded server files", exn);
            }

            try
            {
                log.Debug("Deleting directory with temporarily extracted files");
                Directory.Delete(_extractTempDir, true);
            }
            catch (Exception exn)
            {
                log.Warn("Could not delete directory with temporarily extracted files", exn);
            }

            try
            {
                if (deleteDestFile)
                {
                    log.Debug("Deleting destination zip file");
                    File.Delete(_destFile);
                }
            }
            catch (Exception ex)
            {
                log.Warn("Could not delete destination zip file", ex);
            }
        }

        public override void RecomputeCanCancel()
        {
            CanCancel = !Cancelling && !IsCompleted;
        }

        protected override void CancelRelatedTask()
        {
        }
    }
}
