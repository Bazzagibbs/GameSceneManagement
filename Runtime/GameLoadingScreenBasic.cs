using System.Threading.Tasks;
using UnityEngine;

namespace BazzaGibbs.GameSceneManagement
{
    public class GameLoadingScreenBasic : MonoBehaviour, IGameLoadingScreen
    {
        public Task LoadingScreenBegin() {
            return Task.CompletedTask;
        }

        public Task LoadingScreenEnd() {
            return Task.CompletedTask;
        }

        public void SetProgressionText(string text) {
        }
    }
}
