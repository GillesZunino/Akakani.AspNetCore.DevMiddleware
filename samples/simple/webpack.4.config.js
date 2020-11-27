const isDevelopment = process.env.NODE_ENV !== 'production';
const hotReload = isDevelopment;
const generateSourceMaps = isDevelopment;

const path = require("path");
const webpack = require("webpack");

const MiniCssExtractPlugin = require("mini-css-extract-plugin");

const hotMiddlewareScript = "webpack-hot-middleware/client?path=/__webpack_hmr&timeout=20000&reload=true";


module.exports = {
    mode: isDevelopment ? "development" : "production",
    devtool: isDevelopment ? "inline-source-map" : "",
    target: "web",

    entry: {
        main: [ "./Scripts/Main.ts", hotMiddlewareScript ]
    },

    output: {
        path: path.join(__dirname, 'wwwroot'),
        filename: `js/[name].min.js`,
        publicPath: "/"
    },

    resolve: {
        extensions: [ ".ts", ".js", ".less", ".css" ],
        modules: [
            path.resolve(__dirname, "Scripts"),
            path.resolve(__dirname, "Styles"),
            "node_modules"
        ]
    },

    module: {
        rules: [
            { test: /\.ts$/, loader: "ts-loader" },
            {
                test: /\.css$/,
                use: [
                    hotReload ? { loader: "css-hot-loader" } : null,
                    { loader: MiniCssExtractPlugin.loader, options: {} },
                    { loader: "css-loader", options: { sourceMap: generateSourceMaps } }
                ].filter(element => element !== null)
            },
            {
                test: /\.less$/,
                use: [
                    hotReload ? { loader: "css-hot-loader" } : null,
                    {
                        loader: MiniCssExtractPlugin.loader
                    },
                    {
                        loader: "css-loader",
                        options: {
                            url: false,
                            importLoaders: 1,
                            sourceMap: generateSourceMaps
                        }
                    },
                    { loader: "less-loader", options: { sourceMap: generateSourceMaps } }
                ].filter(element => element !== null)
            }
        ]
    },
    plugins: [
        new webpack.HotModuleReplacementPlugin(),
        new MiniCssExtractPlugin({
            filename: "css/[name].min.css",
            chunkFilename: "[id].min.css"
        })
    ]
};