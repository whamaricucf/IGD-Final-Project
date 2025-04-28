using UnityEngine;
using System.Collections.Generic;
using static UpgradeTypes;
using System.Linq;

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

    [Header("Summon Settings")]
    public bool isWeaponSummonUpgrade = false;

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
            Debug.LogError($"WeaponUpgradeSO: IWeaponUpgradeable weapon is null when applying upgrade for {upgradeName}!");
            return;
        }

        if (IsMaxLevel())
        {
            Debug.LogWarning($"WeaponUpgradeSO: Attempted to upgrade beyond max level for {upgradeName}.");
            return;
        }

        // Level 1 = unlock, do not apply bonuses yet
        if (currentLevel <= 1)
        {
            Debug.Log($"WeaponUpgradeSO: Skipping applying upgrade effects for {upgradeName} at Level {currentLevel} (unlock only).");
            return;
        }

        // Apply based on (currentLevel - 2), not currentLevel - 1
        var data = upgradeLevels[currentLevel - 2];

        for (int i = 0; i < data.upgradeTypes.Length; i++)
        {
            WeaponUpgradeType type = data.upgradeTypes[i];
            float amount = (data.upgradeAmounts != null && data.upgradeAmounts.Length > i) ? data.upgradeAmounts[i] : 0f;
            bool isPercentage = (data.isPercentageBased != null && data.isPercentageBased.Length > i) ? data.isPercentageBased[i] : false;

            weapon.ApplyWeaponUpgrade(type, amount, isPercentage);
        }

        weapon.ReinitializeWeaponAfterUpgrade();
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
            else
                return "Max level reached!";
        }

        return "No upgrades available.";
    }


    public void SetCurrentLevel(int level)
    {
        currentLevel = Mathf.Clamp(level, 0, maxLevel);
        Debug.Log($"Weapon upgrade level set to {currentLevel} for {upgradeName}");
    }


    public void ApplyLevelUpEffect()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null) return;

        var weapons = playerObject.GetComponentsInChildren<IWeaponUpgradeable>(true);

        foreach (var weapon in weapons)
        {
            if (weapon == null || weapon is not Weapon weaponComponent || weaponComponent.weaponData == null)
                continue; // SKIP

            if (!weaponComponent.gameObject.activeInHierarchy)
                continue; // SKIP inactive weapons

            string weaponTag = weaponComponent.weaponData.wepName.Replace(" ", "");

            if (compatibleWeaponTags.Any(tag => tag.Replace(" ", "") == weaponTag))
            {
                ApplyUpgrade(weapon);
            }
        }

    }

}