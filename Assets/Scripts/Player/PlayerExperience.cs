using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerExperience : MonoBehaviour
{
    public static PlayerExperience Instance;

    public int currentExp = 0;
    public int currentLevel = 1;
    public int expToNextLevel = 10;
    public int pendingUpgradePicks = 0;

    public TextMeshProUGUI expText;
    public Slider expBar;

    private bool isUpgradeScreenActive = false;
    public bool upgradeScreenTriggered = false;

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

        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            currentLevel++;
            pendingUpgradePicks++;

            CharacterUnlockManager.Instance.CheckForCharacterUnlocks(PlayerPrefs.GetString("SelectedCharacter", "A"), currentLevel);
            CalculateNextLevelXP();
        }

        if (pendingUpgradePicks > 0 && !upgradeScreenTriggered)
        {
            upgradeScreenTriggered = true;
            UpgradeManager.Instance.ShowUpgrades();

            FindObjectOfType<AdditiveScenes>()?.OpenUpgradeMenu();
        }

        UpdateUI();
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

    public void UpgradeSelected()
    {
        // Nothing needed here now
    }

    public void UpdateUI()
    {
        if (expText != null)
            expText.text = $"LV {currentLevel}";

        if (expBar != null)
        {
            if (isUpgradeScreenActive)
            {
                expBar.maxValue = 1f;
                expBar.DOValue(1f, 0.25f);
            }
            else
            {
                expBar.maxValue = expToNextLevel;
                expBar.DOKill();
                expBar.DOValue(currentExp, 0.25f).SetEase(Ease.OutQuad);
            }
        }
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }
}