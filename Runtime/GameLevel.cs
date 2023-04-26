using UnityEngine;

namespace BazzaGibbs.GameSceneManager {
    [CreateAssetMenu(menuName = "Game Scene Manager/Game Level", fileName = "New Level", order = 0)]
    public class GameLevel : SceneCollection {
        protected override bool setSelfActive => true;
    }
}