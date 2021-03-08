export default class MiniatureClock {
    
    private span: HTMLSpanElement;
    private currentIteration: number = 0;
    private setIntervalId: number = -1;

    public get iteration() : number { return this.currentIteration; }
    public set iteration(iteration: number) {this.currentIteration = iteration; }

    public constructor(clockSpan: HTMLSpanElement) {
        this.span = clockSpan;
    }

    public start(): void {
        this.setIntervalId = window.setInterval(() => {
            this.iteration += 1;
            this.updateClockDisplay();
        }, 1000);
    }

    public stop(): void {
        if (this.setIntervalId !== -1) {
            window.clearInterval(this.setIntervalId);
            this.setIntervalId = -1;
        }

        this.iteration = 0;
    }

    private updateClockDisplay(): void {
        const today: Date = new Date();
        this.span.textContent = `Time: ${today.getHours()}:${today.getMinutes()}:${today.getSeconds()} - Iteration ${this.iteration}`;
    }
}