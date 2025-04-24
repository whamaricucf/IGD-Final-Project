using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public PlayerData charA, charB, charC;

    public void ChooseCharA() => CharacterSelector.Instance.SelectCharacter(charA);
    public void ChooseCharB() => CharacterSelector.Instance.SelectCharacter(charB);
    public void ChooseCharC() => CharacterSelector.Instance.SelectCharacter(charC);

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }
}
