using UnityEngine;

public class CharacterSelectionManager : MonoBehaviour
{
    public CharacterDatabase characterDatabase;  // Reference to CharacterDatabase

    private PlayerStats playerStats;

    void Start()
    {
        playerStats = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();  // Get reference to PlayerStats
    }

    public void SelectCharacter(string characterID)
    {
        // Get the PlayerData for the selected character from the CharacterDatabase
        PlayerData selectedPlayerData = characterDatabase.GetCharacterData(characterID);

        // Assign the PlayerData to PlayerStats
        playerStats.playerData = selectedPlayerData;

        // Initialize PlayerStats with the selected PlayerData
        playerStats.InitializeStats(selectedPlayerData);

        // Optionally, you can load the game scene or do other necessary tasks
        // SceneManager.LoadScene("GameScene");
    }
}
