const path = require("path");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");


const isDevelopment = process.env.NODE_ENV !== "production";
const generateSourceMaps = isDevelopment;


module.exports = {
    mode: isDevelopment ? "development" : "production",
    devtool: isDevelopment ? "source-map" : "",
    target: "web",

    entry: {
        main: [ "./Scripts/Main.ts" ]
    },

    output: {
        path: path.join(__dirname, "wwwroot"),
        filename: "js/[name].min.js",
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
            { test: /\.ts$/i, loader: "ts-loader" },
            {
                test: /\.css$/i,
                use: [
                    { loader: MiniCssExtractPlugin.loader, options: {} },
                    { loader: "css-loader", options: { sourceMap: generateSourceMaps } }
                ]
            },
            {
                test: /\.less$/i,
                use: [
                    { loader: MiniCssExtractPlugin.loader, options: {} },
                    { loader: "css-loader", options: { sourceMap: generateSourceMaps } },
                    { loader: "less-loader", options: { sourceMap: generateSourceMaps } }
                ]
            }
        ]
    },
    
    plugins: [
        new MiniCssExtractPlugin({
            filename: "css/[name].min.css",
            chunkFilename: "[id].min.css"
        })
    ]
};