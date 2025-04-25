using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdditiveScenes : MonoBehaviour
{
    private bool isPaused = false;
    public string pauseSceneName = "PauseMenu";
    public string upgradeSceneName = "UpgradeMenu";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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

    private void LoadSceneAdditive(string sceneName)
    {
        if (!SceneManager.GetSceneByName(sceneName).isLoaded)
        {
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        }
    }

    private void UnloadSceneIfLoaded(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(sceneName);
        }
    }
}