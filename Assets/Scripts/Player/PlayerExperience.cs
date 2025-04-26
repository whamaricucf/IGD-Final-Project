using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerExperience : MonoBehaviour
{
    public static PlayerExperience Instance;

    public int currentExp = 0;
    public int currentLevel = 1;
    public int expToNextLevel = 10;

    public TextMeshProUGUI expText;
    public Slider expBar;

    private bool isUpgradeScreenActive = false;
    private int upgradesSelected = 0;
    private int upgradesRequired = 0;

    private bool upgradeScreenTriggered = false;  // Prevent multiple upgrade screens
    private bool levelUpInProgress = false; // Track if a level-up is already in progress

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        CalculateNextLevelXP();
        UpdateUI();
    }

    public void GainExperience(int amount)
    {
        currentExp += amount;
        bool levelUpOccurred = false;

        // Only process level-up if not already in progress
        if (!levelUpInProgress)
        {
            levelUpInProgress = true;  // Flag level-up in progress

            while (currentExp >= expToNextLevel)
            {
                currentExp -= expToNextLevel;
                currentLevel++;
                levelUpOccurred = true;
                CharacterUnlockManager.Instance.CheckForCharacterUnlocks(PlayerPrefs.GetString("SelectedCharacter", "A"), currentLevel);
                CalculateNextLevelXP();
            }

            // Only call ShowUpgrades once per level-up
            if (levelUpOccurred && !upgradeScreenTriggered)
            {
                upgradeScreenTriggered = true;  // Prevent multiple calls in the same level-up cycle
                UpgradeManager.Instance.ShowUpgrades();
            }

            // Update the number of upgrades required
            upgradesRequired = Mathf.Max(0, currentLevel - 1);

            // Call OpenUpgradeMenu if the player has reached the required level for upgrades
            if (upgradesRequired > 0)
            {
                AdditiveScenes additiveScenes = FindObjectOfType<AdditiveScenes>();
                if (additiveScenes != null)
                {
                    additiveScenes.OpenUpgradeMenu();
                }
            }

            UpdateUI();
        }
        else
        {
            // If a level-up is already in progress, just update the UI without triggering upgrades
            UpdateUI();
        }

        levelUpInProgress = false;  // Reset flag after the level-up cycle is complete
    }

    private void CalculateNextLevelXP()
    {
        if (currentLevel < 20)
            expToNextLevel = 15 + (currentLevel * 5);
        else if (currentLevel < 40)
            expToNextLevel = 105 + ((currentLevel - 20) * 8);
        else
            expToNextLevel = 265 + ((currentLevel - 40) * 12);
    }

    public void SetUpgradeScreenActive(bool isActive)
    {
        isUpgradeScreenActive = isActive;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (expText != null)
            expText.text = $"LV {currentLevel}";

        if (expBar != null)
        {
            // If the upgrade screen is active, set the exp bar to full
            if (isUpgradeScreenActive)
            {
                expBar.value = 100f;  // Full progress bar
            }
            else
            {
                float expProgress = (float)currentExp / (float)expToNextLevel;
                expBar.value = expProgress;
            }
        }
    }

    public void UpgradeSelected()
    {
        upgradesSelected++;

        // Close the upgrade screen after selecting upgrades
        if (upgradesSelected >= upgradesRequired)
        {
            AdditiveScenes additiveScenes = FindObjectOfType<AdditiveScenes>();
            if (additiveScenes != null)
            {
                additiveScenes.CloseUpgradeMenu();
            }

            upgradeScreenTriggered = false;  // Reset the flag after closing the menu
        }
    }
}
