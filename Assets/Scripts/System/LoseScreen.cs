using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseScreen : MonoBehaviour
{
    public static LoseScreen Instance;
    public GameObject panel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (panel != null)
            panel.SetActive(false); // Only hide the panel, not the entire canvas
    }

    public void Show()
    {
        if (panel != null)
            panel.SetActive(true);
    }

    public void Retry()
    {
        Time.timeScale = 1f; // Unpause
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload current scene
    }

    public void Quit()
    {
        Time.timeScale = 1f; // Unpause
        SceneManager.LoadScene("MainMenu"); // Load your main menu scene
    }
}
