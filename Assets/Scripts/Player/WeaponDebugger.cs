using UnityEngine;

public class WeaponDebugger : MonoBehaviour
{
    [ContextMenu("Print Active Weapons and Upgrade Levels")]
    public void PrintActiveWeapons()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("WeaponDebugger: Player not found!");
            return;
        }

        Weapon[] weapons = playerObject.GetComponentsInChildren<Weapon>(true);

        if (weapons.Length == 0)
        {
            Debug.LogWarning("WeaponDebugger: No weapons found under Player.");
            return;
        }

        Debug.Log("==== Active Weapons & Upgrade Levels ====");

        foreach (var weapon in weapons)
        {
            if (weapon != null && weapon.gameObject.activeSelf)
            {
                string weaponName = weapon.weaponData.name.Replace(" ", "");

                int level = 0;
                if (UpgradeManager.Instance != null)
                {
                    foreach (var upgrade in UpgradeManager.Instance.availableUpgrades)
                    {
                        if (upgrade != null && upgrade.upgradeName.Replace(" ", "") == weaponName)
                        {
                            level = UpgradeManager.Instance.GetCurrentUpgradeLevel(upgrade);
                            break;
                        }
                    }
                }


                Debug.Log($"Weapon: {weaponName} | Upgrade Level: {level}");
            }
        }

        Debug.Log("=========================================");
    }
}
