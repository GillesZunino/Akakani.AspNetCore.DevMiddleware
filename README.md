# Webpack Development Middleware for ASP.NET Core

Streamline development workflow by triggering [Webpack](https://webpack.js.org/) builds on demand. This middleware is a community supported replacement of Microsoft's deprecated [Webpack Dev Middleware](https://docs.microsoft.com/en-us/aspnet/core/client-side/spa-services?view=aspnetcore-2.2#webpack-dev-middleware).

Webpack Development Middleware automatically compiles and serves client-side resources when a page is reloaded in the browser. The alternate approach is to manually invoke Webpack via the project's npm build script when a third-party dependency or the custom code changes.

Optionally, enable Webpack's [Hot Module Replacement (HMR)](https://webpack.js.org/concepts/hot-module-replacement/) which further streamlines the development workflow by automatically updating page content after compiling the changes. Changes to client-side resources are pushed to the browser via a live link between the Webpack Dev Middleware and the browser. The alternate approach is to manually refresh the web browser to apply changes and loose in memory state and debugging session.


# Pre-requisites
Webpack Development Middleware requires:

* [Webpack](https://webpack.js.org/) 4 or above,
* [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core) 3.1 or above.

# Installation

1. Install Nuget package `Akakani.AspNetCore.WebpackDevMiddleware` to the ASP.NET Core project.
2. In `Startup.cs` configure Webpack Development Middleware. Optionally, specify the relative path to the Webpack configuration file:

    ```C#
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions() {
            ConfigFile = "webpack.dev.config.js" // Optional, defaults to "webpack.config.js"
        });
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
    }

    // Call UseWebpackDevMiddleware before UseStaticFiles
    app.UseStaticFiles();
    ```

    The `UseWebpackDevMiddleware` extension method must be called before registering static file hosting via the `UseStaticFiles` extension method. For security reasons, **only register the middleware when the app runs in development mode**.

## TODO: Point to an example to see it in action. Possibly add a video.


# Hot Module Replacement (HMR)
1. Install `webpack-dev-middleware` and `webpack-hot-middleware` as development dependencies:

    ```shell
    npm i -D webpack-dev-middleware webpack-hot-middleware
    ```
2. Webpack 5 _**only**_ : when using [mini-css-extract-plugin](https://github.com/webpack-contrib/mini-css-extract-plugin), disable caching in Webpack configuration via `cache: false` as follows:

    ```js
    const isDevelopment = process.env.NODE_ENV !== "production";
    
    ...

    module.exports (env) => {
        mode: isDevelopment ? "development" : "production",
        devtool: isDevelopment ? "source-map" : "",
        target: "web",

        // Webpack 5 only: HMR with MiniCssExtractPlugin currently requires the cache to be disabled
        cache: isDevelopment ? false : undefined,

        ...
    }
    ```

3. In Webpack configuration, set `output.publicPath` to the path the middleware watches for changes. In the following example, the middleware responds to HMR requests for the `dist` folder:

    ```js
    module.exports = (env) => {
        ...

        output: {
            filename: '[name].js',
            publicPath: '/dist' // When HMR is enabled, Webpack dev middleware handles requests for this URL prefix
        }

        ...
    }
    ```
    

4. In `Startup.cs`, add `HotModuleReplacement = true` to `WebpackDevMiddlewareOptions` as follows:

    ```C#
    app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions {
        ConfigFile = "webpack.dev.config.js", // Optional, defaults to "webpack.config.js"
        HotModuleReplacement = true
    });
    ```

5. TODO: Explain how to accept HMR in code - Advanced Scenarios

6. TODO: how how to start the projet
## TODO: Typescript HMR
install `@types/webpack-env` to get typings for HTM (*module.hot, accept() ..)

## TODO: Point to an example to see it in action. Possibly add a video.

# Advanced Confguration
TODO: Describe passing properties to Webpack, enabling debugging support...

# Samples
This repo maintains various samples:
* [Simple](https://github.com/GillesZunino/Akakani.AspNetCore.DevMiddleware/tree/master/samples/simple): Basic ASP.NET Core application with TypeScript, LESS / CSS. Webpack 4 and 5 configuration demonstrates HMR for TypeScript and CSS extracted via [mini-css-extract-plugin](https://github.com/webpack-contrib/mini-css-extract-plugin).


# Authoring Webpack configuration with TypeScript
TODO: Explain how to do this


# Future Improvements

* No support for React (yet),
* No support for Vue.js (yet),
* Webpack 5 HMR with MinCssExtractPlugin currently requires `cache: false` in Webpack configuration.

# Acknowledgements
Webpack Development Middleware for ASP.NET Core is inspired by Microsoft's [ASP.NET Core JavaScript Services](https://github.com/aspnet/JavaScriptServices/blob/master/LICENSE.txt). JavaScript Services [were made obsolete](https://github.com/dotnet/aspnetcore/issues/12890) in the ASP.NET Core 2.2 release. The content of this repo has been inspired by ASP.NET Core JavaScript Services which was released under the Apache 2.0 license. A copy of this license can be found [here](https://github.com/aspnet/JavaScriptServices/blob/master/LICENSE.txt).


# Resources
* Generate Webpack configuration in UI - [Create App](https://createapp.dev/webpack),
* A mixed TypeScript/JavaScript Webpack boilerplate with Express [mixpack](https://github.com/waldronmatt/mixpack),
