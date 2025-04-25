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
        else Destroy(gameObject);
    }

    public void CheckForCharacterUnlocks(string characterID, int newLevel)
    {
        SaveData data = SaveManager.Load();

        // Track highest level
        if (!data.highestLevelReached.ContainsKey(characterID) || newLevel > data.highestLevelReached[characterID])
            data.highestLevelReached[characterID] = newLevel;

        // Unlock B at level 10
        if (!data.unlockedCharacters.Contains("B") && newLevel >= 10)
            data.unlockedCharacters.Add("B");

        // Unlock C if: B reaches 10 OR A reaches 20
        if (!data.unlockedCharacters.Contains("C"))
        {
            bool a20 = data.highestLevelReached.TryGetValue("A", out int aLvl) && aLvl >= 20;
            bool b10 = data.highestLevelReached.TryGetValue("B", out int bLvl) && bLvl >= 10;
            if (a20 || b10)
                data.unlockedCharacters.Add("C");
        }

        SaveManager.Save(data);
    }
}
