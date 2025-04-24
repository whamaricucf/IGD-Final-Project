using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public List<string> unlockedCharacters = new List<string>();
    public int totalCoins;
    public Dictionary<string, int> playerUpgrades = new Dictionary<string, int>();
    public Dictionary<string, int> enemiesDefeated = new Dictionary<string, int>();

    public SaveData()
    {
        totalCoins = 0;
        enemiesDefeated = new Dictionary<string, int>();
        unlockedCharacters = new List<string>();
        playerUpgrades = new Dictionary<string, int>();
    }
}
