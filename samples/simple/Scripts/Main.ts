// Typescript modules
import MiniatureClock from "./MiniatureClock";

// LESS
import "../Styles/main.less";

// CSS
import "../Styles/page.css"


declare global {
    var __MINIATURE_CLOCK__: MiniatureClock | null;
}


// Enable Hot Module Replacement
if (module.hot) {
    // Accept the current module without notification of parents
    module.hot.accept();

    // Register a dispose handler to stop the current miniature clock if needed
    module.hot.dispose((data: any): void => {
        if (window.__MINIATURE_CLOCK__ !== null) {
            window.__MINIATURE_CLOCK__.stop();
            window.__MINIATURE_CLOCK__ = null;
        }

        console.log("Disposed window.__MINIATURE_CLOCK__");
   });
}

// Start a miniature clock
const miniatureClock: MiniatureClock = new MiniatureClock(document.getElementById("clock") as HTMLSpanElement);
miniatureClock.start();

// Stash the miniature clock instance so it can be stopped during Hot Module Replacement
window.__MINIATURE_CLOCK__ = miniatureClock;