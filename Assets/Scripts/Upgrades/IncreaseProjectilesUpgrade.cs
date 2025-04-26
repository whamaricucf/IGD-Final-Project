using UnityEngine;

[CreateAssetMenu(fileName = "NewIncreaseProjectilesUpgrade", menuName = "Upgrades/Increase Projectiles")]
public class IncreaseProjectilesUpgrade : WeaponUpgrade
{
    public int projectilesIncreaseAmount;  // Amount to increase projectiles per level
    public new int maxLevel = 2;

    // Constructor to initialize the upgrade
    public IncreaseProjectilesUpgrade(string name, string[] descriptions, int maxLevel, int projectilesIncreaseAmount)
        : base(name, descriptions, maxLevel, "Projectiles", projectilesIncreaseAmount)
    {
    }

    public override void ApplyUpgrade(Weapon weapon)
    {
        if (weapon != null)
        {
            // Apply the increase in projectiles to the weapon
            weapon.amount += projectilesIncreaseAmount;
            PlayerStats.Instance.amountLevel++;  // Track the upgrade level for projectiles
            Debug.Log($"Projectiles increased! New projectile count: {weapon.amount}");
        }
    }
}
