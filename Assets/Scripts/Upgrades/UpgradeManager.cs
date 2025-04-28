using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("Available Upgrades")]
    public UpgradeSO[] availableUpgrades;

    [Header("UI References")]
    public GameObject upgradeButtonPrefab;
    public Transform upgradeButtonContainer;

    private GameManager gameManager;
    private List<PickedUpgrade> pickedUpgrades = new List<PickedUpgrade>();

    [System.Serializable]
    public struct PickedUpgrade
    {
        public UpgradeSO upgrade;
        public int levelWhenPicked;
    }


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        CacheGameManager();
        StartCoroutine(WaitForPlayerAndInitialize());
    }

    private IEnumerator WaitForPlayerAndInitialize()
    {
        while (PlayerStats.Instance == null || PlayerExperience.Instance == null)
            yield return null;

        InitializeUpgradeLevels();
        InitializeStartingWeaponLevels();

        // Force re-run EquipStartingWeapon() after initializing upgrades
        GameManager.Instance?.EquipStartingWeapon();

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnStatsChanged += RefreshAllWeaponsFromStats;
    }


    private void OnDestroy()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnStatsChanged -= RefreshAllWeaponsFromStats;
    }

    private void CacheGameManager()
    {
        gameManager = GameObject.FindWithTag("GameManager")?.GetComponent<GameManager>();
        if (gameManager == null)
            Debug.LogError("UpgradeManager: GameManager not found!");
    }

    private void InitializeUpgradeLevels()
    {
        foreach (var upgrade in availableUpgrades)
        {
            if (upgrade != null)
                upgrade.ResetUpgradeLevel();
        }
    }

    private void InitializeStartingWeaponLevels()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("UpgradeManager: Player not found!");
            return;
        }

        Weapon[] weapons = playerObject.GetComponentsInChildren<Weapon>(true);

        foreach (var weapon in weapons)
        {
            if (weapon == null || weapon.weaponData == null)
                continue;

            string weaponDataName = weapon.weaponData.name.Replace(" ", "");

            if (!IsWeaponUnlocked(weaponDataName))
            {
                weapon.gameObject.SetActive(false);
                continue;
            }

            if (!weapon.gameObject.activeSelf)
                continue;

            UpgradeSO weaponUpgrade = availableUpgrades.FirstOrDefault(u => u != null && u.upgradeName.Replace(" ", "") == weaponDataName);

            if (weaponUpgrade != null && weaponUpgrade is WeaponUpgradeSO weaponSO)
            {
                Debug.Log($"[UpgradeManager] Detected active weapon: {weapon.weaponData.name}, setting Level 1 cleanly.");

                weaponSO.SetCurrentLevel(1);

                // Add to picked list if not already there
                bool alreadyExists = pickedUpgrades.Any(p => p.upgrade == weaponSO);
                if (!alreadyExists)
                {
                    weaponSO.SetCurrentLevel(1); // Set weapon upgrade to Level 1
                    pickedUpgrades.Add(new PickedUpgrade
                    {
                        upgrade = weaponSO,
                        levelWhenPicked = 1 // explicitly says: 'already picked once'
                    });
                }

                weapon.RefreshWeaponStats();

            }
        }
    }


    public void RefreshWeaponUpgradesOnWeapon(Weapon weapon)
    {
        foreach (var picked in pickedUpgrades)
        {
            if (picked.upgrade is WeaponUpgradeSO weaponUpgrade)
            {
                string weaponTag = weapon.weaponData.wepName.Replace(" ", "");
                if (weaponUpgrade.compatibleWeaponTags.Any(tag => tag.Replace(" ", "") == weaponTag))
                {
                    weaponUpgrade.ApplyUpgrade(weapon as IWeaponUpgradeable);
                }
            }
        }

        weapon.RefreshWeaponStats();
    }




    public void ShowUpgrades()
    {
        foreach (Transform child in upgradeButtonContainer)
            Destroy(child.gameObject);

        List<UpgradeSO> validUpgrades = availableUpgrades
            .Where(u => u != null && !IsUpgradeMaxed(u) && IsUpgradeCompatibleWithCurrentWeapons(u))
            .ToList();

        if (validUpgrades.Count == 0)
        {
            Debug.LogWarning("[UpgradeManager] No valid upgrades available. Skipping upgrade menu.");
            if (PlayerExperience.Instance != null)
                PlayerExperience.Instance.pendingUpgradePicks = 0;

            CloseUpgradeMenu();
            ResumeGame();
            return;
        }

        List<UpgradeSO> weightedUpgrades = new List<UpgradeSO>();

        foreach (var upgrade in validUpgrades)
        {
            int effectiveLevel = GetEffectiveUpgradeLevel(upgrade);

            if (!IsUpgradeMaxed(upgrade))
            {
                for (int i = 0; i < Mathf.Max(1, Mathf.CeilToInt(upgrade.weight)); i++)
                {
                    weightedUpgrades.Add(upgrade);
                }
            }
        }

        List<UpgradeSO> selectedUpgrades = new List<UpgradeSO>();
        int upgradesToShow = Mathf.Min(3, validUpgrades.Count);

        while (selectedUpgrades.Count < upgradesToShow && weightedUpgrades.Count > 0)
        {
            int index = Random.Range(0, weightedUpgrades.Count);
            UpgradeSO candidate = weightedUpgrades[index];

            if (!selectedUpgrades.Contains(candidate))
                selectedUpgrades.Add(candidate);

            weightedUpgrades.RemoveAll(u => u == candidate);
        }

        foreach (var upgrade in selectedUpgrades)
            CreateUpgradeButton(upgrade);
    }

    private int GetEffectiveUpgradeLevel(UpgradeSO upgrade)
    {
        if (upgrade is WeaponUpgradeSO weaponUpgrade)
        {
            string weaponName = weaponUpgrade.upgradeName.Replace(" ", "");

            GameObject playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                Weapon[] weapons = playerObject.GetComponentsInChildren<Weapon>(true);
                foreach (var weapon in weapons)
                {
                    if (weapon != null && weapon.weaponData != null)
                    {
                        string weaponTag = weapon.weaponData.wepName.Replace(" ", "");
                        if (weapon.gameObject.activeSelf && weaponTag == weaponName)
                        {
                            // Weapon is already active -> treat it as if picked once already
                            return weaponUpgrade.GetCurrentLevel() + 1;
                        }
                    }
                }
            }
        }

        // Otherwise, just return the real current level
        return upgrade.GetCurrentLevel();
    }


    private void CreateUpgradeButton(UpgradeSO upgrade)
    {
        GameObject buttonObj = Instantiate(upgradeButtonPrefab, upgradeButtonContainer);
        UpgradeButton button = buttonObj.GetComponent<UpgradeButton>();
        button.SetUpgradeDetails(upgrade, GetEffectiveUpgradeLevel(upgrade));
    }

    public void ApplyUpgrade(UpgradeSO selectedUpgrade)
    {
        if (selectedUpgrade == null)
        {
            Debug.LogError("Selected upgrade is null!");
            return;
        }

        Debug.Log($"[UpgradeManager] Applying upgrade: {selectedUpgrade.upgradeName} (Level {selectedUpgrade.GetCurrentLevel()})");

        if (selectedUpgrade is PassiveUpgradeSO passive)
        {
            passive.ApplyUpgrade(PlayerStats.Instance);
            RefreshActiveWeaponsFromStats();
            PassiveUpgradesHUD.Instance?.RegisterPassiveUpgrade(passive.upgradeName, passive.icon, selectedUpgrade.GetCurrentLevel());
        }
        else if (selectedUpgrade is WeaponUpgradeSO weaponUpgrade)
        {
            ApplyWeaponUpgrade(weaponUpgrade);
        }

        if (!pickedUpgrades.Any(p => p.upgrade == selectedUpgrade))
        {
            pickedUpgrades.Add(new PickedUpgrade
            {
                upgrade = selectedUpgrade,
                levelWhenPicked = selectedUpgrade.GetCurrentLevel()
            });
        }

        if (IsUpgradeMaxed(selectedUpgrade))
        {
            availableUpgrades = availableUpgrades.Where(u => u != selectedUpgrade).ToArray();
            Debug.Log($"[UpgradeManager] {selectedUpgrade.upgradeName} reached max level and was removed.");
        }

        if (PlayerExperience.Instance != null)
        {
            PlayerExperience.Instance.pendingUpgradePicks--;
            PlayerExperience.Instance.UpgradeSelected();

            if (PlayerExperience.Instance.pendingUpgradePicks <= 0)
            {
                CloseUpgradeMenu();
                ResumeGame();
            }
            else
            {
                ShowUpgrades();
            }
        }
        else
        {
            Debug.LogError("[UpgradeManager] PlayerExperience instance missing! Can't apply upgrade properly.");
        }
    }




    private void RefreshActiveWeaponsFromStats()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
            return;

        var upgradeables = playerObject.GetComponentsInChildren<IWeaponUpgradeable>(true);

        foreach (var weapon in upgradeables)
        {
            if (weapon is Weapon weaponComponent)
            {
                if (weaponComponent.gameObject.activeSelf) // Only active weapons
                {
                    weapon.RefreshWeaponStats();
                }
            }
        }
    }


    private void ApplyWeaponUpgrade(WeaponUpgradeSO weaponUpgrade)
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
            return;

        var upgradeables = playerObject.GetComponentsInChildren<Weapon>(true);
        bool foundCompatibleWeapon = false;

        foreach (var weapon in upgradeables)
        {
            if (weapon == null || weapon.weaponData == null)
                continue;

            string weaponTag = weapon.weaponData.wepName.Replace(" ", "");

            bool compatible = weaponUpgrade.compatibleWeaponTags.Any(tag => tag.Replace(" ", "") == weaponTag);

            if (compatible && IsWeaponUnlocked(weaponTag))
            {
                foundCompatibleWeapon = true;

                if (!weapon.gameObject.activeSelf)
                {
                    if (weaponUpgrade.isWeaponSummonUpgrade)
                    {
                        Debug.Log($"[UpgradeManager] Summoning and activating new weapon: {weapon.weaponData.wepName}");
                        weapon.gameObject.SetActive(true);

                        if (weapon is IWeaponUpgradeable weaponUpgradable)
                        {
                            weaponUpgrade.ApplyUpgrade(weaponUpgradable);
                            weaponUpgradable.RefreshWeaponStats();
                            weaponUpgradable.ReinitializeWeaponAfterUpgrade();
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[UpgradeManager] {weaponUpgrade.upgradeName} applies to {weapon.weaponData.wepName}, but weapon is inactive and not summonable. Skipping.");
                    }
                }
                else
                {
                    if (weapon is IWeaponUpgradeable weaponUpgradable)
                    {
                        weaponUpgrade.ApplyUpgrade(weaponUpgradable);
                        weaponUpgradable.RefreshWeaponStats();
                        weaponUpgradable.ReinitializeWeaponAfterUpgrade();
                    }
                }
            }
        }

        if (!foundCompatibleWeapon)
        {
            Debug.LogWarning($"[UpgradeManager] No compatible weapon found for upgrade {weaponUpgrade.upgradeName}. Skipping upgrade application.");
        }
    }



    private bool AreAllUpgradesMaxed()
    {
        return availableUpgrades.All(u => u == null || IsUpgradeMaxed(u));
    }

    private bool IsUpgradeCompatibleWithCurrentWeapons(UpgradeSO upgrade)
    {
        if (upgrade == null)
            return false;

        if (upgrade is WeaponUpgradeSO weaponUpgrade)
            {
                 return CheckWeaponTags(weaponUpgrade.compatibleWeaponTags, requireActive: true);
            }
        else if (upgrade is PassiveUpgradeSO passiveUpgrade)
        {
            if (passiveUpgrade.compatibleWeaponTags != null && passiveUpgrade.compatibleWeaponTags.Count > 0)
            {
                // For passive upgrades, require that at least one compatible weapon is currently ACTIVE
                return CheckWeaponTags(passiveUpgrade.compatibleWeaponTags, requireActive: true);
            }
            else
            {
                // Passive upgrades with no weapon restrictions are always valid
                return true;
            }
        }

        return true;
    }


    private bool CheckWeaponTags(List<string> tags, bool requireActive)
    {
        if (tags == null || tags.Count == 0)
            return true;

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
            return false;

        var weaponScripts = playerObject.GetComponentsInChildren<Weapon>(true);

        foreach (var weapon in weaponScripts)
        {
            if (weapon == null || weapon.weaponData == null)
                continue;

            string weaponTag = weapon.weaponData.wepName.Replace(" ", "");

            bool isUnlocked = IsWeaponUnlocked(weaponTag);
            bool isActive = weapon.gameObject.activeSelf;

            if (tags.Any(tag => tag.Replace(" ", "") == weaponTag))
            {
                if (requireActive)
                {
                    // ONLY care about active weapons if requireActive is true
                    if (isActive)
                        return true;
                }
                else
                {
                    // Otherwise, allow unlocked weapons
                    if (isUnlocked)
                        return true;
                }
            }
        }

        return false;
    }



    private bool IsWeaponUnlocked(string weaponTag)
    {
        // Example - Customize based on your SaveManager or unlock system
        SaveData data = SaveManager.Load();

        if (weaponTag == "KingBible")
            return data.unlockedCharacters.Contains("B");
        if (weaponTag == "GarlicAura")
            return data.unlockedCharacters.Contains("C");
        // MagicWand (Character A) is unlocked by default, you could allow by default
        if (weaponTag == "MagicWand")
            return true;

        return false;
    }



    private bool IsUpgradeMaxed(UpgradeSO upgrade)
    {
        return upgrade != null && upgrade.IsMaxLevel();
    }

    public List<PickedUpgrade> GetPickedUpgrades()
    {
        return pickedUpgrades;
    }


    private void RefreshAllWeaponsFromStats()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
            return;

        var upgradeables = playerObject.GetComponentsInChildren<IWeaponUpgradeable>(true);

        foreach (var weapon in upgradeables)
            weapon.RefreshWeaponStats();
    }

    private void CloseUpgradeMenu()
    {
        FindObjectOfType<AdditiveScenes>()?.CloseUpgradeMenu();
        PlayerExperience.Instance.upgradeScreenTriggered = false;
    }

    private void ResumeGame()
    {
        gameManager?.ResumeGame();
    }

    public void ResetAllUpgrades()
    {
        Debug.Log("[UpgradeManager] Resetting all upgrades...");
        foreach (var upgrade in availableUpgrades)
        {
            if (upgrade == null) continue;
            upgrade.ResetUpgradeLevel();
        }
    }
    public int GetCurrentUpgradeLevel(UpgradeSO upgrade)
    {
        if (upgrade == null)
            return 0;

        return upgrade.GetCurrentLevel();
    }

#if UNITY_EDITOR
    [ContextMenu("Auto-Fill Available Upgrades")]
    private void AutoFillAvailableUpgrades()
    {
        string[] guids = AssetDatabase.FindAssets("t:UpgradeSO");

        availableUpgrades = new UpgradeSO[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            availableUpgrades[i] = AssetDatabase.LoadAssetAtPath<UpgradeSO>(path);
        }

        EditorUtility.SetDirty(this); // Mark this object dirty so changes are saved
        Debug.Log($"[UpgradeManager] Auto-filled {availableUpgrades.Length} upgrades.");
    }
#endif
}
