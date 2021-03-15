namespace Akakani.AspNetCore.DevMiddleware
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;


    internal class NodeJsProcessInstance : ProcessInstance
    {
        public event EventHandler<string> StdOut;
        public event EventHandler<string> StdErr;

        public NodeJsProcessInstance(ILogger logger) : base(logger)
        {
        }

        public Task<int> LaunchNodeJsAsync(string scriptName, string commandLineArguments, string workingDirectory, IDictionary<string, string> environmentVariables, bool enableDebugging, CancellationToken stopProcessCancellationToken)
        {
            string arguments = PrepareNodeJsCommandLine(scriptName, commandLineArguments, enableDebugging);
            IDictionary<string, string> environmentVariablesWithNodeJsPath = AppendNodeJsPath(environmentVariables, workingDirectory);
            return LaunchAsync("node", arguments, workingDirectory, environmentVariablesWithNodeJsPath, stopProcessCancellationToken);
        }

        private static string PrepareNodeJsCommandLine(string scriptName, string commandLineArguments, bool enableDebugging)
        {
            // [--inspect-brk=<port>] "<script name>" [commandLineArguments]
            return FormattableString.Invariant($"{( enableDebugging ? "--inspect-brk " : string.Empty)}\"{scriptName}\" {(!string.IsNullOrEmpty(commandLineArguments) ? commandLineArguments : string.Empty )}");
        }

        private static IDictionary<string, string> AppendNodeJsPath(IDictionary<string, string> environmentVariables, string workingDirectory)
        {
            const string NodePathVariableName = "NODE_PATH";
            const string NodeModulesDirectoryName = "node_modules";

            // Get the current value of NODE_PATH - We first search in the given environment variables list and then in the OS environment variables list
            string nodePath = (environmentVariables != null) && environmentVariables.ContainsKey(NodePathVariableName) ? environmentVariables[NodePathVariableName] : string.Empty;
            nodePath = string.IsNullOrEmpty(nodePath) ? (Environment.GetEnvironmentVariable(NodePathVariableName) ?? string.Empty) : nodePath;

            // Add working directory "node_modules" folder to NODE_PATH
            if (!string.IsNullOrEmpty(nodePath))
            {
                nodePath += Path.PathSeparator;
            }

            // Copy given list of variables and insert NODE_PATH
            IDictionary<string, string> augmentedEnvironmentVariables = new Dictionary<string, string>(environmentVariables, StringComparer.Ordinal);
            augmentedEnvironmentVariables[NodePathVariableName] = nodePath + Path.Combine(workingDirectory, NodeModulesDirectoryName);

            return augmentedEnvironmentVariables;
        }

        protected override void OnOutputDataReceived(string outputData)
        {
            StdOut?.Invoke(this, outputData);
        }

        protected override void OnErrorDataReceived(string errorData)
        {
            StdErr?.Invoke(this, errorData);
        }
    }
}
