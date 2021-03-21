// -----------------------------------------------------------------------------------
// Copyright 2021, AKAKANI.COM
// -----------------------------------------------------------------------------------


"use strict";

const path = require("path");
const webpack = require("webpack");

const WebpackNodeExternals = require("webpack-node-externals");
const { CleanWebpackPlugin } = require("clean-webpack-plugin");


// Common webpack configuration / utilities
const projectConstants = require("./webpack.constants.js");


// Global constants
const MODE_PRODUCTION = "production";
const MODE_DEVELOPMENT = "development";


module.exports = {

    // Global constants
    PRODUCTION: MODE_PRODUCTION,
    DEVELOPMENT: MODE_DEVELOPMENT,

    // Common webpack configuration
    getCommonConfiguration: (type) => {

        const outputSuffix = "min";

        return {
            target: "node",
            entry: {
                main: "./Webpack-Devserver/AspNetWebpackMiddlewareDevServer.ts"
            },
            output: {
                libraryTarget: "commonjs",
                path: path.join(__dirname, `${projectConstants.outputRoot}`),
                filename: `[name].${outputSuffix}.js`
            },
            resolve: {
                extensions: [ ".ts", ".js" ],
                modules: [
                    "node_modules"
                ]
            },
            module: {
                rules: [
                    { test: /\.ts$/i, loader: "ts-loader" }
                ]
            },
            externals: [ WebpackNodeExternals({
                allowlist: [ "finalhandler", "encodeurl", "escape-html", "on-finished", "ee-first", "parseurl", "statuses", "unpipe", "utils-merge", "connect" ]
            }) ],
            plugins: [
                new CleanWebpackPlugin({
                    dry: false,
                    verbose: true,
                    cleanOnceBeforeBuildPatterns: projectConstants.outputPaths.map((value, index, array) => path.join(process.cwd(), `${value}`))
                }),
                
                new webpack.DefinePlugin({
                    DEVELOPMENT: JSON.stringify(type === MODE_DEVELOPMENT),
                    "process.env.NODE_ENV": JSON.stringify(type)
                })
            ]
        };
    }
};