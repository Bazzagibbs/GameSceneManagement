using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace BazzaGibbs.GameSceneManagement
{
    public class SceneCollection : ScriptableObject {
        public string uniqueNameOverride = "";
        public List<AssetReference> sceneRefs;
        public UnityEvent<LoadedSceneCollection> OnSceneCollectionLoaded;
        
        protected virtual bool setSelfActive => false; // Overridden by GameLevel
        
        public virtual async Task<LoadedSceneCollection> LoadAsync() {
            Task<SceneInstance>[] handles = new Task<SceneInstance>[sceneRefs.Count];
            
            for (int i = 0; i < sceneRefs.Count; i++) {
                AssetReference assetRef = sceneRefs[i];
                handles[i] = assetRef.LoadSceneAsync(LoadSceneMode.Additive).Task;
            }

            SceneInstance[] instances = await Task.WhenAll(handles);
            if (setSelfActive) {
                SceneManager.SetActiveScene(instances[0].Scene);
            }
            
            LoadedSceneCollection loadedCollection = new();
            loadedCollection.sceneInstances = instances;
            
            OnSceneCollectionLoaded?.Invoke(loadedCollection);
            return loadedCollection;
        }

        public virtual async Task UnloadAsync() {
            Task<SceneInstance>[] handles = new Task<SceneInstance>[sceneRefs.Count];
            // Unload scenes in reverse order in case of dependencies
            for(int i = sceneRefs.Count - 1; i >= 0; i--) {
                AssetReference assetRef = sceneRefs[i];
                handles[i] = assetRef.UnLoadScene().Task;
            }

            await Task.WhenAll(handles);
        }


        public bool EqualSceneCollection(SceneCollection other) {
            string thisIdentifier = uniqueNameOverride == "" ? name : uniqueNameOverride;
            string otherIdentifier = other.uniqueNameOverride == "" ? other.name : other.uniqueNameOverride;

            return thisIdentifier == otherIdentifier;
        }
    }

    public class LoadedSceneCollection {
        public SceneInstance[] sceneInstances;
    }
}
