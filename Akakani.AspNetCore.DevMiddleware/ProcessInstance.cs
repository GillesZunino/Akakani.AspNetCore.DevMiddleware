namespace Akakani.AspNetCore.DevMiddleware
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;


    internal class ProcessInstance : IDisposable
    {
        private bool disposed;
        private Process spawnedProcess;

        private CancellationToken terminateProcessCancellationToken;
        private CancellationTokenRegistration stopProcessCancellationRegistration;
        private TaskCompletionSource<int> processCompletedCompletionSource;

        protected readonly ILogger Logger;


        public ProcessInstance(ILogger logger)
        {
            Logger = logger;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ProcessInstance()
        {
            Dispose(false);
        }

        protected Task<int> LaunchAsync(string command, string commandLineArguments, string workingDirectory, IDictionary<string, string> environmentVariables, CancellationToken cancellationToken)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("ProcessInstance");
            }

            if ((spawnedProcess != null) || (processCompletedCompletionSource != null))
            {
                throw new InvalidOperationException("A process has already been spawned. To spawn another process, create a new instance of 'ProcessInstance'");
            }

            // Get process start information
            processCompletedCompletionSource = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            ProcessStartInfo processStartInfo = GetProcessStartInfo(command, commandLineArguments, workingDirectory, environmentVariables);

            // Register a callback to be called should cancellation be requested
            terminateProcessCancellationToken = cancellationToken;
            stopProcessCancellationRegistration = cancellationToken.Register(TerminateProcess);

            // Start the process
            LogProcessStarting(processStartInfo);
            spawnedProcess = Process.Start(processStartInfo);
            LogProcessStarted(spawnedProcess);

            // Observe the state of the process
            spawnedProcess.EnableRaisingEvents = true;
            spawnedProcess.Exited += OnProcessExited;

            // Listen to stdout / stderr
            ConnectToProcessStreams(spawnedProcess);

            return processCompletedCompletionSource.Task;
        }

        protected virtual ProcessStartInfo GetProcessStartInfo(string command, string commandLineArguments, string workingDirectory, IDictionary<string, string> environmentVariables)
        {
            // Craft a new ProcessStartInfo
            ProcessStartInfo startInfo = new ProcessStartInfo(command)
            {
                Arguments = commandLineArguments,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = ProcessWindowStyle.Normal,
                WorkingDirectory = workingDirectory
            };

            // Append environment variables
            if (environmentVariables != null)
            {
                foreach (string variableName in environmentVariables.Keys)
                {
                    string variableValue = environmentVariables[variableName];
                    if (variableValue != null)
                    {
                        startInfo.Environment[variableName] = variableValue;
                    }
                }
            }

            return startInfo;
        }

        protected virtual void OnOutputDataReceived(string outputData)
        {
            Logger.LogInformation(outputData);
        }

        protected virtual void OnErrorDataReceived(string errorData)
        {
            Logger.LogError(errorData);
        }

        private void ConnectToProcessStreams(Process process)
        {
            if (process != null)
            {
                process.OutputDataReceived += OnStdOutputDataReceived;
                process.ErrorDataReceived += OnStdErrorDataReceived;

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
        }

        private void DisconnectFromProcessStreams(Process process)
        {
            if (process != null)
            {
                process.OutputDataReceived -= OnStdOutputDataReceived;
                process.ErrorDataReceived -= OnStdErrorDataReceived;

                process.CancelOutputRead();
                process.CancelErrorRead();
            }
        }

        private void OnStdOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            OnOutputDataReceived(e.Data);
        }

        private void OnStdErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            OnErrorDataReceived(e.Data);
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            Process process = sender as Process;

            LogProcessExited(process);

            // Propagate status to the task we handed off when the process started
            if (terminateProcessCancellationToken.IsCancellationRequested)
            {
                processCompletedCompletionSource.TrySetCanceled(stopProcessCancellationRegistration.Token);
            }
            else
            {
                processCompletedCompletionSource.TrySetResult(process.ExitCode);
            }
        }

        private void TerminateProcess()
        {
            try
            {
                if ((spawnedProcess != null) && !spawnedProcess.HasExited)
                {
                    spawnedProcess.Kill(true);
                }
            }
            catch (Exception ex)
            {
                // TODO
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (spawnedProcess != null)
                    {
                        // Make sure the Node process is finished
                        TerminateProcess();

                        // Detach events
                        DisconnectFromProcessStreams(spawnedProcess);
                        spawnedProcess.Exited -= OnProcessExited;

                        // Release resources
                        spawnedProcess.Dispose();
                        spawnedProcess = null;
                    }

                    // Cleanup cancellation registration
                    stopProcessCancellationRegistration.Unregister();
                    stopProcessCancellationRegistration.Dispose();

                    // Detach from the task completion source
                    processCompletedCompletionSource = null;
                }

                disposed = true;
            }
        }

        private void LogProcessStarting(ProcessStartInfo processStartInfo)
        {
            Logger.LogInformation("Working directory '{0}'", processStartInfo.WorkingDirectory);
            Logger.LogInformation("Spawning '{0} {1}'", processStartInfo.FileName, processStartInfo.Arguments);
        }

        private void LogProcessStarted(Process process)
        {
            Logger.LogInformation("Spawned process - pid:{0} - HasExited: {1}", process.Id, process.HasExited);
        }

        private void LogProcessExited(Process process)
        {
            Logger.LogInformation("Process '{0} {1}' has exited - pid:{2} - ExitCode: {3}", process.StartInfo.FileName, process.StartInfo.Arguments, process.Id, process.ExitCode);
        }
    }
}
