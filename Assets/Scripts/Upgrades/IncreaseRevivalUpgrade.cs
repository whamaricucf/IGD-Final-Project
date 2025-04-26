using UnityEngine;

[CreateAssetMenu(fileName = "NewIncreaseRevivalUpgrade", menuName = "Upgrades/Increase Revival")]
public class IncreaseRevivalUpgrade : PassiveUpgrade
{
    public int additionalRevival;  // Number of additional revivals per level
    public new int maxLevel = 2;

    public IncreaseRevivalUpgrade(string name, string[] descriptions, int maxLevel, int additionalRevival)
        : base(name, descriptions, maxLevel, "Revival", additionalRevival)
    {
        // Custom constructor
    }

    public override void ApplyUpgrade(PlayerStats playerStats)
    {
        playerStats.IncreaseRevivals(additionalRevival);
        playerStats.revivalLevel++; // Track the upgrade level for revival
        Debug.Log($"Revival increased! Current revivals: {playerStats.revival}");
    }
}
