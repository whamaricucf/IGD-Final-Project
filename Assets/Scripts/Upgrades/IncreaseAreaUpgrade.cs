using UnityEngine;

[CreateAssetMenu(fileName = "NewIncreaseAreaUpgrade", menuName = "Upgrades/Increase Area")]
public class IncreaseAreaUpgrade : WeaponUpgrade
{
    public IncreaseAreaUpgrade(string name, string[] descriptions, int maxLevel, float areaIncreasePercentage)
        : base(name, descriptions, maxLevel, "Area", (int)(areaIncreasePercentage * 100))  // Using percentage as integer
    {
    }

    public override void ApplyUpgrade(Weapon weapon)
    {
        weapon.area += weapon.area * (upgradeAmount / 100f);  // Apply the area increase as a percentage
        PlayerStats.Instance.areaLevel++;  // Track the upgrade level for area
        Debug.Log($"Weapon area increased! New area: {weapon.area}");
    }
}
