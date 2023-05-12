using System.Threading.Tasks;
using UnityEngine;

namespace BazzaGibbs.GameSceneManagement {
    
    /// <summary>
    /// Convenience ScriptableObject to allow easy UnityEvent hooks, such as using a UI button to load a scene.
    /// </summary>
    [CreateAssetMenu(menuName = "Game Scene Management/Game Scene Manager Hook", fileName = "Game Scene Manager Hook", order = 100)]
    public class GameSceneManagerHook : ScriptableObject{
        public static void SetLevel(GameLevel level) {
            // Task t = GameSceneManager.SetLevelAsync(level);
            // t.Start();
            // t.Wait();
            _ = GameSceneManager.SetLevelAsync(level);
        }

        public static void LoadAuxScene(GameAuxiliaryScene scene) {
            _ = GameSceneManager.LoadAuxSceneAsync(scene);
        }

        public static void UnloadAuxScene(GameAuxiliaryScene scene) {
            GameSceneManager.UnloadAuxSceneAsync(scene);
        }

        public static void ToggleAuxScene(GameAuxiliaryScene scene) {
            GameSceneManager.ToggleAuxScene(scene);
        }

        public static void LoadCoreScene(GameCoreScene scene) { 
            _ = GameSceneManager.LoadCoreSceneAsync(scene);
        }

        public static void UnloadCoreScene(GameCoreScene scene) {
            GameSceneManager.UnloadCoreSceneAsync(scene);
        }
    }
}