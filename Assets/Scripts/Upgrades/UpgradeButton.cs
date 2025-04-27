using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButton : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI upgradeNameText;
    public Image upgradeIcon;
    public TextMeshProUGUI upgradeDescriptionText;

    private UpgradeSO upgrade;
    private int displayedLevel; // new: store the level shown on the button

    public void SetUpgradeDetails(UpgradeSO upgradeData, int currentLevel)
    {
        upgrade = upgradeData;
        displayedLevel = currentLevel; // store the level being shown

        if (upgrade == null)
        {
            Debug.LogError("UpgradeButton: No upgrade data provided!");
            return;
        }

        upgradeNameText.text = upgrade.upgradeName;
        upgradeIcon.sprite = upgrade.icon;

        if (upgrade.IsMaxLevel())
        {
            upgradeDescriptionText.text = "Max Level Reached!";
        }
        else
        {
            upgradeDescriptionText.text = upgrade.GetUpgradeDescription(displayedLevel); // use displayedLevel
        }
    }

    public void OnUpgradeButtonClick()
    {
        if (UpgradeManager.Instance != null && upgrade != null)
        {
            UpgradeManager.Instance.ApplyUpgrade(upgrade);
        }
        else
        {
            Debug.LogError("UpgradeButton: Missing UpgradeManager or Upgrade!");
        }
    }
}
