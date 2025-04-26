using UnityEngine;

[CreateAssetMenu(fileName = "NewIncreaseCooldownUpgrade", menuName = "Upgrades/Increase Cooldown")]
public class IncreaseCooldownUpgrade : WeaponUpgrade
{
    public IncreaseCooldownUpgrade(string name, string[] descriptions, int maxLevel, float cooldownReductionPercentage)
        : base(name, descriptions, maxLevel, "Cooldown", (int)(cooldownReductionPercentage * 100))  // Using percentage as integer
    {
    }

    public override void ApplyUpgrade(Weapon weapon)
    {
        weapon.cooldown -= weapon.cooldown * (upgradeAmount / 100f);  // Apply the cooldown reduction as a percentage
        PlayerStats.Instance.cdLevel++;  // Track the upgrade level for cooldown
        Debug.Log($"Cooldown reduced! New cooldown: {weapon.cooldown}");
    }
}
