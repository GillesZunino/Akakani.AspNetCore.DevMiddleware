// Typescript modules
import MiniatureClock from "./MiniatureClock";

// LESS
import "../Styles/main.less";

// CSS
import "../Styles/page.css"


declare global {
    var __MINIATURE_CLOCK__: MiniatureClock | null;
}


// Handle Hot Module Replacement
if (module.hot) {
    // Accept the current module without notification of parents
    module.hot.accept();

    // Register a dispose handler to transer state from running module(s) to module(s) we are hot replacing
    module.hot.dispose((data: any): void => {
        // If we have a miniature clock module running, save its state and dispose it
        if (window.__MINIATURE_CLOCK__ !== null) {
            // Save miniature clock state
            console.log("Saving window.__MINIATURE_CLOCK__ state");
            data.miniatureClock = { 
                currentIteration: window.__MINIATURE_CLOCK__.iteration
            };

            // Dispose miniature clock
            window.__MINIATURE_CLOCK__.stop();
            window.__MINIATURE_CLOCK__ = null;

            console.log("Disposed window.__MINIATURE_CLOCK__");
        }
   });
}


// Create a new miniature clock
const miniatureClock: MiniatureClock = new MiniatureClock(document.getElementById("clock") as HTMLSpanElement);

// Stash the miniature clock instance so its state can be saved and it can be disposed during Hot Module Replacement
window.__MINIATURE_CLOCK__ = miniatureClock;

// Apply Hot Module Replacement saved miniature clock state, if available
if (module.hot?.data?.miniatureClock) {
    console.log("Restoring window.__MINIATURE_CLOCK__ state");
    miniatureClock.iteration = module.hot?.data?.miniatureClock.currentIteration;
}

// Start the miniature clock
miniatureClock.start();