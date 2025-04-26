using UnityEngine;

[CreateAssetMenu(fileName = "NewIncreaseLuckUpgrade", menuName = "Upgrades/Increase Luck")]
public class IncreaseLuckUpgrade : PassiveUpgrade
{
    public IncreaseLuckUpgrade(string name, string[] descriptions, int maxLevel, float luckIncreasePercentage)
        : base(name, descriptions, maxLevel, "Luck", (int)(luckIncreasePercentage * 100))  // Using percentage increase as integer
    {
        // Custom constructor
    }

    public override void ApplyUpgrade(PlayerStats playerStats)
    {
        playerStats.luck += upgradeAmount / 100f; // Apply luck as a percentage
        playerStats.luckLevel++; // Track the upgrade level for luck
        Debug.Log($"Luck increased! New luck: {playerStats.luck}");
    }
}
