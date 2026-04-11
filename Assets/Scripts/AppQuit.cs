using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Quits the application when Escape is pressed.
/// Attach to any persistent GameObject in the scene.
/// </summary>
public class AppQuit : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
