namespace Akakani.AspNetCore.DevMiddleware
{
    using System;
    using System.IO;
    using System.Threading;
    using Microsoft.Extensions.Logging;

    internal sealed class AutoDeleteTemporaryFile : IDisposable
    {
        private readonly ILogger Logger;

        private bool disposed;
        private bool tempFileDeleted;
        private object fileDeletionLock;
        private readonly CancellationTokenRegistration cancellationTokenRegistration;

       
        public readonly string FilePath;


        public AutoDeleteTemporaryFile(string content, CancellationToken cancellationToken, ILogger logger)
        {
            fileDeletionLock = new object();

            Logger = logger;

            // We have two opportunities to cleanup the file: cancelation token event handler and .NET finalizer
            cancellationTokenRegistration = cancellationToken.Register(EnsureTempFileDeleted);

            // Emit the content to the temp file
            FilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            File.WriteAllText(FilePath, content);
            Logger.LogDebug("Created temporary file '{0}'", FilePath);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AutoDeleteTemporaryFile()
        {
            Dispose(false);
        }

        private void EnsureTempFileDeleted()
        {
            try
            {
                lock (fileDeletionLock)
                {
                    if (!tempFileDeleted)
                    {
                        File.Delete(FilePath);
                        tempFileDeleted = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "Could not delete temporary file '{0}'", FilePath);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    cancellationTokenRegistration.Unregister();
                    cancellationTokenRegistration.Dispose();
                }

                EnsureTempFileDeleted();

                disposed = true;
            }
        }
    }
}
