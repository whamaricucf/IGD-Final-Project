using UnityEngine;

public class PassiveUpgrade : Upgrade
{
    public string upgradeType;  // Type of passive upgrade (e.g., "Health", "Speed")
    public int upgradeAmount;   // Amount of change to the stat (e.g., +5 health)

    // Constructor to initialize the upgrade
    public PassiveUpgrade(string name, string[] descriptions, int maxLevel, string upgradeType, int upgradeAmount)
    {
        this.upgradeName = name;
        this.upgradeDescriptions = descriptions;
        this.maxLevel = maxLevel;
        this.upgradeType = upgradeType;
        this.upgradeAmount = upgradeAmount;
    }

    // Applies the passive upgrade to PlayerStats
    public override void ApplyUpgrade(PlayerStats playerStats)
    {
        switch (upgradeType)
        {
            case "Health":
                playerStats.IncreaseHealth();  // Increase player's health level
                Debug.Log($"Health upgraded to level {playerStats.healthLevel}");
                break;

            case "Speed":
                playerStats.IncreaseSpeed();  // Increase player's speed level
                Debug.Log($"Speed upgraded to level {playerStats.speedLevel}");
                break;

            case "Damage":
                playerStats.IncreaseDamage();  // Increase player's damage level
                Debug.Log($"Damage upgraded to level {playerStats.damageLevel}");
                break;

            case "Regen":
                playerStats.IncreaseRegen();  // Increase player's regeneration level
                Debug.Log($"Regen upgraded to level {playerStats.regenLevel}");
                break;

            case "Magnet":
                playerStats.IncreaseMagnet(1);  // Assuming a value of 1 for now, you can modify this
                Debug.Log($"Magnet range upgraded to level {playerStats.magnetRangeLevel}");
                break;

            case "Luck":
                playerStats.IncreaseLuck(1);  // Assuming a value of 1 for luck
                Debug.Log($"Luck upgraded to level {playerStats.luckLevel}");
                break;

            case "Armor":
                playerStats.IncreaseArmor(1);  // Assuming a value of 1 for armor
                Debug.Log($"Armor upgraded to level {playerStats.armorLevel}");
                break;

            case "Growth":
                playerStats.ApplyExperienceMultiplier(1);  // Assuming a value of 1 for growth multiplier
                Debug.Log($"Experience multiplier upgraded to {playerStats.growth}");
                break;

            case "Revival":
                playerStats.IncreaseRevival();  // Increase player's revival level
                Debug.Log($"Revival upgraded to level {playerStats.revivalLevel}");
                break;

            default:
                Debug.LogWarning($"Unknown passive upgrade: {upgradeType}");
                break;
        }
    }

    // Weapon upgrades do not apply to PlayerStats, so this method is empty
    public override void ApplyUpgrade(Weapon playerWeapon)
    {
        // Do nothing here as PassiveUpgrades do not apply to weapons
    }
}
