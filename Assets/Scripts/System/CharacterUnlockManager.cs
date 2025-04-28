using UnityEngine;

public class CharacterUnlockManager : MonoBehaviour
{
    public static CharacterUnlockManager Instance;

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

    public void CheckForCharacterUnlocks(string characterID, int newLevel)
    {
        SaveData data = SaveManager.Load();
        var highestLevels = SaveManager.ConvertListToDict(data.highestLevelReached);

        // Track highest level
        if (!highestLevels.ContainsKey(characterID) || newLevel > highestLevels[characterID])
            highestLevels[characterID] = newLevel;

        bool saveNeeded = false;

        // Unlock B at level 10
        if (!data.unlockedCharacters.Contains("B") && newLevel >= 10)
        {
            data.unlockedCharacters.Add("B");
            PlayerPrefs.SetInt("UnlockedCharacterB", 1); // 🛠 Set PlayerPrefs
            saveNeeded = true;
            Debug.Log("[Unlock] Character B unlocked via level 10!");
        }

        // Unlock C if: B reaches 10 OR A reaches 20
        if (!data.unlockedCharacters.Contains("C"))
        {
            bool a20 = highestLevels.TryGetValue("A", out int aLvl) && aLvl >= 20;
            bool b10 = highestLevels.TryGetValue("B", out int bLvl) && bLvl >= 10;
            if (a20 || b10)
            {
                data.unlockedCharacters.Add("C");
                PlayerPrefs.SetInt("UnlockedCharacterC", 1); // 🛠 Set PlayerPrefs
                saveNeeded = true;
                Debug.Log("[Unlock] Character C unlocked!");
            }
        }

        if (saveNeeded)
        {
            data.highestLevelReached = SaveManager.ConvertDictToList(highestLevels);
            SaveManager.Save(data);
            PlayerPrefs.Save();
        }
    }
}
