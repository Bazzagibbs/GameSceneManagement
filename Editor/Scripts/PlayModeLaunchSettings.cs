#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

namespace BazzaGibbs.GameSceneManager {
     
    [CreateAssetMenu(menuName = "Game Scene Manager/Play Mode Launch Settings", fileName = "PlayModeLaunchSettings")]
    public class PlayModeLaunchSettings : ScriptableObject {
        public SceneAsset playModeScene;

        private void Awake() {
            SetPlayModeScene();
        }
        
        private void OnValidate() {
            SetPlayModeScene();
        }

        private void SetPlayModeScene() {
#if UNITY_EDITOR
            EditorSceneManager.playModeStartScene = playModeScene;
#endif
        }
    }
}