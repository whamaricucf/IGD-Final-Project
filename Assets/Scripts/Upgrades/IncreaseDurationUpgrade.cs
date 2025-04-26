using UnityEngine;

[CreateAssetMenu(fileName = "NewIncreaseDurationUpgrade", menuName = "Upgrades/Increase Duration")]
public class IncreaseDurationUpgrade : WeaponUpgrade
{
    public IncreaseDurationUpgrade(string name, string[] descriptions, int maxLevel, float durationIncreasePercentage)
        : base(name, descriptions, maxLevel, "Duration", (int)(durationIncreasePercentage * 100))  // Using percentage as integer
    {
    }

    public override void ApplyUpgrade(Weapon weapon)
    {
        weapon.duration += weapon.duration * (upgradeAmount / 100f);  // Apply the duration increase as a percentage
        PlayerStats.Instance.durationLevel++;  // Track the upgrade level for duration
        Debug.Log($"Attack duration increased! New duration: {weapon.duration}");
    }
}
