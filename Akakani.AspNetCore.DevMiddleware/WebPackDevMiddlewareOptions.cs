
namespace Akakani.AspNetCore.DevMiddleware
{
    using System.Collections.Generic;

    /// <summary>
    /// Options for configuring a Webpack dev middleware compiler.
    /// </summary>
    public class WebpackDevMiddlewareOptions
    {
        /// <summary>
        /// Enables hot module replacement (HMR). If not set, defaults to "false".
        /// </summary>
        public bool HotModuleReplacement { get; set; }

        /// <summary>
        /// Specifies the URL that Webpack's client-side code will connect to when listening for updates.
        /// This must be a root-relative URL similar to "/__webpack_hmr". If not set, defaults to "/__webpack_hmr".
        /// </summary>
        public string HotModuleReplacementEndpoint { get; set; }

        /// <summary>
        /// Specifies the port number that client-side HMR code will connect to. If not set, defaults to 8080.
        /// </summary>
        public int HotModuleReplacementServerPort { get; set; }

        /// <summary>
        /// Specifies the Webpack configuration file to be used. If not set, defaults to "webpack.config.js".
        /// </summary>
        public string ConfigFile { get; set; }

        /// <summary>
        /// Specifies additional environment variables to be passed to the Node instance hosting the webpack compiler.
        /// </summary>
        public IDictionary<string, string> EnvironmentVariables { get; set; }
    }
}
