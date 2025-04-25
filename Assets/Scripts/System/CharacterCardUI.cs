using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CharacterCardUI : MonoBehaviour
{
    [Header("Character Info")]
    public string characterID;
    public string displayName;
    public Sprite characterPortraitSprite;
    public string weaponName;
    public Sprite weaponIcon;

    [Header("UI References")]
    public Image characterPortrait;
    public Image weaponIconImage;
    public GameObject lockOverlay;
    public GameObject lockIcon;
    public Button selectButton;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI weaponNameText;

    private void Start()
    {
        ApplyData();
    }

    public void ApplyData()
    {
        SaveData save = SaveManager.Load();
        bool isUnlocked = characterID == "A" || save.unlockedCharacters.Contains(characterID);

        lockOverlay.SetActive(!isUnlocked);
        lockIcon.SetActive(!isUnlocked);
        selectButton.interactable = isUnlocked;

        // Set visual elements
        characterPortrait.sprite = characterPortraitSprite;
        nameText.text = displayName;
        weaponNameText.text = weaponName;
        weaponIconImage.sprite = weaponIcon;

        // Adjust text color based on unlock
        nameText.color = isUnlocked ? Color.white : Color.gray;
        weaponNameText.color = isUnlocked ? Color.white : Color.gray;
    }

    public void OnCharacterSelect()
    {
        if (!selectButton.interactable) return;

        PlayerPrefs.SetString("SelectedCharacter", characterID);
        SceneManager.LoadScene("GameScene");
    }
}
