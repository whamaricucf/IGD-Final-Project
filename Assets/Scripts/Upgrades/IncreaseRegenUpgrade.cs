using UnityEngine;

[CreateAssetMenu(fileName = "NewIncreaseRegenUpgrade", menuName = "Upgrades/Increase Regen")]
public class IncreaseRegenUpgrade : PassiveUpgrade
{
    public IncreaseRegenUpgrade(string name, string[] descriptions, int maxLevel, float regenIncreaseAmount)
        : base(name, descriptions, maxLevel, "Regen", (int)regenIncreaseAmount)
    {
        // Custom constructor
    }

    public override void ApplyUpgrade(PlayerStats playerStats)
    {
        playerStats.regen += upgradeAmount;
        playerStats.regenLevel++; // Track the upgrade level for regen
        Debug.Log($"Regen increased! New regen rate: {playerStats.regen}");
    }
}
