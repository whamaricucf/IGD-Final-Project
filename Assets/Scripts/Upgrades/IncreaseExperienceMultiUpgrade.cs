using UnityEngine;

[CreateAssetMenu(fileName = "NewIncreaseExperienceMultiUpgrade", menuName = "Upgrades/Increase Experience Multiplier")]
public class IncreaseExperienceMultiUpgrade : PassiveUpgrade
{
    public IncreaseExperienceMultiUpgrade(string name, string[] descriptions, int maxLevel, float experienceMultiIncreasePercentage)
        : base(name, descriptions, maxLevel, "ExperienceMulti", (int)(experienceMultiIncreasePercentage * 100))  // Percentage as int
    {
        // Custom constructor
    }

    public override void ApplyUpgrade(PlayerStats playerStats)
    {
        playerStats.growth += upgradeAmount / 100f; // Apply experience multiplier increase as percentage
        playerStats.growthLevel++; // Track the upgrade level for experience multiplier
        Debug.Log($"Experience multiplier increased! New multiplier: {playerStats.growth}");
    }
}
