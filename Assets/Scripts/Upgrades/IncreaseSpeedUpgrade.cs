using UnityEngine;

[CreateAssetMenu(fileName = "NewIncreaseSpeedUpgrade", menuName = "Upgrades/Increase Speed")]
public class IncreaseSpeedUpgrade : WeaponUpgrade
{
    public IncreaseSpeedUpgrade(string name, string[] descriptions, int maxLevel, int upgradeAmount)
        : base(name, descriptions, maxLevel, "Speed", upgradeAmount)
    {
        // Custom constructor
    }

    public override void ApplyUpgrade(Weapon playerWeapon)
    {
        playerWeapon.speed += upgradeAmount;
        PlayerStats.Instance.speedLevel++; // Track the upgrade level for speed
        Debug.Log($"Weapon speed upgraded to {playerWeapon.speed}");
    }
}
