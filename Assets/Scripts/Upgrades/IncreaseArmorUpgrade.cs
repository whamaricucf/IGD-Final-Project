using UnityEngine;

[CreateAssetMenu(fileName = "NewIncreaseArmorUpgrade", menuName = "Upgrades/Increase Armor")]
public class IncreaseArmorUpgrade : PassiveUpgrade
{
    public IncreaseArmorUpgrade(string name, string[] descriptions, int maxLevel, int armorIncrease)
        : base(name, descriptions, maxLevel, "Armor", armorIncrease)
    {
        // Custom constructor
    }

    public override void ApplyUpgrade(PlayerStats playerStats)
    {
        playerStats.armor += upgradeAmount;
        playerStats.armorLevel++; // Track the upgrade level for armor
        Debug.Log($"Armor increased! Current armor: {playerStats.armor}");
    }
}
