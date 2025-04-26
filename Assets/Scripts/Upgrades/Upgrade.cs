using UnityEngine;

public abstract class Upgrade : ScriptableObject
{
    public string upgradeName;                // Name of the upgrade (e.g., "Damage", "Speed")
    public Sprite icon;                       // Icon for the upgrade
    public string[] upgradeDescriptions;      // Array of descriptions for each level
    public float weight;                      // Weight of the upgrade (if needed for any specific calculations)
    public virtual int maxLevel { get; set; } // Max level of the upgrade, can be overridden by derived classes

    // Abstract methods for applying upgrades to player weapon and player stats
    public abstract void ApplyUpgrade(Weapon playerWeapon);
    public abstract void ApplyUpgrade(PlayerStats playerStats);

    // Get the description for a specific level
    public string GetUpgradeDescription(int level)
    {
        if (level < upgradeDescriptions.Length)
        {
            return upgradeDescriptions[level];
        }
        else
        {
            return "Max level reached!";
        }
    }
}
