using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void load_scene(string scene_name)
    {
        if (scene_name == LobbyUi.PLAY_SCENE)
        {
            LoginOverlay.require_login(() => open_scene(scene_name));
            return;
        }

        open_scene(scene_name);
    }

    void open_scene(string scene_name)
    {
        if (string.IsNullOrEmpty(scene_name) || !Application.CanStreamedLevelBeLoaded(scene_name))
        {
            Debug.LogWarning($"scene '{scene_name}' is not in the build settings yet");
            return;
        }

        SceneManager.LoadScene(scene_name);
    }
}
