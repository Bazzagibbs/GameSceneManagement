using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace BazzaGibbs.GameSceneManager
{
    public class SceneCollection : ScriptableObject {
        // public AssetReference loadingScreenSceneRef;
        public List<AssetReference> sceneRefs;
        public UnityEvent<SceneCollection> OnSceneCollectionLoaded;
        
        protected virtual bool setSelfActive => false; // Overridden by GameLevel
        
        public virtual void Load() {
            
            // TODO: Show loading Screen scene if requested
            LoadInternal();
            
        }


        private async void LoadInternal() {
            for (int i = 0; i < sceneRefs.Count; i++) {
                AssetReference assetRef = sceneRefs[i];
                AsyncOperationHandle handle = assetRef.LoadSceneAsync(LoadSceneMode.Additive);
                // handle.Completed += OnHandleCompleted;
                await handle.Task;

                // Set the first scene as the active scene if we're loading a Level
                if (i == 0 && setSelfActive) {
                    SceneManager.SetActiveScene(((SceneInstance)handle.Result).Scene);
                }
            }

            OnSceneCollectionLoaded?.Invoke(this);
        }

        // private void OnHandleCompleted(AsyncOperationHandle obj) {
        //     // Interlocked.Increment(ref m_ScenesLoaded);
        // }

        public virtual void Unload() {
            
            foreach (AssetReference assetRef in sceneRefs) {
                assetRef.UnLoadScene();
            }
        }
    }
}
