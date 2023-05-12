using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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
        [SerializeField] private GameCoreScene loadingScene;
        // One and only one level can be loaded at a time.
        [NonSerialized] public GameLevel currentLevel;
        // Core scenes are not dependent on any level or aux scenes, and do not need to be unloaded when changing level.
        [NonSerialized] public HashSet<GameCoreScene> coreScenes = new();
        // Auxiliary scenes can be dependent on a level or core scene. They are all unloaded when the level is changed.
        [NonSerialized] public HashSet<GameAuxiliaryScene> auxiliaryScenes = new();

        private UnityEvent onLoadingSceneReady = new(); // loading screen -> GSM
        [SerializeField] private UnityEvent onLevelLoaded = new(); // GSM -> loading screen + others

        private bool wasInstantiatedByProperty = false;

        private const int loadingScreenTransitionTimeOut = 3000;
        
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
                LoadCoreSceneAsync(coreScene);
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
                using (SemaphoreSlim sph = new SemaphoreSlim(0, 1)) {
                    await LoadCoreSceneAsync(Instance.loadingScene);

                    // loading screen should be ready, but we need to wait for its callback to continue.
                    void CallbackDelegate() => sph.Release();
                    Instance.onLoadingSceneReady.AddListener(CallbackDelegate);
                    Task callbackTask = sph.WaitAsync();

                    if (await Task.WhenAny(callbackTask, Task.Delay(loadingScreenTransitionTimeOut)) != callbackTask) {
                        Debug.LogError("Loading scene didn't callback after Fade In");
                    }

                    // loading screen has called back or timed out
                    Instance.onLoadingSceneReady.RemoveListener(CallbackDelegate);
                }
            }

            // Unload aux scenes before changing level
            Task[] unloadAuxTasks = new Task[Instance.auxiliaryScenes.Count];
            int i = 0;
            foreach (GameAuxiliaryScene auxScene in Instance.auxiliaryScenes) {
                unloadAuxTasks[i] = auxScene.UnloadAsync();
                i++;
            }

            await Task.WhenAll(unloadAuxTasks);
            Instance.auxiliaryScenes.Clear();

            // Unload previous level
            if (Instance.currentLevel != null) {
                await Instance.currentLevel.UnloadAsync();
            }

            // Load current level
            Instance.currentLevel = level;
            // Level will set itself active, we don't have a reference to the actual Scene object
            LoadedSceneCollection result = await level.LoadAsync();
            // Loading screen will unload itself if it's subscribed to the onLevelLoaded event
            Instance.onLevelLoaded?.Invoke();
            return result;
        }


        public static Task<LoadedSceneCollection> LoadAuxSceneAsync(GameAuxiliaryScene auxScene) {
            if (Instance.auxiliaryScenes.Contains(auxScene)) return null;
            Instance.auxiliaryScenes.Add(auxScene);
            return auxScene.LoadAsync();
        }

        public static async void UnloadAuxSceneAsync(GameAuxiliaryScene auxScene) {
            if(Instance.auxiliaryScenes.TryGetValue(auxScene, out GameAuxiliaryScene loadedAuxScene)) {
                await loadedAuxScene.UnloadAsync();
                Instance.auxiliaryScenes.Remove(loadedAuxScene);
            }
        }

        public static void ToggleAuxScene(GameAuxiliaryScene auxScene) {
            if (Instance.auxiliaryScenes.TryGetValue(auxScene, out GameAuxiliaryScene loadedAuxScene)) {
                UnloadAuxSceneAsync(loadedAuxScene);
            }
            else {
                LoadAuxSceneAsync(auxScene);
            }
        }

        public static Task<LoadedSceneCollection> LoadCoreSceneAsync(GameCoreScene coreScene) {
            if (Instance.coreScenes.Contains(coreScene)) return null;
            Instance.coreScenes.Add(coreScene);
            return coreScene.LoadAsync();
        }

        public static async void UnloadCoreSceneAsync(GameCoreScene coreScene) {
            if(Instance.coreScenes.TryGetValue(coreScene, out GameCoreScene loadedCoreScene)) {
                await loadedCoreScene.UnloadAsync();
                Instance.coreScenes.Remove(loadedCoreScene);
            }
        }

        public void LoadingScreenReady() {
            onLoadingSceneReady?.Invoke();
        }
    }
}