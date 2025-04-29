using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class LoseScreen : MonoBehaviour
{
    public static LoseScreen Instance;

    [Header("UI References")]
    public GameObject panel;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI enemiesText;
    public TextMeshProUGUI upgradesText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (panel != null)
            panel.SetActive(false); // Hide at start
    }

    public void Show()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            player.SetActive(false);

        if (panel != null)
            panel.SetActive(true);

        UpdateLoseScreenInfo();
    }

    private void UpdateLoseScreenInfo()
    {
        if (coinsText != null)
            coinsText.text = $"Coins Earned: {GameManager.Instance.GetCoinsEarned()}";

        if (enemiesText != null)
            enemiesText.text = $"Enemies Defeated: {GameManager.Instance.GetEnemiesDefeated()}";

        if (upgradesText != null)
            upgradesText.text = GenerateUpgradesList();
    }

    private string GenerateUpgradesList()
    {
        if (UpgradeManager.Instance == null)
            return "No Upgrades Acquired.";

        var pickedUpgrades = UpgradeManager.Instance.GetPickedUpgrades();
        if (pickedUpgrades == null || pickedUpgrades.Count == 0)
            return "No Upgrades Acquired.";

        List<string> weaponLines = new List<string>();
        List<string> passiveLines = new List<string>();

        foreach (var picked in pickedUpgrades)
        {
            if (picked.upgrade == null)
                continue;

            // Instead of picked.level, fetch the current REAL upgrade level!
            int realLevel = picked.upgrade.GetCurrentLevel();

            string line = $"{picked.upgrade.upgradeName} - LV {realLevel}";

            if (picked.upgrade is WeaponUpgradeSO)
                weaponLines.Add(line);
            else
                passiveLines.Add(line);
        }

        // Sort each group alphabetically
        weaponLines.Sort();
        passiveLines.Sort();

        List<string> allLines = new List<string>();
        allLines.AddRange(weaponLines);
        allLines.AddRange(passiveLines);

        return string.Join("\n", allLines);
    }



    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Time.timeScale = 1f;
        LoadMainMenuSceneAndReset();
    }


    private void LoadMainMenuSceneAndReset()
    {
        SaveRunResultsToSaveData(); // Save FIRST

        if (!SceneManager.GetSceneByName("MainMenu").isLoaded)
        {
            SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive).completed += (op) =>
            {
                SetMainMenuScene();
            };
        }
        else
        {
            SetMainMenuScene();
        }
    }

    private void SetMainMenuScene()
    {
        SceneManager.UnloadSceneAsync("GameScene").completed += (op) =>
        {
            Debug.Log("[GameManager] GameScene unloaded after loss.");
        };

        Scene mainMenuScene = SceneManager.GetSceneByName("MainMenu");
        if (mainMenuScene.IsValid())
            SceneManager.SetActiveScene(mainMenuScene);

        GameManager.Instance?.ResetRunData();
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.FullResetAllUpgrades();

        MainMenu menu = FindObjectOfType<MainMenu>();
        if (menu != null)
            menu.UpdateStatsPanel();
    }

    private void SaveRunResultsToSaveData()
    {
        SaveData data = SaveManager.Load();

        data.totalCoins += GameManager.Instance.GetCoinsEarned();

        var enemiesDefeatedDict = SaveManager.ConvertListToDict(data.enemiesDefeated);
        enemiesDefeatedDict.TryGetValue("Total", out int existing);
        enemiesDefeatedDict["Total"] = existing + GameManager.Instance.GetEnemiesDefeated();
        data.enemiesDefeated = SaveManager.ConvertDictToList(enemiesDefeatedDict);

        var highestLevels = SaveManager.ConvertListToDict(data.highestLevelReached);

        string currentChar = PlayerPrefs.GetString("SelectedCharacter", "A");
        int newLevel = PlayerExperience.Instance != null ? PlayerExperience.Instance.currentLevel : 1;

        if (!highestLevels.ContainsKey(currentChar) || newLevel > highestLevels[currentChar])
            highestLevels[currentChar] = newLevel;

        data.highestLevelReached = SaveManager.ConvertDictToList(highestLevels);

        SaveManager.Save(data);
    }
}
