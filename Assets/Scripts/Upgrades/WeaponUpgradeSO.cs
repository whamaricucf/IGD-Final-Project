using UnityEngine;
using System.Collections.Generic;
using static UpgradeTypes;

[CreateAssetMenu(fileName = "NewWeaponUpgrade", menuName = "Upgrades/Weapon Upgrade")]
public class WeaponUpgradeSO : UpgradeSO
{
    [System.Serializable]
    public struct WeaponUpgradeLevelData
    {
        public WeaponUpgradeType[] upgradeTypes;
        public float[] upgradeAmounts;
        public bool[] isPercentageBased;
        public string description;
    }

    [Header("Weapon Upgrade Settings")]
    public WeaponUpgradeLevelData[] upgradeLevels;

    [Header("Compatible Weapon Tags (optional)")]
    public List<string> compatibleWeaponTags;

    private int currentLevel = 0;

    public override void ApplyUpgrade(PlayerStats playerStats)
    {
        // Weapon upgrades do not apply to PlayerStats.
    }

    public override void ApplyUpgrade(IWeaponUpgradeable weapon)
    {
        if (weapon == null)
        {
            Debug.LogError("WeaponUpgradeSO: IWeaponUpgradeable weapon is null when applying upgrade!");
            return;
        }

        if (IsMaxLevel())
        {
            Debug.LogWarning($"WeaponUpgradeSO: Attempted to upgrade beyond max level for {upgradeName}.");
            return;
        }

        var data = upgradeLevels[currentLevel];

        for (int i = 0; i < data.upgradeTypes.Length; i++)
        {
            WeaponUpgradeType type = data.upgradeTypes[i];
            float amount = (data.upgradeAmounts != null && data.upgradeAmounts.Length > i) ? data.upgradeAmounts[i] : 0f;
            bool isPercentage = (data.isPercentageBased != null && data.isPercentageBased.Length > i) ? data.isPercentageBased[i] : false;

            weapon.ApplyWeaponUpgrade(type, amount, isPercentage);
        }

        currentLevel++;
    }

    public override int GetCurrentLevel()
    {
        return currentLevel;
    }

    public override bool IsMaxLevel()
    {
        return currentLevel >= maxLevel;
    }

    public override void ResetUpgradeLevel()
    {
        currentLevel = 0;
    }

    public override string GetUpgradeDescription(int level)
    {
        if (upgradeLevels != null && upgradeLevels.Length > 0)
        {
            if (level >= 0 && level < upgradeLevels.Length)
                return upgradeLevels[level].description;
            else if (upgradeLevels.Length > 0)
                return "Max level reached!";
        }

        return "No upgrades available.";
    }

    public void SetCurrentLevel(int level)
    {
        currentLevel = Mathf.Clamp(level, 0, maxLevel);
    }
}