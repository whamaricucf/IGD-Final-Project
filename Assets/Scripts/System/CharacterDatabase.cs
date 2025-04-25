using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "ScriptableObjects/CharacterDatabase")]
public class CharacterDatabase : ScriptableObject
{
    [System.Serializable]
    public class CharacterEntry
    {
        public string characterID;
        public PlayerData playerStats;
    }

    public List<CharacterEntry> characters;

    private Dictionary<string, PlayerData> characterLookup;

    public void Initialize()
    {
        characterLookup = new Dictionary<string, PlayerData>();
        foreach (var entry in characters)
        {
            characterLookup[entry.characterID] = entry.playerStats;
        }
    }

    public PlayerData GetCharacterData(string id)
    {
        if (characterLookup == null) Initialize();
        return characterLookup.ContainsKey(id) ? characterLookup[id] : null;
    }
}
