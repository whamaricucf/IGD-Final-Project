using UnityEngine;

[CreateAssetMenu(fileName = "NewIncreaseDamageUpgrade", menuName = "Upgrades/Increase Damage")]
public class IncreaseDamageUpgrade : WeaponUpgrade
{
    public IncreaseDamageUpgrade(string name, string[] descriptions, int maxLevel, int upgradeAmount)
        : base(name, descriptions, maxLevel, "Damage", upgradeAmount)
    {
        // Custom constructor
    }

    public override void ApplyUpgrade(Weapon playerWeapon)
    {
        playerWeapon.damage += upgradeAmount;
        PlayerStats.Instance.damageLevel++; // Track the upgrade level for damage
        Debug.Log($"Weapon damage upgraded to {playerWeapon.damage}");
    }
}
