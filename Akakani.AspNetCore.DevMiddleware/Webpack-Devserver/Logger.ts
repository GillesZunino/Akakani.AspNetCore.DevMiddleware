export default class Logger {
    public static readonly LogPrefix: string = "[Akakani.AspNetCore.DevMiddleware.WebpackDevServer]";

    public static readonly ErrorPostfix: string = "[ERR]";
    public static readonly WarningPostfix: string = "[WRN]";
    public static readonly InformationPostfix: string = "[INF]";

    public error(message: string, ...optionalParams: any[]): void {
        console.error.apply(this, [ `${Logger.LogPrefix}${Logger.ErrorPostfix}${message}`, ...optionalParams ]);
    }

    public warning(message?: any, ...optionalParams: any[]): void {
        console.warn.apply(this, [ `${Logger.LogPrefix}${Logger.WarningPostfix}${message}`, ...optionalParams ]);
    }

    public information(message?: any, ...optionalParams: any[]): void {
        console.info.apply(this, [ `${Logger.LogPrefix}${Logger.InformationPostfix}${message}`, ...optionalParams ]);
    }
}