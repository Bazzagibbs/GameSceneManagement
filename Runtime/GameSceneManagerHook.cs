using UnityEngine;

namespace BazzaGibbs.GameSceneManager {
    
    /// <summary>
    /// Convenience ScriptableObject to allow easy UnityEvent hooks, such as using a UI button to load a scene.
    /// </summary>
    [CreateAssetMenu(menuName = "Game Scene Manager/Game Scene Manager Hook", fileName = "Game Scene Manager Hook", order = 100)]
    public class GameSceneManagerHook : ScriptableObject{
        public static void SetLevel(GameLevel level) {
            GameSceneManager.SetLevel(level);
        }

        public static void LoadAuxScene(GameAuxiliaryScene scene) {
            GameSceneManager.LoadAuxScene(scene);
        }

        public static void UnloadAuxScene(GameAuxiliaryScene scene) {
            GameSceneManager.UnloadAuxScene(scene);
        }

        public static void ToggleAuxScene(GameAuxiliaryScene scene) {
            GameSceneManager.ToggleAuxScene(scene);
        }

        public static void LoadCoreScene(GameCoreScene scene) {
            GameSceneManager.LoadCoreScene(scene);
        }

        public static void UnloadCoreScene(GameCoreScene scene) {
            GameSceneManager.UnloadCoreScene(scene);
        }
    }
}