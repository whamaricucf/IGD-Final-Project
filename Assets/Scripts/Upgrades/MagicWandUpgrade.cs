using UnityEngine;

[CreateAssetMenu(fileName = "NewMagicWandUpgrade", menuName = "Upgrades/MagicWand")]
public class MagicWandUpgrade : WeaponUpgrade
{
    public int[] projectilesIncrease;
    public float[] cooldownReduction;
    public int[] damageIncrease;
    public int[] pierceIncrease;

    public new int maxLevel = 8; // Max level for upgrades

    public MagicWandUpgrade(string name, string[] descriptions, int maxLevel, int[] projectilesIncrease, float[] cooldownReduction, int[] damageIncrease, int[] pierceIncrease)
        : base(name, descriptions, maxLevel, "MagicWand", 0)
    {
        this.projectilesIncrease = projectilesIncrease;
        this.cooldownReduction = cooldownReduction;
        this.damageIncrease = damageIncrease;
        this.pierceIncrease = pierceIncrease;
    }

    public override void ApplyUpgrade(Weapon weapon)
    {
        if (weapon.GetWeaponType() == "MagicWand")
        {
            MagicWand magicWand = weapon.GetComponent<MagicWand>();
            int currentLevel = weapon.GetCurrentUpgradeLevel("MagicWand");

            if (currentLevel < maxLevel)
            {
                if (currentLevel < projectilesIncrease.Length)
                    magicWand.wandStats.amount += projectilesIncrease[currentLevel];

                if (currentLevel < cooldownReduction.Length)
                    magicWand.wandStats.cd -= cooldownReduction[currentLevel];

                if (currentLevel < damageIncrease.Length)
                    magicWand.wandStats.baseDMG += damageIncrease[currentLevel];

                if (currentLevel < pierceIncrease.Length)
                    magicWand.wandStats.pierce += pierceIncrease[currentLevel];

                Debug.Log($"Magic Wand upgraded at level {currentLevel + 1}!");
            }
            else
            {
                Debug.LogWarning("Magic Wand upgrade level exceeds maximum.");
            }
        }
    }
}
