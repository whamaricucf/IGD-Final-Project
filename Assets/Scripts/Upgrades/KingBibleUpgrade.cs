using UnityEngine;

[CreateAssetMenu(fileName = "NewKingBibleUpgrade", menuName = "Upgrades/KingBible")]
public class KingBibleUpgrade : WeaponUpgrade
{
    public int[] bibleAmountIncrease;
    public int[] damageIncrease;
    public float[] areaIncrease;
    public float[] durationIncrease;
    public float[] speedIncrease;

    public new int maxLevel = 8; // Max level for upgrades

    public KingBibleUpgrade(string name, string[] descriptions, int maxLevel, int[] bibleAmountIncrease, int[] damageIncrease, float[] areaIncrease, float[] durationIncrease, float[] speedIncrease)
        : base(name, descriptions, maxLevel, "KingBible", 0)
    {
        this.bibleAmountIncrease = bibleAmountIncrease;
        this.damageIncrease = damageIncrease;
        this.areaIncrease = areaIncrease;
        this.durationIncrease = durationIncrease;
        this.speedIncrease = speedIncrease;
    }

    public override void ApplyUpgrade(Weapon weapon)
    {
        if (weapon.GetWeaponType() == "KingBible")
        {
            KingBible kingBible = weapon.GetComponent<KingBible>();
            int currentLevel = weapon.GetCurrentUpgradeLevel("KingBible");

            if (currentLevel < maxLevel)
            {
                if (currentLevel < bibleAmountIncrease.Length)
                    kingBible.bibleStats.amount += bibleAmountIncrease[currentLevel];

                if (currentLevel < damageIncrease.Length)
                    kingBible.bibleStats.baseDMG += damageIncrease[currentLevel];

                if (currentLevel < areaIncrease.Length)
                    kingBible.bibleStats.area *= (1 + areaIncrease[currentLevel] / 100f);

                if (currentLevel < durationIncrease.Length)
                    kingBible.bibleStats.duration += 1 + durationIncrease[currentLevel];

                if (currentLevel < speedIncrease.Length)
                    kingBible.bibleStats.spd *= (1 + speedIncrease[currentLevel] / 100f);

                Debug.Log($"King Bible upgraded at level {currentLevel + 1}!");
            }
            else
            {
                Debug.LogWarning("King Bible upgrade level exceeds maximum.");
            }
        }
    }
}
