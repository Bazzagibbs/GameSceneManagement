using UnityEngine;

namespace BazzaGibbs.GameSceneManagement {
    [CreateAssetMenu(menuName = "Game Scene Management/Game Level", fileName = "New Level", order = 0)]
    public class GameLevel : SceneCollection {
        protected override bool setSelfActive => true;
    }
}