using UnityEngine;

[CreateAssetMenu(fileName = "NewGarlicAuraUpgrade", menuName = "Upgrades/GarlicAura")]
public class GarlicAuraUpgrade : WeaponUpgrade
{
    public float[] radiusIncrease;
    public int[] damageIncrease;
    public float[] hitDelayReduction;

    public new int maxLevel = 8; // Max level for upgrades

    public GarlicAuraUpgrade(string name, string[] descriptions, int maxLevel, float[] radiusIncrease, int[] damageIncrease, float[] hitDelayReduction)
        : base(name, descriptions, maxLevel, "GarlicAura", 0)
    {
        this.radiusIncrease = radiusIncrease;
        this.damageIncrease = damageIncrease;
        this.hitDelayReduction = hitDelayReduction;
    }

    public override void ApplyUpgrade(Weapon weapon)
    {
        if (weapon.GetWeaponType() == "GarlicAura")
        {
            GarlicAura garlicAura = weapon.GetComponent<GarlicAura>();
            int currentLevel = weapon.GetCurrentUpgradeLevel("GarlicAura");

            if (currentLevel < maxLevel)
            {
                if (currentLevel < radiusIncrease.Length)
                    garlicAura.garlicStats.area *= (1 + radiusIncrease[currentLevel] / 100f);

                if (currentLevel < damageIncrease.Length)
                    garlicAura.garlicStats.baseDMG += damageIncrease[currentLevel];

                if (currentLevel < hitDelayReduction.Length)
                    garlicAura.garlicStats.hitDelay -= hitDelayReduction[currentLevel];

                Debug.Log($"Garlic Aura upgraded at level {currentLevel + 1}!");
            }
            else
            {
                Debug.LogWarning("Garlic Aura upgrade level exceeds maximum.");
            }
        }
    }
}
