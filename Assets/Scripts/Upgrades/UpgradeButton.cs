using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButton : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI upgradeNameText;  // To display the upgrade's name
    public Image upgradeIcon;  // To display the upgrade's icon
    public TextMeshProUGUI upgradeDescriptionText;  // To show the upgrade's description inside the button

    private Upgrade upgrade;

    // Set the upgrade details for the button
    public void SetUpgradeDetails(Upgrade upgradeData, int currentLevel)
    {
        upgrade = upgradeData;

        // Set the UI elements with the upgrade's info
        upgradeNameText.text = upgrade.upgradeName;
        upgradeIcon.sprite = upgrade.icon;

        // Set the description based on the current upgrade level
        upgradeDescriptionText.text = upgrade.GetUpgradeDescription(currentLevel);
    }

    // Add an onClick listener for when the button is clicked
    public void OnUpgradeButtonClick()
    {
        // Assuming UpgradeManager is in the scene, apply the selected upgrade
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.ApplyUpgrade(upgrade);
        }
    }
}
