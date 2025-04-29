using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    public PlayerData charA, charB, charC;

    [Header("Panels")]
    public GameObject characterSelectPanel;
    public GameObject cheatsPanel;
    public GameObject statsPanel;
    public GameObject mainMenuButtonsPanel;

    [Header("Stats UI References")]
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI enemiesText;
    public TextMeshProUGUI highestLevelCharA;
    public TextMeshProUGUI highestLevelCharB;
    public TextMeshProUGUI highestLevelCharC;

    private CharacterSelectionManager characterSelectionManager;

    private void Start()
    {
        UpdateStatsPanel();
        characterSelectionManager = FindObjectOfType<CharacterSelectionManager>();
    }

    public void UpdateStatsPanel()
    {
        SaveData data = SaveManager.Load();

        // Force update PlayerPrefs unlocks if needed
        if (data.unlockedCharacters.Contains("B") && PlayerPrefs.GetInt("UnlockedCharacterB", 0) == 0)
        {
            PlayerPrefs.SetInt("UnlockedCharacterB", 1);
            PlayerPrefs.Save();
        }
        if (data.unlockedCharacters.Contains("C") && PlayerPrefs.GetInt("UnlockedCharacterC", 0) == 0)
        {
            PlayerPrefs.SetInt("UnlockedCharacterC", 1);
            PlayerPrefs.Save();
        }

        if (coinsText != null)
            coinsText.text = $"Total Coins: {data.totalCoins}";

        var enemiesDefeatedDict = SaveManager.ConvertListToDict(data.enemiesDefeated);
        enemiesDefeatedDict.TryGetValue("Total", out int totalDefeated);

        if (enemiesText != null)
            enemiesText.text = $"Total Enemies Defeated: {totalDefeated}";

        var highestLevelDict = SaveManager.ConvertListToDict(data.highestLevelReached);

        if (highestLevelCharA != null)
            highestLevelCharA.text = $"Wizard Highest Level: {highestLevelDict.GetValueOrDefault("A", 0)}";

        if (highestLevelCharB != null)
        {
            if (PlayerPrefs.GetInt("UnlockedCharacterB", 0) == 1)
            {
                highestLevelCharB.gameObject.SetActive(true);
                highestLevelCharB.text = $"Priest Highest Level: {highestLevelDict.GetValueOrDefault("B", 0)}";
            }
            else
            {
                highestLevelCharB.gameObject.SetActive(false);
            }
        }

        if (highestLevelCharC != null)
        {
            if (PlayerPrefs.GetInt("UnlockedCharacterC", 0) == 1)
            {
                highestLevelCharC.gameObject.SetActive(true);
                highestLevelCharC.text = $"Hermit Highest Level: {highestLevelDict.GetValueOrDefault("C", 0)}";
            }
            else
            {
                highestLevelCharC.gameObject.SetActive(false);
            }
        }
    }

    // --- Character Selection ---

    public void ChooseCharA()
    {
        characterSelectionManager.SelectCharacter("A");
        if (GameManager.Instance != null)
            GameManager.Instance.selectedCharacter = GameManager.CharacterType.A;
        StartGame();
    }

    public void ChooseCharB()
    {
        characterSelectionManager.SelectCharacter("B");
        if (GameManager.Instance != null)
            GameManager.Instance.selectedCharacter = GameManager.CharacterType.B;
        StartGame();
    }

    public void ChooseCharC()
    {
        characterSelectionManager.SelectCharacter("C");
        if (GameManager.Instance != null)
            GameManager.Instance.selectedCharacter = GameManager.CharacterType.C;
        StartGame();
    }


    public void StartGame()
    {
        CloseCheatsPanel();

        // If player already exists and is disabled, re-enable it
        GameObject existingPlayer = GameObject.FindWithTag("Player");
        if (existingPlayer != null && !existingPlayer.activeSelf)
        {
            existingPlayer.SetActive(true);

            existingPlayer.transform.position = new Vector3(150, 1.58f, 150);

            PlayerHealth playerHealth = existingPlayer.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.ForceRefreshStats();
                playerHealth.currentHealth = playerHealth.maxHealth;
                playerHealth.UpdateUI();
            }

            // RESET MOVEMENT
            PlayerController playerController = existingPlayer.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.inputMoveX = 0;
                playerController.inputMoveY = 0;
            }
        }

        SceneManager.LoadScene("GameScene", LoadSceneMode.Additive);
        StartCoroutine(HandleSceneTransition());
        AudioManager.Instance.PlaySceneLoaded();
    }


    private IEnumerator HandleSceneTransition()
    {
        yield return null;

        Scene gameScene = SceneManager.GetSceneByName("GameScene");
        while (!gameScene.isLoaded)
        {
            yield return null;
            gameScene = SceneManager.GetSceneByName("GameScene");
        }

        SceneManager.SetActiveScene(gameScene);

        Scene mainMenuScene = SceneManager.GetSceneByName("MainMenu");
        if (mainMenuScene.IsValid())
            SceneManager.UnloadSceneAsync(mainMenuScene);
    }

    // --- UI Panel Controls ---

    public void OpenCharacterSelect()
    {
        CloseCheatsPanel();
        characterSelectPanel.SetActive(true);
        mainMenuButtonsPanel.SetActive(false);
        statsPanel.SetActive(false);

        RefreshAllCharacterCards();
    }

    public void ReturnToMainMenu()
    {
        characterSelectPanel.SetActive(false);
        cheatsPanel.SetActive(false);
        mainMenuButtonsPanel.SetActive(true);
        statsPanel.SetActive(true);
    }

    public void OpenCheatsPanel()
    {
        if (cheatsPanel != null)
            cheatsPanel.SetActive(!cheatsPanel.activeSelf);
    }

    public void CloseCheatsPanel()
    {
        if (cheatsPanel != null && cheatsPanel.activeSelf)
            cheatsPanel.SetActive(false);
    }

    // --- Unlock Cheats ---

    public void UnlockCharacterB()
    {
        PlayerPrefs.SetInt("UnlockedCharacterB", 1);
        PlayerPrefs.Save();

        SaveData save = SaveManager.Load();
        if (!save.unlockedCharacters.Contains("B"))
            save.unlockedCharacters.Add("B");
        SaveManager.Save(save);

        Debug.Log("[Cheat] Character B unlocked.");
        UpdateStatsPanel();
        RefreshAllCharacterCards();
    }

    public void UnlockCharacterC()
    {
        PlayerPrefs.SetInt("UnlockedCharacterC", 1);
        PlayerPrefs.Save();

        SaveData save = SaveManager.Load();
        if (!save.unlockedCharacters.Contains("C"))
            save.unlockedCharacters.Add("C");
        SaveManager.Save(save);

        Debug.Log("[Cheat] Character C unlocked.");
        UpdateStatsPanel();
        RefreshAllCharacterCards();
    }

    // --- Helper Functions ---

    public void RefreshAllCharacterCards()
    {
        CharacterCardUI.cachedSaveData = null; // Clear old cache
        SaveData debugSave = SaveManager.Load();

        Debug.Log("[SAVE DEBUG] Unlocked Characters:");
        foreach (var unlocked in debugSave.unlockedCharacters)
            Debug.Log($"- {unlocked}");

        CharacterCardUI[] allCards = FindObjectsOfType<CharacterCardUI>(true);
        foreach (var card in allCards)
            card.ApplyData();
    }

    public void ResetSaveData()
    {
        SaveManager.DeleteSave();
        Debug.Log("[Reset] Save data cleared.");

        PlayerPrefs.DeleteKey("UnlockedCharacterB");
        PlayerPrefs.DeleteKey("UnlockedCharacterC");
        PlayerPrefs.DeleteKey("SelectedCharacter");
        PlayerPrefs.Save();

        CharacterCardUI.cachedSaveData = null;
        UpdateStatsPanel();
        RefreshAllCharacterCards();
    }

    public void QuitGame()
    {
        CloseCheatsPanel();
        Application.Quit();
        Debug.Log("Quitting game...");
    }


}