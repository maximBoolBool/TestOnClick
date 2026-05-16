using UnityEngine;

namespace Assets.Scripts
{
    public class CloseScript : MonoBehaviour
{
    public static void Close()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    }
}
