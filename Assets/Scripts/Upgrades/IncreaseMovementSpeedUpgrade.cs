using UnityEngine;

[CreateAssetMenu(fileName = "NewIncreaseMovementSpeedUpgrade", menuName = "Upgrades/Increase Movement Speed")]
public class IncreaseMovementSpeedUpgrade : PassiveUpgrade
{
    public IncreaseMovementSpeedUpgrade(string name, string[] descriptions, int maxLevel, float movementSpeedIncreasePercentage)
        : base(name, descriptions, maxLevel, "MovementSpeed", (int)(movementSpeedIncreasePercentage * 100))  // Percentage stored as int
    {
        // Custom constructor
    }

    public override void ApplyUpgrade(PlayerStats playerStats)
    {
        playerStats.speed += playerStats.speed * (upgradeAmount / 100f); // Apply speed increase as percentage
        playerStats.speedLevel++; // Track the upgrade level for movement speed
        Debug.Log($"Movement speed increased! New speed: {playerStats.speed}");
    }
}
