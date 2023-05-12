using System.Threading.Tasks;

namespace BazzaGibbs.GameSceneManagement {
    public interface IGameLoadingScreen {
        /// <summary>
        /// Async operation to begin the loading screen animation.
        /// Returns when the loading screen is ready to take over completely from the old scene.
        /// </summary>
        public Task LoadingScreenBegin();
        /// <summary>
        /// Async operation to signal that the level has finished loading.
        /// Returns when the loading screen has finished its animation and is ready to be unloaded.
        /// </summary>
        public Task LoadingScreenEnd();

        /// <summary>
        /// Text that may be displayed to let the player know what is happening. E.g. "Loading level", "Connecting to server".
        /// </summary>
        public void SetProgressionText(string text);
    }
}