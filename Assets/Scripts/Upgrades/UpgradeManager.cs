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
    private SaveData cachedSaveData;
    private void Start()
    {
        CacheGameManager();
        cachedSaveData = SaveManager.Load(); // <-- add this
        StartCoroutine(WaitForPlayerAndInitialize());
    }


    private IEnumerator WaitForPlayerAndInitialize()
    {
        while (PlayerStats.Instance == null || PlayerExperience.Instance == null)
            yield return null;

        InitializeUpgradeLevels();

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

    public void InitializeStartingWeaponLevels()
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

            // Only care about weapons that are already active (starting weapon)
            if (!weapon.gameObject.activeSelf)
                continue;

            UpgradeSO weaponUpgrade = availableUpgrades.FirstOrDefault(u => u != null && u.upgradeName.Replace(" ", "") == weaponDataName);

            if (weaponUpgrade != null && weaponUpgrade is WeaponUpgradeSO weaponSO)
            {
                Debug.Log($"[UpgradeManager] Detected equipped starting weapon: {weapon.weaponData.name}");

                // Important: Set currentLevel to 1 (unlocked), but do NOT apply upgrade effects yet
                weaponSO.SetCurrentLevel(1);

                if (!pickedUpgrades.Any(p => p.upgrade == weaponSO))
                {
                    pickedUpgrades.Add(new PickedUpgrade
                    {
                        upgrade = weaponSO,
                        levelWhenPicked = 1
                    });
                }

                // Just refresh base stats for the weapon
                weapon.RefreshWeaponStats();
            }
        }

        // Initialize all OTHER weapon upgrades (that are NOT active at start) to locked state
        foreach (var upgrade in availableUpgrades)
        {
            if (upgrade != null && upgrade is WeaponUpgradeSO weaponUpgrade)
            {
                if (pickedUpgrades.Any(p => p.upgrade == weaponUpgrade))
                    continue;

                foreach (string tag in weaponUpgrade.compatibleWeaponTags)
                {
                    string cleanTag = tag.Replace(" ", "");
                    if (IsWeaponUnlocked(cleanTag))
                    {
                        Debug.Log($"[UpgradeManager] Detected unlocked weapon upgrade: {weaponUpgrade.upgradeName}, initializing at Level 0 (locked).");

                        weaponUpgrade.SetCurrentLevel(0); // Level 0 = locked, awaiting first pick
                        break;
                    }
                }
            }
        }
    }




    public void RefreshWeaponUpgradesOnWeapon(Weapon weapon)
    {
        if (weapon == null || weapon.weaponData == null)
            return;

        // Reset live stats back to base values
        weapon.WeaponAmountBonus = 0;
        weapon.cooldown = Mathf.Max(0.05f, weapon.BaseCooldown);
        weapon.area = weapon.BaseArea;
        weapon.damage = weapon.BaseDamage;
        weapon.speed = weapon.BaseSpeed;
        weapon.duration = weapon.BaseDuration;
        weapon.knockback = weapon.BaseKnockback;
        weapon.critChance = weapon.BaseCritChance;
        weapon.critMulti = weapon.BaseCritMulti;
        weapon.pierce = weapon.BasePierce;

        // Instead of pickedUpgrades, use all available upgrades
        foreach (var upgrade in availableUpgrades)
        {
            if (upgrade is WeaponUpgradeSO weaponUpgrade)
            {
                string weaponTag = weapon.weaponData.wepName.Replace(" ", "");
                if (weaponUpgrade.compatibleWeaponTags.Any(tag => tag.Replace(" ", "") == weaponTag))
                {
                    if (weaponUpgrade.GetCurrentLevel() > 0)
                    {
                        weaponUpgrade.ApplyUpgrade(weapon as IWeaponUpgradeable);
                    }
                }
            }
        }

        weapon.RefreshWeaponStats(); // Final refresh to include PlayerStats bonuses
    }


    public void RefreshOnlyWeaponStats(Weapon weapon)
    {
        weapon.RefreshWeaponStats(); // Only refresh PlayerStats-related things
    }


    // New method that was missing, to handle stats refresh
    private void RefreshAllWeaponsFromStats()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
            return;

        var upgradeables = playerObject.GetComponentsInChildren<IWeaponUpgradeable>(true);

        foreach (var weapon in upgradeables)
        {
            if (weapon is Weapon weaponComponent && weaponComponent.gameObject.activeSelf)
            {
                RefreshOnlyWeaponStats(weaponComponent); // <-- NOT RefreshWeaponUpgradesOnWeapon!
            }
        }
    }


    public void ShowUpgrades()
    {
        // Remove maxed upgrades before anything else
        availableUpgrades = availableUpgrades.Where(u => u != null && !u.IsMaxLevel()).ToArray();

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
        if (upgrade == null)
            return 0;

        return upgrade.GetCurrentLevel();
    }


    private void CreateUpgradeButton(UpgradeSO upgrade)
    {
        GameObject buttonObj = Instantiate(upgradeButtonPrefab, upgradeButtonContainer);
        UpgradeButton button = buttonObj.GetComponent<UpgradeButton>();

        // Always show next level the player will reach by picking this upgrade
        int displayLevel = Mathf.Clamp(upgrade.GetCurrentLevel() + 1, 1, upgrade.maxLevel);



        if (upgrade is WeaponUpgradeSO weaponUpgrade)
        {
            // ONLY show Unlock Weapon if it's Level 0 *AND* the weapon is actually locked
            if (weaponUpgrade.GetCurrentLevel() == 0 && !IsWeaponUnlocked(weaponUpgrade.compatibleWeaponTags.FirstOrDefault()?.Replace(" ", "")))
            {
                button.SetUnlockWeaponDetails(weaponUpgrade);
                return;
            }
        }

        // Otherwise, regular upgrade
        button.SetUpgradeDetails(upgrade, displayLevel);
    }
    private bool IsWeaponUnlocked(string weaponTag)
    {
        if (cachedSaveData == null)
            cachedSaveData = SaveManager.Load();

        if (weaponTag == "KingBible")
            return cachedSaveData.unlockedCharacters.Contains("B");
        if (weaponTag == "GarlicAura")
            return cachedSaveData.unlockedCharacters.Contains("C");
        if (weaponTag == "MagicWand")
            return true;

        return false;
    }



    public void ApplyUpgrade(UpgradeSO selectedUpgrade)
    {
        if (selectedUpgrade == null)
        {
            Debug.LogError("Selected upgrade is null!");
            return;
        }

        bool unlockedWeapon = false; // <--- track whether we just unlocked a weapon

        if (selectedUpgrade is WeaponUpgradeSO weaponSO)
        {
            if (!weaponSO.IsMaxLevel())
            {
                if (weaponSO.GetCurrentLevel() == 0)
                {
                    // Unlock weapon without applying any stat bonuses yet
                    EnableWeaponForUpgrade(weaponSO);
                    weaponSO.SetCurrentLevel(1); // Set to Level 1 after unlocking
                    Debug.Log($"[UpgradeManager] Unlocked Weapon: {weaponSO.upgradeName} at Level 1 (no bonus applied yet)");
                    unlockedWeapon = true; // <--- mark that it was just unlocked
                }
                else
                {
                    // Normal level-up: apply stat bonus
                    weaponSO.SetCurrentLevel(weaponSO.GetCurrentLevel() + 1);
                    weaponSO.ApplyLevelUpEffect();
                    Debug.Log($"[UpgradeManager] Applied Weapon Upgrade: {weaponSO.upgradeName} (New Level {weaponSO.GetCurrentLevel()})");
                }
            }
        }
        else if (selectedUpgrade is PassiveUpgradeSO passiveSO)
        {
            if (!passiveSO.IsMaxLevel())
            {
                passiveSO.ApplyLevelUpEffect();
                Debug.Log($"[UpgradeManager] Applied Passive Upgrade: {passiveSO.upgradeName} (New Level {passiveSO.GetCurrentLevel()})");
            }
        }
        else
        {
            Debug.LogWarning($"[UpgradeManager] Unknown upgrade type: {selectedUpgrade.upgradeName}");
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
            Debug.Log($"[UpgradeManager] {selectedUpgrade.upgradeName} reached max level and was removed from the pool.");
        }

        // ONLY refresh weapon stats if it was NOT a new unlock
        if (!unlockedWeapon && selectedUpgrade is WeaponUpgradeSO weaponUpgradeRefresh)
        {
            RefreshWeaponsMatchingUpgrade(weaponUpgradeRefresh);
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
            Debug.LogError("[UpgradeManager] PlayerExperience instance missing! Cannot continue properly.");
        }
    }

    private void RefreshWeaponsMatchingUpgrade(WeaponUpgradeSO weaponUpgrade)
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
            return;

        var weapons = playerObject.GetComponentsInChildren<Weapon>(true);

        foreach (var weapon in weapons)
        {
            if (weapon == null || weapon.weaponData == null)
                continue;

            // Skip if not active yet
            if (!weapon.gameObject.activeInHierarchy)
                continue;

            string weaponTag = weapon.weaponData.wepName.Replace(" ", "");

            if (weaponUpgrade.compatibleWeaponTags.Any(tag => tag.Replace(" ", "") == weaponTag))
            {
                RefreshWeaponUpgradesOnWeapon(weapon);
            }
        }
    }


    private void EnableWeaponForUpgrade(WeaponUpgradeSO weaponUpgrade)
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null) return;

        var weapons = playerObject.GetComponentsInChildren<Weapon>(true);

        foreach (var weapon in weapons)
        {
            if (weapon == null || weapon.weaponData == null)
                continue;

            string weaponTag = weapon.weaponData.wepName.Replace(" ", "");

            if (weaponUpgrade.compatibleWeaponTags.Any(tag => tag.Replace(" ", "") == weaponTag))
            {
                weapon.gameObject.SetActive(true);
                RefreshWeaponUpgradesOnWeapon(weapon);
                weapon.RefreshWeaponStats();
                Debug.Log($"[UpgradeManager] Enabled weapon {weapon.name} for {weaponUpgrade.upgradeName} Level 1!");
            }
        }
    }

    private void RefreshActiveWeaponsFromStats()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
            return;

        var upgradeables = playerObject.GetComponentsInChildren<IWeaponUpgradeable>(true);

        foreach (var weapon in upgradeables)
            weapon.RefreshWeaponStats();
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
                return CheckWeaponTags(passiveUpgrade.compatibleWeaponTags, requireActive: true);
            }
            else
            {
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
                    if (isActive || isUnlocked)
                        return true;
                }
                else
                {
                    if (isUnlocked)
                        return true;
                }
            }
        }

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