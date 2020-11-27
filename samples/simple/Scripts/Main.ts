// Typescript modules
import MiniatureClock from "./MiniatureClock";

// LESS
import "../Styles/main.less";


// Enable Hot Module Replacement
if (module.hot) {
    // This accepts all changes to the 'Main' module
    module.hot.accept("Main", function () : void {
        console.log("module.hot.accept('Main') called");
    });
}

// Start a miniature clock
const miniatureClock: MiniatureClock = new MiniatureClock(document.getElementById("clock") as HTMLSpanElement);
miniatureClock.run();