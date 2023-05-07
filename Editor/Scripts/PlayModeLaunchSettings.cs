#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using System;
using UnityEngine;

namespace BazzaGibbs.GameSceneManagement {
     
    [CreateAssetMenu(menuName = "Game Scene Management/Play Mode Launch Settings", fileName = "PlayModeLaunchSettings", order = 22)]
    public class PlayModeLaunchSettings : ScriptableObject {
        public bool enabled = true;
#if UNITY_EDITOR
        public SceneAsset playModeScene;

        [InitializeOnLoadMethod]
        public static void SetupOnLoad() {
            if (TryFindPlayModeLaunchSettings(out PlayModeLaunchSettings settings)) {
                settings.SetPlayModeScene();
            }
        }

        public static bool TryFindPlayModeLaunchSettings(out PlayModeLaunchSettings settings) {
            string[] results = AssetDatabase.FindAssets("t:PlayModeLaunchSettings");
        
            if (results.Length <= 0) {
                settings = null;
                return false;
            } 
            
            if (results.Length > 1) {
                Debug.LogWarning("[Game Scene Manager] Multiple PlayModeLaunchSettings assets found. Please make sure there are no more than one.");
            }

            string path = AssetDatabase.GUIDToAssetPath(results[0]);
            settings = AssetDatabase.LoadAssetAtPath<PlayModeLaunchSettings>(path);
            return true;
        }
        
        [MenuItem("Tools/Game Scene Management/Play Mode Launch Settings")]
        public static void FindOrCreatePlayModeLaunchSettings() {
            if (TryFindPlayModeLaunchSettings(out PlayModeLaunchSettings settings)) {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }
            else {
                PlayModeLaunchSettings instance = CreateInstance<PlayModeLaunchSettings>();
                AssetDatabase.CreateAsset(instance, "Assets/Settings/PlayModeLaunchSettings.asset");
            }
        }
        
#endif

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