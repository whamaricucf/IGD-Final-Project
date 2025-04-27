using UnityEngine;

public abstract class UpgradeSO : ScriptableObject
{
    [Header("General Upgrade Info")]
    public string upgradeName;
    public Sprite icon;
    [Tooltip("Descriptions for each level of this upgrade.")]
    public string[] upgradeDescriptions;
    [Tooltip("Weight for random upgrade selection (higher = more common).")]
    public float weight = 1f;

    [Header("Upgrade Settings")]
    [Tooltip("Maximum upgrade levels allowed.")]
    public int maxLevel = 5;
    public virtual int GetMaxLevel() => maxLevel;

    public abstract void ApplyUpgrade(PlayerStats playerStats);
    public abstract void ApplyUpgrade(IWeaponUpgradeable weapon);

    public virtual string GetUpgradeDescription(int level)
    {
        if (upgradeDescriptions != null && level < upgradeDescriptions.Length)
            return upgradeDescriptions[level];
        else
            return "Max level reached!";
    }

    public virtual int GetCurrentLevel() => 0;
    public virtual bool IsMaxLevel() => false;
    public virtual void ResetUpgradeLevel() { }
}
