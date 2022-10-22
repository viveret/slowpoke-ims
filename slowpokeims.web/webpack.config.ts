import { Configuration } from 'webpack';
import { glob } from 'glob';
import path from 'path';
import MiniCssExtractPlugin from 'mini-css-extract-plugin';

const config: Configuration = {
    mode: "development",
    entry: glob.sync('./ts/src/**.ts').reduce(function(obj: any, el: any){
        obj[path.parse(el).name] = el;
        return obj
     },{}),
    module: {
        rules: [
            {
                exclude: /(node_modules)/,
                loader: 'ts-loader',
                test: /\.[tj]sx?$/
            },
            {
                use: [ MiniCssExtractPlugin.loader, 'css-loader' ],
                test: /\.css$/
            },
            {
                test: /\.less$/,
                use: [
                    {
                        loader: MiniCssExtractPlugin.loader,
                    },
                    {
                        loader: "css-loader",
                        options: {
                          sourceMap: true,
                        },
                    },
                    {
                        loader: "less-loader",
                        options: {
                          sourceMap: true,
                        },
                    }
                ],
            }
        ]
    },
    plugins: [
        new MiniCssExtractPlugin({
            filename: "[name].css",
            chunkFilename: "[name].[id].css",
        })
    ],
    resolve: {
        extensions: ['.js', '.jsx', '.ts', '.tsx']
    },
    output: {
        path: path.resolve(__dirname + '/wwwroot/js'),
        filename: "[name].js"
    }
};

export default config;