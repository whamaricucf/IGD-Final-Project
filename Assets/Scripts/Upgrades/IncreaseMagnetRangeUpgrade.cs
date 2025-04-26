using UnityEngine;

[CreateAssetMenu(fileName = "NewIncreaseMagnetRangeUpgrade", menuName = "Upgrades/Increase Magnet Range")]
public class IncreaseMagnetRangeUpgrade : PassiveUpgrade
{
    public float[] magnetRangeMultipliers; // Array of magnet range multipliers for each level
    public new int maxLevel = 5;

    public IncreaseMagnetRangeUpgrade(string name, string[] descriptions, int maxLevel, float[] magnetRangeMultipliers)
        : base(name, descriptions, maxLevel, "MagnetRange", 0)  // Magnet range is handled via the multiplier array
    {
        this.magnetRangeMultipliers = magnetRangeMultipliers;
    }

    public override void ApplyUpgrade(Weapon weapon)
    {
        if (PlayerStats.Instance != null)
        {
            int currentLevel = PlayerStats.Instance.magnetRangeLevel;

            if (currentLevel < magnetRangeMultipliers.Length)
            {
                PlayerStats.Instance.IncreaseMagnet(magnetRangeMultipliers[currentLevel]);
                PlayerStats.Instance.magnetRangeLevel++; // Track the upgrade level for magnet range
                Debug.Log($"Magnet range increased by {magnetRangeMultipliers[currentLevel]}! New magnet range: {PlayerStats.Instance.magnet}");
            }
            else
            {
                Debug.LogWarning("Magnet range upgrade level exceeds available multipliers.");
            }
        }
    }

    public override void ApplyUpgrade(PlayerStats playerStats) { }
}
