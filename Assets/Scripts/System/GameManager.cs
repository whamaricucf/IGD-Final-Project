using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager : MonoBehaviour
{
    private SaveData currentSaveData;
    public CharacterDatabase characterDatabase;

    public TextMeshProUGUI coinsEarnedText;
    public TextMeshProUGUI enemiesDefeatedText;

    private int coinsEarned, enemiesDefeated;
    private bool uiInitialized = false;

    private GameTimer gameTimer;

    public static GameManager Instance;

    private float savedTimeScale = 1f;

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentSaveData = SaveManager.Load();
        coinsEarned = 0;
        enemiesDefeated = 0;

        EnemyBatAI.OnEnemyDied += HandleEnemyDeath;
        EnemyGhostAI.OnEnemyDied += HandleEnemyDeath;
        EnemySpiderAI.OnEnemyDied += HandleEnemyDeath;

        if (SceneManager.GetActiveScene().name == "GameMaster")
        {
            SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
        }
    }

    private IEnumerator DelayedUIInit()
    {
        yield return null;
        UpdateCoinsUI();
        UpdateEnemiesDefeatedUI();
        uiInitialized = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            string selectedCharacterID = PlayerPrefs.GetString("SelectedCharacter", "A");
            PlayerData stats = characterDatabase.GetCharacterData(selectedCharacterID);

            // Initialize PlayerStats
            PlayerStats.Instance.InitializeStats(stats);

            // Force refresh PlayerHealth
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.ForceRefreshStats();
            }
            else
            {
                Debug.LogWarning("GameManager: PlayerHealth not found!");
            }

            // MagnetZone should already auto-update itself based on PlayerStats
            GameObject magnetZone = GameObject.FindWithTag("Magnet");
            if (magnetZone == null)
            {
                Debug.LogError("GameManager: MagnetZone not found or not properly assigned.");
            }

            ApplyPermanentUpgrades();

            coinsEarnedText = GameObject.FindWithTag("CoinsText")?.GetComponent<TextMeshProUGUI>();
            enemiesDefeatedText = GameObject.FindWithTag("EnemiesText")?.GetComponent<TextMeshProUGUI>();

            UpdateCoinsUI();
            UpdateEnemiesDefeatedUI();
            uiInitialized = true;

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

    private void ApplyPermanentUpgrades()
    {
        PlayerStats playerStats = PlayerStats.Instance;

        playerStats.damage += PlayerPrefs.GetFloat("PermanentDamageUpgrade", 0);
        playerStats.speed += PlayerPrefs.GetFloat("PermanentSpeedUpgrade", 0);
        playerStats.health += PlayerPrefs.GetFloat("PermanentHealthUpgrade", 0);

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

        target.rectTransform.DOKill();
        target.DOKill();

        // Snap back if cursed
        if (target.rectTransform.localScale.magnitude > 2f)
            target.rectTransform.localScale = Vector3.one;

        if (target.color.maxColorComponent > 1.5f)
            target.color = Color.white;

        // Save the correct local values
        Vector3 trueOriginalScale = target.rectTransform.localScale;
        Vector3 trueOriginalPosition = target.rectTransform.localPosition;
        Color trueOriginalColor = target.color;

        // Animate scaling
        target.rectTransform
            .DOScale(trueOriginalScale * scale, duration / 2f)
            .SetEase(Ease.OutElastic)
            .OnComplete(() =>
            {
                target.rectTransform.DOScale(trueOriginalScale, duration / 2f)
                    .SetEase(Ease.InOutElastic);
            });

        // Animate color flash
        target.DOColor(flashColor, duration / 2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                target.DOColor(trueOriginalColor, duration / 2f)
                    .SetEase(Ease.InQuad);
            });

        // Animate shake
        if (shake > 0f)
        {
            target.rectTransform
                .DOShakeAnchorPos(0.2f, new Vector2(shake, 0f)) // 💥 Use DOShakeAnchorPos for UI!!
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    target.rectTransform.localPosition = trueOriginalPosition; // 🛠 Safely snap after
                });
        }
    }

    private void HandleEnemyDeath()
    {
        AddEnemiesDefeated(1);
        AddCoins(1);
    }

    public void PauseGame()
    {
        savedTimeScale = Time.timeScale; // Save current time scale
        Time.timeScale = 0f;
        if (gameTimer != null) gameTimer.PauseTimer();
        else Debug.LogError("GameTimer is not properly initialized.");
    }


    public void ResumeGame()
    {
        Time.timeScale = savedTimeScale; // Restore saved time scale
        if (gameTimer != null) gameTimer.ResumeTimer();
        else Debug.LogError("GameTimer is not properly initialized.");

        UnfreezeAllProjectiles();
    }


    private void UnfreezeAllProjectiles()
    {
        var bibleProjectiles = FindObjectsOfType<KingBibleProjectile>();
        foreach (var bible in bibleProjectiles)
        {
            bible.enabled = true; // Force re-enable Update movement if frozen
        }

        var wandProjectiles = FindObjectsOfType<MagicWandProjectile>();
        foreach (var wand in wandProjectiles)
        {
            wand.enabled = true; // Force re-enable Update movement if frozen
        }
    }



    public void LoseGame()
    {
        Debug.Log("[GameManager] Player has lost the game!");

        PauseGame();

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.ResetStats();
        else
            Debug.LogWarning("LoseGame: PlayerStats not found!");

        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.ResetAllUpgrades();
        else
            Debug.LogWarning("LoseGame: UpgradeManager not found!");

        if (LoseScreen.Instance != null)
        {
            LoseScreen.Instance.Show();
        }
        else
        {
            Debug.LogWarning("LoseScreen.Instance not found! Attempting FindObjectOfType...");
            LoseScreen screen = FindObjectOfType<LoseScreen>();
            if (screen != null)
            {
                screen.Show();
            }
            else
            {
                Debug.LogError("LoseScreen object still not found after fallback!");
            }
        }
    }


    private bool isDoubleSpeed = false;
    private bool isTripleSpeed = false;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift)) // or RightShift if you prefer
        {
            ToggleDoubleSpeed();
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl)) // CTRL for 3x toggle
        {
            ToggleTripleSpeed();
        }
    }

    private void ToggleDoubleSpeed()
    {
        if (isDoubleSpeed)
        {
            Time.timeScale = 1f;
            savedTimeScale = 1f; // Update saved time scale too!
            isDoubleSpeed = false;
        }
        else
        {
            Time.timeScale = 2f;
            savedTimeScale = 2f; // Update saved time scale too!
            isDoubleSpeed = true;
            isTripleSpeed = false;
        }
    }

    private void ToggleTripleSpeed()
    {
        if (isTripleSpeed)
        {
            Time.timeScale = 1f;
            savedTimeScale = 1f; // Update saved time scale too!
            isTripleSpeed = false;
        }
        else
        {
            Time.timeScale = 3f;
            savedTimeScale = 3f; // Update saved time scale too!
            isTripleSpeed = true;
            isDoubleSpeed = false;
        }
    }
}