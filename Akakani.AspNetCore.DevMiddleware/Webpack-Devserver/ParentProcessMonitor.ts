import Logger from "./Logger";


export interface ParentMonitorOptions {
    parentPid: number;
    ignoreSIGINT: boolean;
    pollIntervalMs: number;
}


//
// In general, we want the Node child processes to be terminated as soon as the parent .NET processes exits, because we have no further use for them.
//
// * - If the .NET process shuts down gracefully, it will run its finalizers and the associated Node process will be killed.
// * - If the .NET process is terminated forcefully (e.g., on Linux/OSX with 'kill -9'), then it won't have any opportunity to shut down its child processes, and by default they will keep running.
//      In this case, it's up to the child process to detect this has happened and terminate itself.
//
// There are many possible approaches to detecting when a parent process has exited. The only documented as a valid strategy, cross-platform approach is to poll to check whether the parent PID is still running.
//

export class ParentProcessMonitor {

    private readonly options: ParentMonitorOptions;
    private readonly logger: Logger;

    private monitorSetIntervalCookie: NodeJS.Timeout | undefined;

    public constructor(options: ParentMonitorOptions, logger: Logger) {
        this.options = options;
        this.logger = logger;

        this.exitWhenParentExits();
    }

    public exitWhenParentExits(): void {
        if (this.options.parentPid) {
            this.monitorSetIntervalCookie = setInterval((): void => {
                if (!this.processExists(this.options.parentPid)) {
                    if (this.monitorSetIntervalCookie !== undefined) {
                        clearInterval(this.monitorSetIntervalCookie);
                    }
                    
                    process.exit();
                }
            }, this.options.pollIntervalMs);

            if (this.options.ignoreSIGINT) {
                //
                // Pressing Ctrl+C in the terminal sends a SIGINT to all processes in the foreground process tree.
                // By default, the Node process would then exit before the .NET process, because ASP.NET implements
                // a delayed shutdown to allow ongoing requests to complete.
                //
                // This is problematic, because if Node exits first, the CopyToAsync code in ConditionalProxyMiddleware
                // will experience a read fault, and logs a huge load of errors. Fortunately, since the Node process is
                // already set up to shut itself down if it detects the .NET process is terminated, all we have to do is
                // ignore the SIGINT. The Node process will then terminate automatically after the .NET process does.
                //
                process.on("SIGINT", (): void => {
                    console.log("Received SIGINT. Waiting for .NET process to exit...");
                    this.logger.information("Received SIGINT. Ignoring until the .NET process exists");
                });
            }
        }
    }

    private processExists(processId: number): boolean {
        try {
            // Sending signal 0 (on all platforms) tests whether the process exists. As long as it doesn't throw, that means it does exist
            process.kill(processId, 0);
            return true;
        } catch (ex) {
            // If the reason for the error is that we don't have permission to ask about this process, report that as a separate problem
            if (ex.code === "EPERM") {
                throw new Error(`Attempted to check whether process ${processId} was running, but got a permissions error.`);
            }
    
            return false;
        }
    }
}
