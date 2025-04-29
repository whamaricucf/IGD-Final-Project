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
    private SaveData cachedSaveData;

    private List<UpgradeSO> activeUpgrades = new List<UpgradeSO>();

    private int queuedUpgradeMenus = 0;
    private bool upgradeMenuOpen = false;


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
        cachedSaveData = SaveManager.Load();
        StartCoroutine(WaitForPlayerAndInitialize());
    }

    private IEnumerator WaitForPlayerAndInitialize()
    {
        while (PlayerStats.Instance == null || PlayerExperience.Instance == null)
            yield return null;

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnStatsChanged += RefreshAllWeaponsFromStats;

        // Initialize starting weapons after player is ready
        InitializeStartingWeaponLevels();
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

    public void FullResetAllUpgrades()
    {
        foreach (var upgrade in availableUpgrades)
        {
            if (upgrade != null)
                upgrade.ResetUpgradeLevel();
        }
        pickedUpgrades.Clear();
    }

    public void InitializeStartingWeaponLevels()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("UpgradeManager: Player not found!");
            return;
        }

        // Get player weapons ONCE at top
        var playerWeapons = playerObject.GetComponentsInChildren<Weapon>(true);

        HashSet<WeaponUpgradeSO> startingWeapons = new HashSet<WeaponUpgradeSO>();

        foreach (var upgrade in availableUpgrades)
        {
            if (upgrade is WeaponUpgradeSO weaponSO)
            {
                bool matchedWeaponOnPlayer = false;

                foreach (var weapon in playerWeapons)
                {
                    if (weapon == null || weapon.weaponData == null)
                        continue;

                    string weaponTag = weapon.weaponData.wepName.Replace(" ", "");

                    if (weaponSO.compatibleWeaponTags.Any(tag => tag.Replace(" ", "") == weaponTag))
                    {
                        matchedWeaponOnPlayer = true;

                        if (IsStartingWeapon(weaponSO))
                        {
                            weaponSO.SetCurrentLevel(1);
                            weaponSO.ApplyLevelUpEffect();
                            UpdatePickedUpgrade(weaponSO);

                            if (!activeUpgrades.Contains(weaponSO))
                                activeUpgrades.Add(weaponSO);

                            weapon.gameObject.SetActive(true);
                            weapon.InitializeWeaponDataIfNeeded();
                            RefreshWeaponUpgradesOnWeapon(weapon);
                        }
                        else
                        {
                            weaponSO.SetCurrentLevel(0);
                            weapon.gameObject.SetActive(false);
                        }

                        break; // ✅ Important, exit after finding matching weapon
                    }
                }

                if (!matchedWeaponOnPlayer)
                {
                    weaponSO.SetCurrentLevel(0);
                }
            }
        }
    }


    private bool IsStartingWeapon(WeaponUpgradeSO weaponUpgrade)
    {
        if (gameManager == null)
            return false;

        string upgradeName = weaponUpgrade.upgradeName.Replace(" ", "");

        switch (gameManager.selectedCharacter)
        {
            case GameManager.CharacterType.A:
                return upgradeName == "MagicWand";
            case GameManager.CharacterType.B:
                return upgradeName == "KingBible";
            case GameManager.CharacterType.C:
                return upgradeName == "GarlicAura";
            default:
                return false;
        }
    }

    public void ApplyUpgrade(UpgradeSO selectedUpgrade)
    {
        if (selectedUpgrade == null)
        {
            Debug.LogError("Selected upgrade is null!");
            return;
        }

        if (selectedUpgrade is WeaponUpgradeSO weaponSO)
        {
            if (!weaponSO.IsMaxLevel())
            {
                bool wasLocked = (weaponSO.GetCurrentLevel() == 0);

                weaponSO.SetCurrentLevel(weaponSO.GetCurrentLevel() + 1); // INCREMENT FIRST!

                if (!activeUpgrades.Contains(weaponSO))
                    activeUpgrades.Add(weaponSO);

                UpdatePickedUpgrade(weaponSO); // Track immediately

                weaponSO.ApplyLevelUpEffect(); // Apply logic based on new level

                EnableWeaponForUpgrade(weaponSO); // Make sure weapon is enabled

                RefreshWeaponsMatchingUpgrade(weaponSO);
                RefreshAllWeaponsFromStats();

                if (wasLocked)
                    Debug.Log($"Unlocked Weapon: {weaponSO.upgradeName} at Level 1");
                else
                    Debug.Log($"Applied Weapon Upgrade: {weaponSO.upgradeName} (New Level {weaponSO.GetCurrentLevel()})");
            }
        }
        else if (selectedUpgrade is PassiveUpgradeSO passiveSO)
        {
            if (!passiveSO.IsMaxLevel())
            {
                passiveSO.ApplyLevelUpEffect();
                UpdatePickedUpgrade(passiveSO);
                Debug.Log($"Applied Passive Upgrade: {passiveSO.upgradeName} (New Level {passiveSO.GetCurrentLevel()})");
                RefreshAllWeaponsFromStats();
            }
        }

        if (IsUpgradeMaxed(selectedUpgrade))
            availableUpgrades = availableUpgrades.Where(u => u != selectedUpgrade).ToArray();

        if (PlayerExperience.Instance != null)
        {
            PlayerExperience.Instance.pendingUpgradePicks--;
            PlayerExperience.Instance.UpgradeSelected();

            if (PlayerExperience.Instance.pendingUpgradePicks > 0)
            {
                queuedUpgradeMenus++; // only queue if still pending upgrades
            }
            else
            {
                CloseUpgradeMenu(); // no more upgrades, close properly!
                ResumeGame();
                return;
            }

            TryOpenNextUpgradeMenu();
        }

    }


    private void TryOpenNextUpgradeMenu()
    {
        if (upgradeMenuOpen || queuedUpgradeMenus <= 0)
            return;

        upgradeMenuOpen = true;
        queuedUpgradeMenus--;

        ShowUpgrades();
    }

    private GameObject FindInactiveObjectWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true)) // true = include inactive
        {
            if (child.CompareTag(tag))
                return child.gameObject;
        }
        return null;
    }

    private void EnableWeaponForUpgrade(WeaponUpgradeSO weaponUpgrade)
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null) return;

        var weapons = playerObject.GetComponentsInChildren<Weapon>(true);
        bool foundMatch = false;

        foreach (var weapon in weapons)
        {
            if (weapon == null || weapon.weaponData == null)
                continue;

            string weaponTag = weapon.weaponData.wepName.Replace(" ", "");

            if (weaponUpgrade.compatibleWeaponTags.Any(tag => tag.Replace(" ", "") == weaponTag))
            {
                weapon.gameObject.SetActive(true);
                weapon.InitializeWeaponDataIfNeeded();
                weapon.ReinitializeWeaponAfterUpgrade();
                foundMatch = true;
            }
        }

        // 🧄 SPECIAL CASE: GarlicAura might not be found if it's fully inactive
        if (!foundMatch && weaponUpgrade.compatibleWeaponTags.Any(tag => tag.Replace(" ", "") == "GarlicAura"))
        {
            GameObject garlicAuraObj = FindInactiveObjectWithTag(playerObject.transform, "GarlicAura");
            if (garlicAuraObj != null)
            {
                var garlicWeapon = garlicAuraObj.GetComponent<GarlicAura>();
                if (garlicWeapon != null)
                {
                    if (garlicWeapon.weaponData == null)
                    {
                        garlicWeapon.InitializeWeaponDataIfNeeded();
                        Debug.Log("🧄 GarlicAura weaponData initialized manually!");
                    }

                    garlicAuraObj.SetActive(true);
                    garlicWeapon.ReinitializeWeaponAfterUpgrade();
                    Debug.Log("🧄 Force-enabled GarlicAura via manual search and reinit!");
                }
                else
                {
                    Debug.LogWarning("🧄 GarlicAura object found but missing GarlicAura script?");
                }
            }
            else
            {
                Debug.LogWarning("🧄 Tried to force-enable GarlicAura, but no object found with tag 'GarlicAura'!");
            }
        }
    }

    public void RefreshWeaponUpgradesOnWeapon(Weapon weapon)
    {
        if (weapon == null || weapon.weaponData == null)
            return;

        weapon.ResetToBaseStats();

        // Apply all active WEAPON upgrades
        foreach (var upgrade in activeUpgrades)
        {
            if (upgrade is WeaponUpgradeSO weaponUpgrade)
            {
                string weaponTag = weapon.weaponData.wepName.Replace(" ", "");
                if (weaponUpgrade.compatibleWeaponTags.Any(tag => tag.Replace(" ", "") == weaponTag))
                {
                    if (weaponUpgrade.GetCurrentLevel() > 0)
                        weaponUpgrade.ApplyUpgrade(weapon as IWeaponUpgradeable);
                }
            }
        }

        // Apply ALL picked PASSIVE upgrades too
        foreach (var picked in pickedUpgrades)
        {
            if (picked.upgrade is PassiveUpgradeSO passiveUpgrade)
            {
                passiveUpgrade.ApplyUpgrade(weapon as IWeaponUpgradeable);
            }
        }

        weapon.RefreshWeaponStats();
    }


    private void RefreshAllWeaponsFromStats()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
            return;

        var weapons = playerObject.GetComponentsInChildren<IWeaponUpgradeable>(true);

        foreach (var weapon in weapons)
        {
            if (weapon is Weapon weaponComponent && weaponComponent.gameObject.activeSelf)
            {
                RefreshWeaponUpgradesOnWeapon(weaponComponent);
            }
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
            if (weapon == null || weapon.weaponData == null || !weapon.gameObject.activeInHierarchy)
                continue;

            string weaponTag = weapon.weaponData.wepName.Replace(" ", "");
            if (weaponUpgrade.compatibleWeaponTags.Any(tag => tag.Replace(" ", "") == weaponTag))
            {
                RefreshWeaponUpgradesOnWeapon(weapon);
            }
        }
    }

    public void ShowUpgrades()
    {
        availableUpgrades = availableUpgrades.Where(u => u != null && !u.IsMaxLevel()).ToArray();

        foreach (Transform child in upgradeButtonContainer)
            Destroy(child.gameObject);

        List<UpgradeSO> validUpgrades = availableUpgrades.Where(u => u != null && IsUpgradeCompatibleWithCurrentWeapons(u)).ToList();

        if (validUpgrades.Count == 0)
        {
            if (PlayerExperience.Instance != null)
                PlayerExperience.Instance.pendingUpgradePicks = 0;
            CloseUpgradeMenu();
            ResumeGame();
            return;
        }

        List<UpgradeSO> weightedUpgrades = new List<UpgradeSO>();
        foreach (var upgrade in validUpgrades)
        {
            for (int i = 0; i < Mathf.Max(1, Mathf.CeilToInt(upgrade.weight)); i++)
                weightedUpgrades.Add(upgrade);
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
        {
            CreateUpgradeButton(upgrade);
        }
    }

    private void SyncAvailableUpgradeLevels()
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
            return;

        var playerWeapons = playerObject.GetComponentsInChildren<Weapon>(true);

        foreach (var upgrade in availableUpgrades)
        {
            if (upgrade is WeaponUpgradeSO weaponUpgrade)
            {
                foreach (var weapon in playerWeapons)
                {
                    if (weapon == null || weapon.weaponData == null)
                        continue;

                    string weaponTag = weapon.weaponData.wepName.Replace(" ", "");

                    if (weaponUpgrade.compatibleWeaponTags.Any(tag => tag.Replace(" ", "") == weaponTag))
                    {
                        if (weapon.gameObject.activeSelf)
                        {
                            // If active and level 0, assume unlock at Level 1
                            if (weaponUpgrade.GetCurrentLevel() == 0)
                            {
                                weaponUpgrade.SetCurrentLevel(1);
                            }
                        }
                    }
                }
            }
        }
    }



    private void CreateUpgradeButton(UpgradeSO upgrade)
    {
        GameObject buttonObj = Instantiate(upgradeButtonPrefab, upgradeButtonContainer);
        UpgradeButton button = buttonObj.GetComponent<UpgradeButton>();

        int displayLevel = 1;

        if (upgrade is WeaponUpgradeSO weaponUpgrade)
        {
            int currentLevel = weaponUpgrade.GetCurrentLevel();
            bool isStartingWeapon = IsStartingWeapon(weaponUpgrade);

            if (currentLevel == 0)
            {
                displayLevel = 0; // Locked weapon, show Unlock (LV1)
            }
            else
            {
                if (isStartingWeapon)
                {
                    // Starting weapon: show currentLevel + 1
                    displayLevel = currentLevel;
                }
                else
                {
                    // Non-starting weapon:
                    // If Level 1 → this is first upgrade, so displayLevel = 2
                    // If Level 2+ → displayLevel = currentLevel + 1
                    displayLevel = currentLevel;
                }
            }
        }
        else if (upgrade is PassiveUpgradeSO passiveUpgrade)
        {
            int currentLevel = passiveUpgrade.GetCurrentLevel();
            if (currentLevel == 0)
                displayLevel = 1;
            else
                displayLevel = currentLevel + 1;
        }

        if (upgrade.IsMaxLevel())
            displayLevel = upgrade.GetMaxLevel();

        button.SetUpgradeDetails(upgrade, displayLevel);
    }



    private bool IsUpgradeCompatibleWithCurrentWeapons(UpgradeSO upgrade)
    {
        return true;
    }

    private bool IsUpgradeMaxed(UpgradeSO upgrade)
    {
        return upgrade != null && upgrade.IsMaxLevel();
    }

    private void CloseUpgradeMenu()
    {
        FindObjectOfType<AdditiveScenes>()?.CloseUpgradeMenu();
        if (PlayerExperience.Instance != null)
            PlayerExperience.Instance.upgradeScreenTriggered = false;

        upgradeMenuOpen = false; // mark menu as closed

        // Try to open the next upgrade menu
        TryOpenNextUpgradeMenu();
    }


    private void ResumeGame()
    {
        gameManager?.ResumeGame();
    }

    private void UpdatePickedUpgrade(UpgradeSO upgrade)
    {
        pickedUpgrades.RemoveAll(p => p.upgrade == upgrade);
        pickedUpgrades.Add(new PickedUpgrade
        {
            upgrade = upgrade,
            levelWhenPicked = upgrade.GetCurrentLevel()
        });
    }

    public List<PickedUpgrade> GetPickedUpgrades()
    {
        return pickedUpgrades;
    }

#if UNITY_EDITOR
    [ContextMenu("Auto-Fill Available Upgrades")]
    private void AutoFillAvailableUpgrades()
    {
        string[] guids = AssetDatabase.FindAssets("t:UpgradeSO");
        availableUpgrades = guids.Select(guid => AssetDatabase.LoadAssetAtPath<UpgradeSO>(AssetDatabase.GUIDToAssetPath(guid))).ToArray();
        EditorUtility.SetDirty(this);
    }
#endif
}
