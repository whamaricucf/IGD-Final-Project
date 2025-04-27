using UnityEngine;
using static UpgradeTypes;

[CreateAssetMenu(fileName = "NewPassiveUpgrade", menuName = "Upgrades/Passive Upgrade")]
public class PassiveUpgradeSO : UpgradeSO
{
    [System.Serializable]
    public struct PassiveUpgradeLevelData
    {
        public PassiveUpgradeType upgradeType;
        public float upgradeAmount;
        public bool isPercentageBased;
        public string description;
    }

    [Header("Passive Upgrade Settings")]
    public PassiveUpgradeLevelData[] upgradeLevels;

    private int currentLevel = 0;

    public override void ApplyUpgrade(PlayerStats playerStats)
    {
        if (playerStats == null)
        {
            Debug.LogError("PassiveUpgradeSO: PlayerStats is null when applying upgrade!");
            return;
        }

        if (currentLevel >= upgradeLevels.Length)
        {
            Debug.LogWarning($"PassiveUpgradeSO: Attempted to upgrade beyond max level for {upgradeName}.");
            return;
        }

        var data = upgradeLevels[currentLevel];

        playerStats.ApplyStatUpgrade(ConvertToStatType(data.upgradeType), data.upgradeAmount, data.isPercentageBased);

        currentLevel++;
    }

    private PlayerStats.StatType ConvertToStatType(PassiveUpgradeType passiveType)
    {
        return passiveType switch
        {
            PassiveUpgradeType.Health => PlayerStats.StatType.Health,
            PassiveUpgradeType.Damage => PlayerStats.StatType.Damage,
            PassiveUpgradeType.MoveSpeed => PlayerStats.StatType.Speed,
            PassiveUpgradeType.Luck => PlayerStats.StatType.Luck,
            PassiveUpgradeType.Regen => PlayerStats.StatType.Regen,
            PassiveUpgradeType.Magnet => PlayerStats.StatType.Magnet,
            PassiveUpgradeType.Armor => PlayerStats.StatType.Armor,
            PassiveUpgradeType.Growth => PlayerStats.StatType.Growth,
            PassiveUpgradeType.Revival => PlayerStats.StatType.Revival,
            PassiveUpgradeType.Amount => PlayerStats.StatType.Amount,
            _ => throw new System.ArgumentOutOfRangeException(nameof(passiveType), passiveType, null),
        };
    }

    public override void ApplyUpgrade(IWeaponUpgradeable weapon)
    {
        // Passive upgrades do not apply to weapons.
    }

    public override int GetCurrentLevel() => currentLevel;

    public override bool IsMaxLevel() => currentLevel >= maxLevel;

    public override void ResetUpgradeLevel() => currentLevel = 0;

    public override string GetUpgradeDescription(int level)
    {
        if (upgradeLevels != null && level < upgradeLevels.Length)
            return upgradeLevels[level].description;
        else
            return "Max level reached!";
    }
}
