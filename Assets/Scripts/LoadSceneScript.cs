using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class LoadSceneScript : MonoBehaviour
    {
        public GameObject unitPrefab;

        public void LoadGameScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
