using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerExperience : MonoBehaviour
{
    public static PlayerExperience Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public int currentExp = 0;
    public int currentLevel = 1;
    public int expToNextLevel = 100;

    [Header("UI Elements")]
    public TextMeshProUGUI expText; // Optional display
    public UnityEngine.UI.Slider expBar; // EXP bar fill

    private void Start()
    {
        UpdateUI();
    }

    public void GainExperience(int amount)
    {
        currentExp += amount;

        if (currentExp >= expToNextLevel)
        {
            LevelUp();
        }

        UpdateUI();
    }

    private void LevelUp()
    {
        currentLevel++;
        currentExp -= expToNextLevel;

        // Optionally scale the next level EXP requirement
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * 1.25f);

        Debug.Log("Leveled Up! New Level: " + currentLevel);

        // TODO: Trigger level-up menu, give upgrade options, etc.
    }

    private void UpdateUI()
    {
        if (expText != null)
        {
            expText.text = $"EXP: {currentExp}/{expToNextLevel} (Lv {currentLevel})";
        }

        if (expBar != null)
        {
            expBar.value = (float)currentExp / expToNextLevel;
        }
    }
}
