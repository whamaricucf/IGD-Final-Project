using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    public static CharacterSelector Instance { get; private set; }

    public PlayerData selectedPlayerData; // Chosen on New Game screen

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SelectCharacter(PlayerData data)
    {
        selectedPlayerData = data;
    }
}
