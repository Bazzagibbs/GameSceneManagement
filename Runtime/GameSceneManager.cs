using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using log4net.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BazzaGibbs.GameSceneManagement
{
    public class GameSceneManager : MonoBehaviour {
        // Only use for checking singleton
        private static GameSceneManager s_Instance;

        private static GameSceneManager Instance {
            get {
                if (s_Instance == null) {
                    s_Instance = new GameObject().AddComponent<GameSceneManager>();
                    s_Instance.wasInstantiatedByProperty = true;
                }

                return s_Instance;
            }
        }

        [SerializeField] private GameLevel m_StartLevel;
        [SerializeField] private List<GameCoreScene> m_StartCoreScenes = new();

        // One and only one level can be loaded at a time.
        [NonSerialized] public GameLevel currentLevel;
        // Core scenes are not dependent on any level or aux scenes, and do not need to be unloaded when changing level.
        [NonSerialized] public HashSet<GameCoreScene> coreScenes = new();
        // Auxiliary scenes can be dependent on a level or core scene. They are all unloaded when the level is changed.
        [NonSerialized] public HashSet<GameAuxiliaryScene> auxiliaryScenes = new();

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
                LoadCoreSceneAsync(coreScene);
            }

            if (m_StartLevel != null) {
                // Task t = SetLevelAsync(m_StartLevel);
                // t.Start();
                // t.Wait();
                _ = SetLevelAsync(m_StartLevel);
            }
        }

        public static async Task<LoadedSceneCollection> SetLevelAsync(GameLevel level) {
            // Set current active scene to entry point.
            if (SceneManager.GetActiveScene() != Instance.gameObject.scene) {
                SceneManager.SetActiveScene(Instance.gameObject.scene);
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
            return await level.LoadAsync();
            // Level will set itself active, we don't have a reference to the actual Scene object
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

    }
}