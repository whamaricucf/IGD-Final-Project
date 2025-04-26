using UnityEngine;

public class Weapon : MonoBehaviour
{
    // These are the stats that will be set based on the WeaponData asset
    public float damage;  // Weapon's base damage
    public float speed;   // Weapon's speed (projectile speed)
    public float area;    // Weapon's area of effect (e.g., size)
    public float cooldown;  // Cooldown between shots or attacks
    public float duration;  // Duration of effect (e.g., for king bible)
    public float projInterval; // Projectile firing interval
    public float hitDelay;  // Hit delay (time between attacks)
    public float knockback;  // Knockback dealt by the weapon/projectiles
    public float critChance; // Critical hit chance
    public float critMulti;  // Critical damage multiplier
    public int maxLvl; // Maximum level for upgrades
    public int rarity; // Rarity for upgrades
    public int amount;  // Number of projectiles fired
    public int pierce;  // Number of enemies the projectile can pierce
    public int limit;   // Maximum number of projectiles on screen at a time
    public bool wallBlock; // Whether the projectile can be blocked by walls

    // New field for weaponType
    public string weaponType; // The type of the weapon (KingBible, MagicWand, etc.)

    // Upgrade levels for each upgrade type
    private int magnetRangeLevel = 0;
    private int damageLevel = 0;
    private int areaLevel = 0;
    private int speedLevel = 0;
    private int projectilesLevel = 0;
    private int cooldownLevel = 0;
    private int healthLevel = 0;
    private int revivalLevel = 0;

    // Reference to the WeaponData ScriptableObject
    public WeaponData weaponData;  // This will hold the data for the weapon's base stats

    void Start()
    {
        // Ensure the weapon data is not null
        if (weaponData != null)
        {
            // Apply the weapon data to set the stats
            ApplyWeaponData();
        }

        // Set the weaponType based on the WeaponData ScriptableObject
        if (weaponData != null)
        {
            weaponType = weaponData.wepName;  // Assign the weaponType from the WeaponData ScriptableObject
        }
    }

    // Apply the values from WeaponData to the Weapon
    public void ApplyWeaponData()
    {
        if (weaponData != null)
        {
            // Assign values from the WeaponData ScriptableObject
            damage = weaponData.baseDMG;
            speed = weaponData.spd;
            area = weaponData.area;
            cooldown = weaponData.cd;
            duration = weaponData.duration;
            projInterval = weaponData.projInterval;
            hitDelay = weaponData.hitDelay;
            knockback = weaponData.knockback;
            critChance = weaponData.critChance;
            critMulti = weaponData.critMulti;
            maxLvl = weaponData.maxLvl;
            rarity = weaponData.rarity;
            amount = weaponData.amount;
            pierce = weaponData.pierce;
            limit = weaponData.limit;
            wallBlock = weaponData.wallBlock;
        }
    }

    // Method to apply the selected upgrade
    public void ApplyUpgrade(Upgrade upgrade)
    {
        upgrade.ApplyUpgrade(this);  // Apply the specific upgrade effect
    }

    // Set upgrade levels when an upgrade is selected
    public void SetUpgradeLevel(string upgradeType, int level)
    {
        switch (upgradeType)
        {
            case "MagnetRange":
                magnetRangeLevel = level;
                break;
            case "Damage":
                damageLevel = level;
                break;
            case "Area":
                areaLevel = level;
                break;
            case "Speed":
                speedLevel = level;
                break;
            case "Projectiles":
                projectilesLevel = level;
                break;
            case "Cooldown":
                cooldownLevel = level;
                break;
            case "Health":
                healthLevel = level;
                break;
            case "Revival":
                revivalLevel = level;
                break;
        }
    }

    // Method to get the current level of a specific upgrade
    public int GetCurrentUpgradeLevel(string upgradeType)
    {
        switch (upgradeType)
        {
            case "MagnetRange":
                return magnetRangeLevel;
            case "Damage":
                return damageLevel;
            case "Area":
                return areaLevel;
            case "Speed":
                return speedLevel;
            case "Projectiles":
                return projectilesLevel;
            case "Cooldown":
                return cooldownLevel;
            case "Health":
                return healthLevel;
            case "Revival":
                return revivalLevel;
            default:
                return 0;  // Return 0 if the upgrade type is not recognized
        }
    }

    // Reset stats when the player dies or when you want to clear upgrades
    public void ResetWeaponStats()
    {
        // Reset values based on WeaponData
        damage = weaponData.baseDMG;
        speed = weaponData.spd;
        area = weaponData.area;
        cooldown = weaponData.cd;
        duration = weaponData.duration;
        projInterval = weaponData.projInterval;
        hitDelay = weaponData.hitDelay;
        knockback = weaponData.knockback;
        critChance = weaponData.critChance;
        critMulti = weaponData.critMulti;
        maxLvl = weaponData.maxLvl;
        rarity = weaponData.rarity;
        amount = weaponData.amount;
        pierce = weaponData.pierce;
        limit = weaponData.limit;
        wallBlock = weaponData.wallBlock;

        // Reset all upgrade levels to 0
        magnetRangeLevel = 0;
        damageLevel = 0;
        areaLevel = 0;
        speedLevel = 0;
        projectilesLevel = 0;
        cooldownLevel = 0;
        healthLevel = 0;
        revivalLevel = 0;
    }

    // Method to get the weapon's type (e.g., "KingBible", "MagicWand", "GarlicAura")
    public string GetWeaponType()
    {
        return weaponType;  // Return the weapon's name from WeaponData
    }
}
