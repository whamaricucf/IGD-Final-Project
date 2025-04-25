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
            CharacterUnlockManager.Instance.CheckForCharacterUnlocks(PlayerPrefs.GetString("SelectedCharacter", "A"), currentLevel);
            CalculateNextLevelXP();
        }
        UpdateUI();
    }

    private void CalculateNextLevelXP()
    {
        if (currentLevel < 20)
            expToNextLevel = 5 + (currentLevel * 5);
        else if (currentLevel < 40)
            expToNextLevel = 105 + ((currentLevel - 20) * 8);
        else
            expToNextLevel = 265 + ((currentLevel - 40) * 12);
    }

    private void UpdateUI()
    {
        if (expText != null)
            expText.text = $"LV {currentLevel}";
        if (expBar != null)
            expBar.value = (float)currentExp / expToNextLevel;
    }
}
