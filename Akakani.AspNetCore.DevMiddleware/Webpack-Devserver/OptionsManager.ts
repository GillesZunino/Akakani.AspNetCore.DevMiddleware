import Logger from "./Logger";
import Dictionary from "./Dictionary";


export interface WebpackDevelopmentServerOptions {
    webPackConfigurationFile: string;
    enableHotModuleReplacement: boolean;
    hotModuleReplacementServerPort: number;
    hotModuleReplacementEndpoint: string;
}


export class OptionsManager {

    private static readonly WebpackDevMiddlewareOptionsEnvironmentName: string = "AWPDS_OPTIONS";

    private readonly logger: Logger;
    private readonly parentPid: number;
    private readonly webpackDevServerOptions: WebpackDevelopmentServerOptions;

    public get parentProcessId(): number {
        return this.parentPid
    }

    public get devServerOptions(): WebpackDevelopmentServerOptions {
        return this.webpackDevServerOptions;
    }

    public constructor(args: string[], env: NodeJS.ProcessEnv, logger: Logger) {
        this.logger = logger;
        
        const parsedArgs: Dictionary<string | undefined> = this.parseArgs(args);
        this.parentPid = this.collectParentPid(parsedArgs);

        this.webpackDevServerOptions = this.collectWebpackDevServerOptions(env);
    }

    private collectWebpackDevServerOptions(env: NodeJS.ProcessEnv): WebpackDevelopmentServerOptions {
        const serializedOptions: string = env[OptionsManager.WebpackDevMiddlewareOptionsEnvironmentName] as string;
        if (serializedOptions) {
            return JSON.parse(serializedOptions);
        } else {
            throw new Error(`Cannot start Webpack dev server: Options variable '${OptionsManager.WebpackDevMiddlewareOptionsEnvironmentName}' was not provided`);
        }
    }

    private collectParentPid(parsedArgs: Dictionary<string | undefined>): number {
        const parentPidKey: string | undefined = parsedArgs.parentPid;
        if (parentPidKey) {
            return parseInt(parentPidKey);
        } else {
            throw new Error("Cannot start Webpack dev server: Parameter '--parentPid' was not provided");
        }
    }

    private parseArgs(args: string[]) : Dictionary<string | undefined> {
        // Very simplistic command line parsing - We want something bare minimal with no external dependencies
        const result: Dictionary<string | undefined> = {};
        let currentKey: string | null = null;
        args.forEach(arg => {
            if (arg.indexOf('--') === 0) {
                const argName = arg.substring(2);
                result[argName] = undefined;
                currentKey = argName;
            } else if (currentKey) {
                result[currentKey] = arg;
                currentKey = null;
            }
        });
    
        return result;
    }
}