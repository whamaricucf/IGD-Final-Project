using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using DG.Tweening;
using UnityEngine.UI;

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            GameObject existingPlayer = GameObject.FindWithTag("Player");

            if (existingPlayer != null)
            {
                Destroy(existingPlayer);
            }

            GameObject playerPrefab = Resources.Load<GameObject>("Player");
            if (playerPrefab != null)
            {
                GameObject newPlayer = Instantiate(playerPrefab, new Vector3(150, 1.58f, 150), Quaternion.identity);
                newPlayer.tag = "Player";

                // MOVE PLAYER INTO GameScene
                SceneManager.MoveGameObjectToScene(newPlayer, SceneManager.GetSceneByName("GameScene"));

                AssignPlayerHUDReferences(newPlayer);
            }
            else
            {
                Debug.LogError("Player prefab not found in Resources folder!");
                return;
            }

            // After spawning, continue normal initialization
            GameObject player = GameObject.FindWithTag("Player");

            string selectedCharacterID = PlayerPrefs.GetString("SelectedCharacter", "A");
            PlayerData stats = characterDatabase.GetCharacterData(selectedCharacterID);

            PlayerStats.Instance.InitializeStats(stats);

            EquipStartingWeapon();

            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.ForceRefreshStats();
                playerHealth.currentHealth = playerHealth.maxHealth;
                playerHealth.UpdateUI();
            }

            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.inputMoveX = 0;
                playerController.inputMoveY = 0;
            }

            GameObject magnetZone = GameObject.FindWithTag("Magnet");
            if (magnetZone == null)
            {
                Debug.LogError("GameManager: MagnetZone not found!");
            }

            ApplyPermanentUpgrades();

            coinsEarnedText = GameObject.FindWithTag("CoinsText")?.GetComponent<TextMeshProUGUI>();
            enemiesDefeatedText = GameObject.FindWithTag("EnemiesText")?.GetComponent<TextMeshProUGUI>();

            StartCoroutine(DelayedUIInit());

            GameObject timerObject = GameObject.FindWithTag("GameTimer");
            if (timerObject != null)
            {
                gameTimer = timerObject.GetComponent<GameTimer>();
            }
            else
            {
                Debug.LogError("GameTimer not found!");
            }
        }
    }

    private void AssignPlayerHUDReferences(GameObject player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        PlayerExperience playerExp = player.GetComponent<PlayerExperience>();

        GameObject healthBarObj = GameObject.FindWithTag("HealthBar");
        GameObject expBarObj = GameObject.FindWithTag("ExpBar");
        GameObject expTextObj = GameObject.FindWithTag("ExpText");

        if (playerHealth != null && healthBarObj != null)
        {
            playerHealth.healthBar = healthBarObj.GetComponent<Slider>();
        }

        if (playerExp != null)
        {
            if (expBarObj != null)
                playerExp.expBar = expBarObj.GetComponent<Slider>();

            if (expTextObj != null)
                playerExp.expText = expTextObj.GetComponent<TextMeshProUGUI>();
        }
    }


    private IEnumerator DelayedUIInit()
    {
        yield return null;
        UpdateCoinsUI();
        UpdateEnemiesDefeatedUI();
        uiInitialized = true;
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
        AnimateText(coinsEarnedText, Color.yellow);
    }

    public void AddEnemiesDefeated(int amount)
    {
        enemiesDefeated += amount;
        if (uiInitialized) UpdateEnemiesDefeatedUI();
        AnimateText(enemiesDefeatedText, Color.red);
    }

    private void UpdateCoinsUI()
    {
        if (coinsEarnedText != null)
            coinsEarnedText.text = coinsEarned.ToString();
    }

    private void UpdateEnemiesDefeatedUI()
    {
        if (enemiesDefeatedText != null)
            enemiesDefeatedText.text = enemiesDefeated.ToString();
    }

    private void AnimateText(TextMeshProUGUI text, Color flashColor)
    {
        if (text == null) return;
        text.rectTransform.DOKill();
        text.DOKill();

        Vector3 originalScale = text.rectTransform.localScale;
        Vector3 originalPos = text.rectTransform.localPosition;
        Color originalColor = text.color;

        text.rectTransform.DOScale(originalScale * 1.2f, 0.2f).SetEase(Ease.OutElastic).OnComplete(() =>
        {
            text.rectTransform.DOScale(originalScale, 0.2f).SetEase(Ease.InOutElastic);
        });

        text.DOColor(flashColor, 0.2f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            text.DOColor(originalColor, 0.2f).SetEase(Ease.InQuad);
        });

        text.rectTransform.DOShakeAnchorPos(0.2f, new Vector2(5f, 0f)).OnComplete(() =>
        {
            text.rectTransform.localPosition = originalPos;
        });
    }

    private void HandleEnemyDeath()
    {
        AddEnemiesDefeated(1);
        AddCoins(1);
    }

    public void PauseGame()
    {
        savedTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        gameTimer?.PauseTimer();
    }

    public void ResumeGame()
    {
        Time.timeScale = savedTimeScale;
        gameTimer?.ResumeTimer();
        UnfreezeAllProjectiles();
    }

    private void UnfreezeAllProjectiles()
    {
        foreach (var bible in FindObjectsOfType<KingBibleProjectile>())
            bible.enabled = true;

        foreach (var wand in FindObjectsOfType<MagicWandProjectile>())
            wand.enabled = true;
    }

    public void LoseGame()
    {
        Debug.Log("[GameManager] Player has lost the game!");

        // Save player run results
        SaveData data = SaveManager.Load();

        data.totalCoins += coinsEarned;

        var enemiesDefeatedDict = SaveManager.ConvertListToDict(data.enemiesDefeated);
        if (!enemiesDefeatedDict.ContainsKey("Total"))
            enemiesDefeatedDict["Total"] = 0;
        enemiesDefeatedDict["Total"] += enemiesDefeated;
        data.enemiesDefeated = SaveManager.ConvertDictToList(enemiesDefeatedDict);

        string selectedChar = PlayerPrefs.GetString("SelectedCharacter", "A");
        var highestLevels = SaveManager.ConvertListToDict(data.highestLevelReached);

        int runLevel = PlayerExperience.Instance != null ? PlayerExperience.Instance.currentLevel : 1;
        if (!highestLevels.ContainsKey(selectedChar))
            highestLevels[selectedChar] = runLevel;
        else
            highestLevels[selectedChar] = Mathf.Max(highestLevels[selectedChar], runLevel);

        data.highestLevelReached = SaveManager.ConvertDictToList(highestLevels);

        SaveManager.Save(data);

        // Pause gameplay
        PauseGame();

        // Reset stats/upgrades for next run
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.ResetStats();

        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.ResetAllUpgrades();

        // Show Lose screen (before unloading scene)
        if (LoseScreen.Instance != null)
            LoseScreen.Instance.Show();
    }


    private bool isDoubleSpeed = false;
    private bool isTripleSpeed = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            ToggleDoubleSpeed();
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            ToggleTripleSpeed();
        }
    }

    private void ToggleDoubleSpeed()
    {
        if (isDoubleSpeed)
        {
            Time.timeScale = 1f;
            savedTimeScale = 1f;
            isDoubleSpeed = false;
        }
        else
        {
            Time.timeScale = 2f;
            savedTimeScale = 2f;
            isDoubleSpeed = true;
            isTripleSpeed = false;
        }
    }

    private void ToggleTripleSpeed()
    {
        if (isTripleSpeed)
        {
            Time.timeScale = 1f;
            savedTimeScale = 1f;
            isTripleSpeed = false;
        }
        else
        {
            Time.timeScale = 3f;
            savedTimeScale = 3f;
            isTripleSpeed = true;
            isDoubleSpeed = false;
        }
    }

    public int GetCoinsEarned() => coinsEarned;
    public int GetEnemiesDefeated() => enemiesDefeated;

    public void ResetRunData()
    {
        coinsEarned = 0;
        enemiesDefeated = 0;
    }

    public void EquipStartingWeapon()
    {
        string selectedCharacterID = PlayerPrefs.GetString("SelectedCharacter", "A");

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[EquipStartingWeapon] Player not found! Aborting.");
            return;
        }

        MagicWand magicWand = player.GetComponentInChildren<MagicWand>(true);
        KingBible kingBible = player.GetComponentInChildren<KingBible>(true);
        GarlicAura garlicAura = player.GetComponentInChildren<GarlicAura>(true);

        if (magicWand != null) magicWand.gameObject.SetActive(false);
        if (kingBible != null) kingBible.gameObject.SetActive(false);
        if (garlicAura != null) garlicAura.gameObject.SetActive(false);

        switch (selectedCharacterID)
        {
            case "A":
                if (magicWand != null) magicWand.gameObject.SetActive(true);
                break;
            case "B":
                if (kingBible != null) kingBible.gameObject.SetActive(true);
                break;
            case "C":
                if (garlicAura != null) garlicAura.gameObject.SetActive(true);
                break;
            default:
                Debug.LogWarning($"Unknown character ID {selectedCharacterID}. Defaulting to Magic Wand.");
                if (magicWand != null) magicWand.gameObject.SetActive(true);
                break;
        }

        // AFTER setting the active weapon, refresh upgrades manually
        UpgradeManager.Instance?.InitializeStartingWeaponLevels();
    }

}