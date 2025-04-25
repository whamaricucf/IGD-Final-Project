using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int totalCoins;
    public Dictionary<string, int> playerUpgrades = new Dictionary<string, int>();
    public Dictionary<string, int> enemiesDefeated = new Dictionary<string, int>();
    public HashSet<string> unlockedCharacters = new HashSet<string>();
    public Dictionary<string, int> highestLevelReached = new Dictionary<string, int>();

    public SaveData()
    {
        totalCoins = 0;
        enemiesDefeated = new Dictionary<string, int>();
        unlockedCharacters = new HashSet<string>();
        playerUpgrades = new Dictionary<string, int>();
    }
}
