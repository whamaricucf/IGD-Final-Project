using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private SaveData currentSaveData;
    public CharacterDatabase characterDatabase;

    public TextMeshProUGUI coinsEarnedText;
    public TextMeshProUGUI enemiesDefeatedText;

    private int coinsEarned, enemiesDefeated;
    private bool uiInitialized = false;

    private GameTimer gameTimer; // Reference to the GameTimer (set dynamically)

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Start()
    {
        currentSaveData = SaveManager.Load();
        coinsEarned = 0;
        enemiesDefeated = 0;

        // Don't touch UI here — we wait for OnSceneLoaded
        EnemyBatAI.OnEnemyDied += HandleEnemyDeath;
        EnemyGhostAI.OnEnemyDied += HandleEnemyDeath;
        EnemySpiderAI.OnEnemyDied += HandleEnemyDeath;
    }

    private IEnumerator DelayedUIInit()
    {
        // Wait 1 frame to ensure UI exists
        yield return null;

        UpdateCoinsUI();
        UpdateEnemiesDefeatedUI();
        uiInitialized = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            // Get stats from selected character
            string selectedCharacterID = PlayerPrefs.GetString("SelectedCharacter", "A");
            PlayerData stats = characterDatabase.GetCharacterData(selectedCharacterID);

            // Initialize PlayerStats with selected character's data
            PlayerStats.Instance.InitializeStats(stats);

            // Assign the player stats to MagnetZone
            GameObject magnetZone = GameObject.FindWithTag("Magnet");
            if (magnetZone != null && magnetZone.TryGetComponent(out MagnetZone magnet))
            {
                magnet.Initialize(stats); // Pass stats to MagnetZone
            }
            else
            {
                Debug.LogError("GameManager: MagnetZone not found or not properly assigned.");
            }

            // Apply permanent upgrades (loaded from SaveData or PlayerPrefs)
            ApplyPermanentUpgrades();

            // Grab UI refs
            coinsEarnedText = GameObject.FindWithTag("CoinsText")?.GetComponent<TextMeshProUGUI>();
            enemiesDefeatedText = GameObject.FindWithTag("EnemiesText")?.GetComponent<TextMeshProUGUI>();

            // Now safe to update UI
            UpdateCoinsUI();
            UpdateEnemiesDefeatedUI();
            uiInitialized = true;

            // Find the GameTimer object dynamically (from the Game Scene)
            GameObject timerObject = GameObject.FindWithTag("GameTimer");
            if (timerObject != null)
            {
                gameTimer = timerObject.GetComponent<GameTimer>();
            }
            else
            {
                Debug.LogError("GameManager: GameTimer not found in the scene.");
            }
        }
    }


    // Apply permanent upgrades to PlayerStats (based on SaveData or PlayerPrefs)
    private void ApplyPermanentUpgrades()
    {
        PlayerStats playerStats = PlayerStats.Instance;

        // Example: Apply damage, speed, etc. from PlayerPrefs or SaveData
        playerStats.damage += PlayerPrefs.GetFloat("PermanentDamageUpgrade", 0);
        playerStats.speed += PlayerPrefs.GetFloat("PermanentSpeedUpgrade", 0);
        playerStats.health += PlayerPrefs.GetFloat("PermanentHealthUpgrade", 0);

        // Apply other permanent upgrades here as necessary (armor, magnet, etc.)
        Debug.Log("Permanent upgrades applied!");
    }

    public void AddCoins(int amount)
    {
        coinsEarned += amount;
        if (uiInitialized) UpdateCoinsUI();

        float dynamicShake = amount >= 3 ? Mathf.Clamp(amount, 5f, 20f) : 0f;
        Color flash = amount >= 3 ? GetFlashColor(dynamicShake, Color.yellow, new Color(1f, 0.84f, 0f)) : Color.yellow;
        PopAndFlashText(coinsEarnedText, flash, 1.2f, 0.2f, dynamicShake);
    }

    public void AddEnemiesDefeated(int amount)
    {
        enemiesDefeated += amount;
        if (uiInitialized) UpdateEnemiesDefeatedUI();

        float dynamicShake = Mathf.Clamp(amount * 2f, 5f, 25f);
        Color flash = GetFlashColor(dynamicShake, Color.red, new Color(0.6f, 0f, 1f));
        PopAndFlashText(enemiesDefeatedText, flash, 1.2f, 0.2f, dynamicShake);
    }

    private Color GetFlashColor(float shake, Color normal, Color critical) => shake >= 15f ? critical : normal;
    private void UpdateCoinsUI() => coinsEarnedText.text = coinsEarned.ToString();
    private void UpdateEnemiesDefeatedUI() => enemiesDefeatedText.text = enemiesDefeated.ToString();

    private void PopAndFlashText(TextMeshProUGUI target, Color flashColor, float scale = 1.2f, float duration = 0.2f, float shake = 0f)
    {
        if (target == null) return;

        Vector3 originalScale = target.rectTransform.localScale;
        Vector3 originalPosition = target.rectTransform.localPosition;
        Color originalColor = target.color;

        target.rectTransform.DOKill();
        target.DOKill();

        target.rectTransform
            .DOScale(originalScale * scale, duration / 2f)
            .SetEase(Ease.OutElastic)
            .OnComplete(() =>
            {
                target.rectTransform.DOScale(originalScale, duration / 2f).SetEase(Ease.InOutElastic).OnComplete(() =>
                {
                    if (shake > 0f)
                        target.rectTransform.DOShakePosition(0.2f, new Vector3(shake, 0f, 0f))
                        .OnComplete(() => target.rectTransform.localPosition = originalPosition);
                });
            });

        target.DOColor(flashColor, duration / 2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => target.DOColor(originalColor, duration / 2f).SetEase(Ease.InQuad));
    }

    private void HandleEnemyDeath()
    {
        AddEnemiesDefeated(1);
        AddCoins(1);
    }

    // Call this method when the game is paused
    public void PauseGame()
    {
        Time.timeScale = 0f; // Pauses the game

        // Manually pause the timer
        if (gameTimer != null)
        {
            gameTimer.PauseTimer();  // Pause the game timer
        }
        else
        {
            Debug.LogError("GameTimer is not properly initialized.");
        }
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f; // Resumes the game

        // Resume the timer when the game is resumed
        if (gameTimer != null)
        {
            gameTimer.ResumeTimer();  // Resume the game timer
        }
        else
        {
            Debug.LogError("GameTimer is not properly initialized.");
        }
    }

}
