using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    private SaveData currentSaveData;

    public TextMeshProUGUI coinsEarnedText;
    public TextMeshProUGUI enemiesDefeatedText;

    private int coinsEarned, enemiesDefeated;

    private void Start()
    {
        // Load existing save at game start
        currentSaveData = SaveManager.Load();

        coinsEarned = 0;
        enemiesDefeated = 0;
        UpdateCoinsUI();
        UpdateEnemiesDefeatedUI();

        // Listen to enemy death events
        EnemyBatAI.OnEnemyDied += HandleEnemyDeath;
        EnemyGhostAI.OnEnemyDied += HandleEnemyDeath;
        EnemySpiderAI.OnEnemyDied += HandleEnemyDeath;

    }

    public void AddCoins(int amount)
    {
        coinsEarned += amount;
        UpdateCoinsUI();

        // Dynamic shake size but only if enough coins are earned
        if (amount >= 3) // 3 or more coins
        {
            float dynamicShake = Mathf.Clamp(amount, 5f, 20f);
            Color dynamicFlashColor = GetFlashColorBasedOnStrength(dynamicShake, Color.yellow, new Color(1f, 0.84f, 0f));
            PopAndFlashText(coinsEarnedText, dynamicFlashColor, 1.2f, 0.2f, dynamicShake);
        }
        else
        {
            PopAndFlashText(coinsEarnedText, Color.yellow);
        }
        
    }

    public void AddEnemiesDefeated(int amount)
    {
        enemiesDefeated += amount;
        UpdateEnemiesDefeatedUI();

        // Make the shake stronger if the player kills more enemies at once
        float dynamicShake = Mathf.Clamp(amount * 2f, 5f, 25f);
        Color dynamicFlashColor = GetFlashColorBasedOnStrength(dynamicShake, Color.red, new Color(0.6f, 0f, 1f));
        PopAndFlashText(enemiesDefeatedText, dynamicFlashColor, 1.2f, 0.2f, dynamicShake);
    }

    private Color GetFlashColorBasedOnStrength(float shakeStrength, Color normalColor, Color criticalColor)
    {
        // If shakeStrength is above a certain threshold, use criticalColor
        if (shakeStrength >= 15f)
        {
            return criticalColor;
        }
        else
        {
            return normalColor;
        }
    }

    private void UpdateCoinsUI()
    {
        coinsEarnedText.text = coinsEarned.ToString();
    }

    private void UpdateEnemiesDefeatedUI()
    {
        enemiesDefeatedText.text = enemiesDefeated.ToString();
    }

    private void PopAndFlashText(TextMeshProUGUI targetText, Color flashColor, float popScale = 1.2f, float popDuration = 0.2f, float shakeStrength = 0f)
    {
        Vector3 originalScale = targetText.rectTransform.localScale; // Store the original scale
        Vector3 originalPosition = targetText.rectTransform.localPosition; // Store original position
        Color originalColor = targetText.color; // Store the original color

        // Kill any existing tween first to prevent stacking
        targetText.rectTransform.DOKill();
        targetText.DOKill(); // Kills any color tweens

        // Scale pop
        targetText.rectTransform
            .DOScale(originalScale * popScale, popDuration / 2f)
            .SetEase(Ease.OutElastic)
            .OnComplete(() =>
            {
                targetText.rectTransform
                .DOScale(originalScale, popDuration / 2f)
                .SetEase(Ease.InOutElastic)
                .OnComplete(() =>
                {
                    if (shakeStrength > 0f)
                    {
                        // Shake based on dynamic strength
                        targetText.rectTransform
                        .DOShakePosition(0.2f, new Vector3(shakeStrength, 0f, 0f)) // Duration, Strength
                        .OnComplete(() =>
                        {
                            // After shaking, reset position
                            targetText.rectTransform.localPosition = originalPosition;
                        });
                    }
                });
            });

        // Color flash (flashColor then back)
        targetText.DOColor(Color.yellow, popDuration / 2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                targetText.DOColor(originalColor, popDuration / 2f)
                .SetEase(Ease.InQuad);
            });
    }

    public void OnEnemyDefeated(string enemyType)
    {
        if (currentSaveData.enemiesDefeated.ContainsKey(enemyType))
        {
            currentSaveData.enemiesDefeated[enemyType]++;
        }
        else
        {
            currentSaveData.enemiesDefeated[enemyType] = 1;
        }

        // Auto-save after updating kill count
        SaveManager.Save(currentSaveData);
        Debug.Log($"Auto-saved after defeating {enemyType}. Total defeats: {currentSaveData.enemiesDefeated[enemyType]}");
    }

    public void OnCharacterUnlocked(string characterName)
    {
        if (!currentSaveData.unlockedCharacters.Contains(characterName))
        {
            currentSaveData.unlockedCharacters.Add(characterName);
            SaveManager.Save(currentSaveData);
            Debug.Log("Auto-saved after unlocking " + characterName);
        }
    }

    public void OnUpgradePurchased(string upgradeType, int level)
    {
        currentSaveData.playerUpgrades[upgradeType] = level;
        SaveManager.Save(currentSaveData);
        Debug.Log("Auto-saved after purchasing upgrade: " + upgradeType);
    }

    private void OnApplicationQuit()
    {
        if (currentSaveData != null)
        {
            SaveManager.Save(currentSaveData);
            Debug.Log("Auto-saved on application quit.");
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause && currentSaveData != null)
        {
            SaveManager.Save(currentSaveData);
            Debug.Log("Auto-saved on application pause.");
        }
    }

    public void OnRunEnd(int coinsEarned)
    {
        // Add coins earned this session to the toal
        currentSaveData.totalCoins += coinsEarned;

        // Save the game
        SaveManager.Save(currentSaveData);
        Debug.Log($"Run ended. Earned {coinsEarned} coins. Total coinws now: {currentSaveData.totalCoins}");
    }

    private void HandleEnemyDeath()
    {
        AddEnemiesDefeated(1);
        AddCoins(1);
    }

}
