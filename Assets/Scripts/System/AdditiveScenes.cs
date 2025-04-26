using UnityEngine;

public class AdditiveScenes : MonoBehaviour
{
    private bool isPaused = false;
    public GameObject upgradeMenuCanvas;  // Cache the reference to the canvas

    private bool hasUpgradedOnce = false;

    void Awake()
    {
        if (upgradeMenuCanvas == null)
        {
            Debug.LogError("Upgrade Menu Canvas not assigned in the Inspector.");
        }
        else
        {
            Debug.Log("Disabling UpgradeMenuCanvas at Awake.");
            upgradeMenuCanvas.SetActive(false); // Forcefully disable it
        }
    }




    public void OpenUpgradeMenu()
    {
        PauseGame();

        // Make sure canvas is not already active
        if (upgradeMenuCanvas != null)
        {
            upgradeMenuCanvas.SetActive(true);  // Forcefully activate if needed
        }
        else
        {
            Debug.LogWarning("Upgrade Menu Canvas not found when opening the upgrade menu.");
        }

        // Set the upgrade screen as active
        PlayerExperience.Instance.SetUpgradeScreenActive(true);
    }


    public void SetUpgradeMenuActive(bool isActive)
    {
        if (upgradeMenuCanvas != null)
        {
            upgradeMenuCanvas.SetActive(isActive);
        }
        else
        {
            Debug.LogWarning("Upgrade Menu Canvas not found when trying to set active!");
        }
    }

    public void CloseUpgradeMenu()
    {
        // Deactivate the upgrade screen before resuming
        SetUpgradeMenuActive(false);
        PlayerExperience.Instance.SetUpgradeScreenActive(false);

        // Optionally, do any cleanup or checks here to ensure the canvas is disabled properly
        ResumeGame();
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
    }
}
