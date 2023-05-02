#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

namespace BazzaGibbs.GameSceneManagement {
     
    [CreateAssetMenu(menuName = "Game Scene Management/Play Mode Launch Settings", fileName = "PlayModeLaunchSettings", order = 22)]
    public class PlayModeLaunchSettings : ScriptableObject {
        public bool enabled = true;
#if UNITY_EDITOR
        public SceneAsset playModeScene;

        [MenuItem("Tools/Game Scene Management/Play Mode Launch Settings")]
        public static void FindPlayModeLaunchSettings() {
            
            string[] results = AssetDatabase.FindAssets("t:PlayModeLaunchSettings");
            PlayModeLaunchSettings instance;
        
            if (results.Length <= 0) {
                instance = CreateInstance<PlayModeLaunchSettings>();
                AssetDatabase.CreateAsset(instance, "Assets/Settings/PlayModeLaunchSettings.asset");
            } 
            else {
                if (results.Length > 1) {
                    Debug.LogWarning("[Game Scene Manager] Multiple PlayModeLaunchSettings assets found. Please make sure there are no more than one.");
                }

                string path = AssetDatabase.GUIDToAssetPath(results[0]);
                instance = AssetDatabase.LoadAssetAtPath<PlayModeLaunchSettings>(path);
            }
            
            Selection.activeObject = instance;
            EditorGUIUtility.PingObject(instance);
        }
        
#endif
        private void Awake() {
            SetPlayModeScene();
        }
        
        private void OnValidate() {
            SetPlayModeScene();
        }

        private void SetPlayModeScene() {
#if UNITY_EDITOR
            if (enabled && playModeScene != null) {
                EditorSceneManager.playModeStartScene = playModeScene;
            }
            else {
                EditorSceneManager.playModeStartScene = null;
            }
#endif
        }
    }
    
}