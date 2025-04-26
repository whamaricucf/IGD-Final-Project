using UnityEngine;

public class WeaponUpgrade : Upgrade
{
    public string upgradeType;  // Type of weapon upgrade (e.g., "Damage", "Speed")
    public int upgradeAmount;   // Amount of change to the stat (e.g., +1 Damage)

    // Constructor to initialize the upgrade
    public WeaponUpgrade(string name, string[] descriptions, int maxLevel, string upgradeType, int upgradeAmount)
    {
        this.upgradeName = name;
        this.upgradeDescriptions = descriptions;
        this.maxLevel = maxLevel;
        this.upgradeType = upgradeType;
        this.upgradeAmount = upgradeAmount;
    }

    // Applies the weapon upgrade to a Weapon instance
    public override void ApplyUpgrade(Weapon playerWeapon)
    {
        switch (upgradeType)
        {
            case "Damage":
                playerWeapon.damage += upgradeAmount;
                playerWeapon.damage = Mathf.Min(playerWeapon.damage, playerWeapon.maxLvl); // Ensure damage doesn't exceed max level
                Debug.Log($"Weapon damage upgraded to {playerWeapon.damage}");
                break;

            case "Speed":
                playerWeapon.speed += upgradeAmount;
                playerWeapon.speed = Mathf.Min(playerWeapon.speed, playerWeapon.maxLvl); // Ensure speed doesn't exceed max level
                Debug.Log($"Weapon speed upgraded to {playerWeapon.speed}");
                break;

            case "Area":
                playerWeapon.area += upgradeAmount;
                playerWeapon.area = Mathf.Min(playerWeapon.area, playerWeapon.maxLvl); // Ensure area doesn't exceed max level
                Debug.Log($"Weapon area upgraded to {playerWeapon.area}");
                break;

            case "Cooldown":
                playerWeapon.cooldown -= upgradeAmount; // Assuming cooldown is decreased to make it faster
                playerWeapon.cooldown = Mathf.Max(playerWeapon.cooldown, 0); // Ensure cooldown doesn't go negative
                Debug.Log($"Weapon cooldown upgraded to {playerWeapon.cooldown}");
                break;

            case "Duration":
                playerWeapon.duration += upgradeAmount;
                playerWeapon.duration = Mathf.Min(playerWeapon.duration, playerWeapon.maxLvl); // Ensure duration doesn't exceed max level
                Debug.Log($"Weapon duration upgraded to {playerWeapon.duration}");
                break;

            default:
                Debug.LogWarning($"Unknown weapon upgrade: {upgradeType}");
                break;
        }
    }

    // Passive upgrades do not apply to weapon stats
    public override void ApplyUpgrade(PlayerStats playerStats)
    {
        // Do nothing here as WeaponUpgrades do not apply to PlayerStats
    }
}
