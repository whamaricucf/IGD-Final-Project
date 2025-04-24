using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditiveScenes : MonoBehaviour
{
    private bool isPaused = false;
    readonly public string pauseSceneName = "PauseMenu"; // set these in the Inspector or hardcode
    readonly public string upgradeSceneName = "UpgradeMenu";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Replace with your input system
        {
            if (!isPaused)
            {
                PauseGame();
                LoadSceneAdditive(pauseSceneName);
            }
            else
            {
                ResumeGame();
            }
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        UnloadSceneIfLoaded(pauseSceneName);
        UnloadSceneIfLoaded(upgradeSceneName);
    }

    public void OpenUpgradeMenu()
    {
        PauseGame();
        LoadSceneAdditive(upgradeSceneName);
    }

    void LoadSceneAdditive(string sceneName)
    {
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }
    }

    void UnloadSceneIfLoaded(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(sceneName);
        }
    }
}
