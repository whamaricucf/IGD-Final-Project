using UnityEngine;

public class CharacterSelectionManager : MonoBehaviour
{
    public void SelectCharacter(string characterID)
    {
        PlayerPrefs.SetString("SelectedCharacter", characterID);
        PlayerPrefs.Save(); // Save immediately to be safe
    }
}
