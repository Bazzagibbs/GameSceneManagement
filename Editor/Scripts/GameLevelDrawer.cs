using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BazzaGibbs.GameSceneManager
{
    // [CustomEditor(typeof(GameLevel))]
    public class GameLevelDrawer : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            if (GUILayout.Button("Load Level")) {
                LoadSceneCollection();
            }
            
        }

        public void LoadSceneCollection() {
            SerializedProperty sceneRefs = serializedObject.FindProperty("sceneRefs");
            // sceneRefs
        }
    }
}
