using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassiveUpgradesHUD : MonoBehaviour
{
    public static PassiveUpgradesHUD Instance;

    private PassiveUpgradeIconHandler iconHandler;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        iconHandler = GetComponentInChildren<PassiveUpgradeIconHandler>(true);

        if (iconHandler == null)
            Debug.LogError("PassiveUpgradesHUD: No PassiveUpgradeIconHandler found!");
    }

    public void RegisterPassiveUpgrade(string upgradeName, Sprite icon, int level)
    {
        if (iconHandler != null)
            iconHandler.AddOrUpdatePassiveUpgrade(upgradeName, icon, level);
        else
            Debug.LogWarning($"PassiveUpgradesHUD: Trying to register upgrade {upgradeName} but no icon handler available!");
    }
}
