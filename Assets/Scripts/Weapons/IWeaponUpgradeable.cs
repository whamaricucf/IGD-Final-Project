using System.Collections;
using UnityEngine;
using static UpgradeTypes;

public interface IWeaponUpgradeable
{
    void ApplyWeaponUpgrade(WeaponUpgradeType type, float amount, bool isPercentage);
    void RefreshWeaponStats();
    string GetWeaponIdentifier(); // Unique weapon ID for matching upgrades
    void ReinitializeWeaponAfterUpgrade();

}
