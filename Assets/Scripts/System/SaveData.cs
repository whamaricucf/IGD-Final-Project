using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeyValue
{
    public string key;
    public int value;
}

[System.Serializable]
public class SaveData
{
    public int totalCoins;
    public List<KeyValue> playerUpgrades = new List<KeyValue>();
    public List<KeyValue> enemiesDefeated = new List<KeyValue>();
    public List<KeyValue> highestLevelReached = new List<KeyValue>();
    public List<string> unlockedCharacters = new List<string>();

    public SaveData()
    {
        totalCoins = 0;
        playerUpgrades = new List<KeyValue>();
        enemiesDefeated = new List<KeyValue>();
        highestLevelReached = new List<KeyValue>();
        unlockedCharacters = new List<string>();
    }
}
