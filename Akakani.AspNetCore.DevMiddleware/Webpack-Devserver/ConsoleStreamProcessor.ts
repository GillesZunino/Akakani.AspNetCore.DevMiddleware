import Logger from "./Logger";

export default class ConsoleStreamProcessor {

    private static readonly FindNewlinesRegex: RegExp = /\n(?!$)/g;
    private static readonly EncodedNewline: string = "__awpdm_nl__";

    public constructor(process: NodeJS.Process) {
        // Intercept writes to stdErr and stdOut and replace line breaks with a marker token
        // .NET code converts the token back to Environment.NewLine and logs to ILogger
        // This preserves multiline log entries as multiline instead of multiple independent log messages
        this.encodeNewlines(process.stdout, Logger.InformationPostfix);
        this.encodeNewlines(process.stderr, Logger.ErrorPostfix);
    }

    private encodeNewlines(outputStream: NodeJS.WritableStream, postfix: string): void {
        const origWriteFunction = outputStream.write;
        outputStream.write = <any> function (this: any, value: any) {
            if (typeof value === "string") {
                const argsClone = Array.prototype.slice.call(arguments, 0);
                if (!value.startsWith(Logger.LogPrefix)) {
                    value = `${Logger.LogPrefix}${postfix}${value}`;
                }
                argsClone[0] = value.replace(ConsoleStreamProcessor.FindNewlinesRegex, ConsoleStreamProcessor.EncodedNewline);
                origWriteFunction.apply(this, argsClone);
            } else {
                origWriteFunction.apply(this, arguments);
            }
        };
    }
}