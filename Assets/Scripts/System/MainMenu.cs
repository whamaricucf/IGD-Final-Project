using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public PlayerData charA, charB, charC;

    [Header("Panels")]
    public GameObject characterSelectPanel;
    public GameObject cheatsPanel;

    public void ChooseCharA()
    {
        CharacterSelector.Instance.SelectCharacter(charA);
        StartGame();
    }

    public void ChooseCharB()
    {
        CharacterSelector.Instance.SelectCharacter(charB);
        StartGame();
    }

    public void ChooseCharC()
    {
        CharacterSelector.Instance.SelectCharacter(charC);
        StartGame();
    }

    public void StartGame()
    {
        CloseCheatsPanel(); // Always close cheats when starting game
        SceneManager.LoadScene("GameScene");
    }

    public void OpenCharacterSelect()
    {
        CloseCheatsPanel(); // Always close cheats when opening character select
        characterSelectPanel.SetActive(true);
    }

    public void OpenCheatsPanel()
    {
        CloseCheatsPanel(); // Make sure cheats panel resets cleanly
        cheatsPanel.SetActive(true);
    }

    public void CloseCheatsPanel()
    {
        if (cheatsPanel != null && cheatsPanel.activeSelf)
            cheatsPanel.SetActive(false);
    }

    public void UnlockCharacterB()
    {
        PlayerPrefs.SetInt("UnlockedCharacterB", 1);
        Debug.Log("Character B Unlocked!");
    }

    public void UnlockCharacterC()
    {
        PlayerPrefs.SetInt("UnlockedCharacterC", 1);
        Debug.Log("Character C Unlocked!");
    }

    public void QuitGame()
    {
        CloseCheatsPanel(); // Always close cheats when quitting
        Application.Quit();
        Debug.Log("Quitting game...");
    }
}
