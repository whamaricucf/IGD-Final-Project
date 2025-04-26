using UnityEngine;

[CreateAssetMenu(fileName = "NewIncreaseHealthUpgrade", menuName = "Upgrades/Increase Health")]
public class IncreaseHealthUpgrade : PassiveUpgrade
{
    public IncreaseHealthUpgrade(string name, string[] descriptions, int maxLevel, int upgradeAmount)
        : base(name, descriptions, maxLevel, "Health", upgradeAmount)
    {
        // Custom constructor
    }

    public override void ApplyUpgrade(PlayerStats playerStats)
    {
        playerStats.health += playerStats.health * (upgradeAmount / 100f); // Health increase as a percentage
        playerStats.healthLevel++; // Track the upgrade level for health
        Debug.Log($"Health upgraded! New health: {playerStats.health}");
    }
}
