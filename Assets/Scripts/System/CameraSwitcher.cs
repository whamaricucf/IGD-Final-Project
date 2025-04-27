using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraSwitcher : MonoBehaviour
{
    private Camera mainMenuCamera;
    private Camera gameCamera;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindCameras(); // Always find fresh references after scene loads

        if (scene.name == "GameScene")
        {
            if (mainMenuCamera != null) mainMenuCamera.gameObject.SetActive(false);
            if (gameCamera != null) gameCamera.gameObject.SetActive(true);
        }
        else if (scene.name == "MainMenu")
        {
            if (mainMenuCamera != null) mainMenuCamera.gameObject.SetActive(true);
            if (gameCamera != null) gameCamera.gameObject.SetActive(false);
        }
    }

    private void FindCameras()
    {
        // Look for cameras by name or tag
        if (mainMenuCamera == null)
            mainMenuCamera = GameObject.Find("MainMenuCam")?.GetComponent<Camera>();

        if (gameCamera == null)
            gameCamera = GameObject.Find("PlayerCamera")?.GetComponent<Camera>();
    }
}
