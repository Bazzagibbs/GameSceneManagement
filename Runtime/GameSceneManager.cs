using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace BazzaGibbs.GameSceneManagement
{
    public class GameSceneManager : MonoBehaviour {
        private static GameSceneManager s_Instance;

        public static GameSceneManager Instance {
            get {
                if (s_Instance == null) {
                    s_Instance = new GameObject().AddComponent<GameSceneManager>();
                    s_Instance.wasInstantiatedByProperty = true;
                }

                return s_Instance;
            }
        }

        public static GameLevel offlineLevel =>
            Instance.registeredLevels.Length > 0
                ? Instance.registeredLevels[0]
                : null;

        [Tooltip("First entry will be used as the offline scene")]
        public GameLevel[] registeredLevels;
        [SerializeField] private List<GameCoreScene> m_StartCoreScenes = new();
        [SerializeField] private GameCoreScene m_LoadingScene;
        private GameCoreScene m_CurrentLoadingScene;
        private IGameLoadingScreen m_LoadingScreenObj;
        
        // One and only one level can be loaded at a time.
        [NonSerialized] public (GameLevel, LoadedSceneCollection) currentLevel;
        // Core scenes are not dependent on any level or aux scenes, and do not need to be unloaded when changing level.
        [NonSerialized] public Dictionary<GameCoreScene, LoadedSceneCollection> coreScenes = new();
        // Auxiliary scenes can be dependent on a level or core scene. They are all unloaded when the level is changed.
        [NonSerialized] public Dictionary<GameAuxiliaryScene, LoadedSceneCollection> auxiliaryScenes = new();

        private bool wasInstantiatedByProperty = false;

        
        private void Awake() {
            if (s_Instance != null && wasInstantiatedByProperty == false) {
                Debug.LogError("[GameSceneManager] Multiple GameSceneManagers. Please make sure only one exists.", this);
                DestroyImmediate(gameObject);
                return;
            }

            if (s_Instance == null) {
                s_Instance = this;
            }

            foreach (GameCoreScene coreScene in m_StartCoreScenes) {
                _ = LoadCoreSceneAsync(coreScene);
            }


            if (registeredLevels.Length > 0 && registeredLevels[0] != null) {
                _ = SetLevelAsync(registeredLevels[0]);
            }
        }

        public static async Task<LoadedSceneCollection> SetLevelAsync(GameLevel level, bool useLoadingScreen = false) {
            // Set current active scene to entry point.
            if (SceneManager.GetActiveScene() != Instance.gameObject.scene) {
                SceneManager.SetActiveScene(Instance.gameObject.scene);
            }

            if (useLoadingScreen) {
                await BeginLoadingScreen();
            }

            // Unload aux scenes before changing level
            Task[] unloadAuxTasks = new Task[Instance.auxiliaryScenes.Count];
            int i = 0;
            foreach (GameAuxiliaryScene auxScene in Instance.auxiliaryScenes.Keys) {
                unloadAuxTasks[i] = auxScene.UnloadAsync();
                i++;
            }

            await Task.WhenAll(unloadAuxTasks);
            Instance.auxiliaryScenes.Clear();

            // Unload previous level
            if (Instance.currentLevel.Item1 != null) {
                Instance.currentLevel.Item2 = null;
                await Instance.currentLevel.Item1.UnloadAsync();
            }

            // Load current level
            Instance.currentLevel = (level, await level.LoadAsync());
            // Level will set itself active, we don't have a reference to the actual Scene object
            // Loading screen will unload itself if it's subscribed to the onLevelLoaded event
            if (useLoadingScreen) {
                _ = EndLoadingScreen();
            }
            return Instance.currentLevel.Item2;
        }


        public static async Task<LoadedSceneCollection> LoadAuxSceneAsync(GameAuxiliaryScene auxScene) {
            if (Instance.auxiliaryScenes.TryGetValue(auxScene, out LoadedSceneCollection loadedScenes)) {
                return loadedScenes;
            }

            LoadedSceneCollection result = await auxScene.LoadAsync();
            Instance.auxiliaryScenes[auxScene] = result;
            return result;
        }

        public static async void UnloadAuxSceneAsync(GameAuxiliaryScene auxScene) {
            if(Instance.auxiliaryScenes.ContainsKey(auxScene)) {
                await auxScene.UnloadAsync();
                Instance.auxiliaryScenes.Remove(auxScene);
            }
        }

        public static void ToggleAuxScene(GameAuxiliaryScene auxScene) {
            if (Instance.auxiliaryScenes.ContainsKey(auxScene)) {
                UnloadAuxSceneAsync(auxScene);
            }
            else { 
                _ = LoadAuxSceneAsync(auxScene);
            }
        }

        public static async Task<LoadedSceneCollection> LoadCoreSceneAsync(GameCoreScene coreScene) {
            if (Instance.coreScenes.TryGetValue(coreScene, out LoadedSceneCollection loadedScenes)) {
                return loadedScenes;
            }

            LoadedSceneCollection result = await coreScene.LoadAsync();
            Instance.coreScenes[coreScene] = result;
            return result;
        }

        public static async void UnloadCoreSceneAsync(GameCoreScene coreScene) {
            if(Instance.coreScenes.ContainsKey(coreScene)) {
                await coreScene.UnloadAsync();
                Instance.coreScenes.Remove(coreScene);
            }
        }

        public static async Task BeginLoadingScreen() {
            LoadedSceneCollection loadingScene =  await LoadCoreSceneAsync(Instance.m_LoadingScene);
            Instance.m_LoadingScreenObj = null;
            foreach (SceneInstance si in loadingScene.sceneInstances) {
                bool shouldBreak = false;
                foreach (GameObject go in si.Scene.GetRootGameObjects()) {
                    Instance.m_LoadingScreenObj = go.GetComponentInChildren<IGameLoadingScreen>();
                    if (Instance.m_LoadingScreenObj != null) {
                        shouldBreak = true;
                        break;
                    }
                }

                if (shouldBreak) break;
            }
            
            if (Instance.m_LoadingScreenObj != null) {
                await Instance.m_LoadingScreenObj.LoadingScreenBegin();
                return;
            }
            
            Debug.LogError("Loading screen was requested, but no IGameLoadingScreen object was found.");
        }
        
        public static async Task EndLoadingScreen() {
            if (Instance.m_LoadingScreenObj != null) {
                await Instance.m_LoadingScreenObj.LoadingScreenEnd();
                UnloadCoreSceneAsync(Instance.m_LoadingScene);
            }
        }

    }
}