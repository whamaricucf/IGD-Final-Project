using System.Collections.Generic;
using System.Linq;
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
        InitializeUpgradeLevels();
        InitializeStartingWeaponLevels();

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
            if (weapon == null || !weapon.gameObject.activeSelf || weapon.weaponData == null)
                continue;

            string weaponDataName = weapon.weaponData.name.Replace(" ", "");
            UpgradeSO weaponUpgrade = availableUpgrades.FirstOrDefault(u => u != null && u.upgradeName.Replace(" ", "") == weaponDataName);

            if (weaponUpgrade != null && weaponUpgrade is WeaponUpgradeSO weaponSO)
            {
                Debug.Log($"[UpgradeManager] Detected active weapon: {weapon.weaponData.name}, setting upgrade level to 1.");
                weaponSO.SetCurrentLevel(1);
            }
        }
    }

    public void ShowUpgrades()
    {
        if (AreAllUpgradesMaxed())
        {
            CloseUpgradeMenu();
            ResumeGame();
            return;
        }

        foreach (Transform child in upgradeButtonContainer)
            Destroy(child.gameObject);

        List<UpgradeSO> validUpgrades = availableUpgrades
            .Where(u => u != null && !IsUpgradeMaxed(u) && IsUpgradeCompatibleWithCurrentWeapons(u))
            .ToList();

        if (validUpgrades.Count == 0)
        {
            CloseUpgradeMenu();
            ResumeGame();
            return;
        }

        List<UpgradeSO> weightedUpgrades = validUpgrades
            .SelectMany(u => Enumerable.Repeat(u, Mathf.Max(1, Mathf.CeilToInt(u.weight))))
            .ToList();

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

    private void CreateUpgradeButton(UpgradeSO upgrade)
    {
        GameObject buttonObj = Instantiate(upgradeButtonPrefab, upgradeButtonContainer);
        UpgradeButton button = buttonObj.GetComponent<UpgradeButton>();
        button.SetUpgradeDetails(upgrade, GetCurrentUpgradeLevel(upgrade));
    }

    public void ApplyUpgrade(UpgradeSO selectedUpgrade)
    {
        if (selectedUpgrade == null)
        {
            Debug.LogError("Selected upgrade is null!");
            return;
        }

        if (selectedUpgrade is PassiveUpgradeSO passive)
        {
            passive.ApplyUpgrade(PlayerStats.Instance);
            PassiveUpgradesHUD.Instance?.RegisterPassiveUpgrade(passive.upgradeName, passive.icon, GetCurrentUpgradeLevel(passive));
        }
        else if (selectedUpgrade is WeaponUpgradeSO weaponUpgrade)
        {
            ApplyWeaponUpgrade(weaponUpgrade);
        }

        if (IsUpgradeMaxed(selectedUpgrade))
        {
            availableUpgrades = availableUpgrades.Where(u => u != selectedUpgrade).ToArray();
            Debug.Log($"[UpgradeManager] {selectedUpgrade.upgradeName} reached max level and was removed.");
        }

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

    private void ApplyWeaponUpgrade(WeaponUpgradeSO weaponUpgrade)
    {
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject == null)
            return;

        var upgradeables = playerObject.GetComponentsInChildren<IWeaponUpgradeable>(true);

        foreach (var weapon in upgradeables)
        {
            if (weapon == null) continue;

            bool compatible = true;

            if (weaponUpgrade.compatibleWeaponTags != null && weaponUpgrade.compatibleWeaponTags.Count > 0)
            {
                if (weapon is Weapon weaponComponent && weaponComponent.weaponData != null)
                {
                    string weaponTag = weaponComponent.weaponData.wepName.Replace(" ", "");
                    compatible = weaponUpgrade.compatibleWeaponTags.Any(tag => tag.Replace(" ", "") == weaponTag);
                }
                else
                {
                    compatible = false;
                }
            }

            if (compatible)
            {
                weaponUpgrade.ApplyUpgrade(weapon);
                weapon.RefreshWeaponStats();
                weapon.ReinitializeWeaponAfterUpgrade(); // ✨ NEW - refresh behavior!
            }
        }
    }

    private bool AreAllUpgradesMaxed()
    {
        return availableUpgrades.All(u => u == null || IsUpgradeMaxed(u));
    }

    private bool IsUpgradeMaxed(UpgradeSO upgrade)
    {
        return upgrade != null && upgrade.IsMaxLevel();
    }

    private bool IsUpgradeCompatibleWithCurrentWeapons(UpgradeSO upgrade)
    {
        if (upgrade is not WeaponUpgradeSO weaponUpgrade)
            return true;

        if (weaponUpgrade.compatibleWeaponTags == null || weaponUpgrade.compatibleWeaponTags.Count == 0)
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

            if (weaponUpgrade.compatibleWeaponTags.Any(tag => tag.Replace(" ", "") == weaponTag))
                return true;
        }

        return false;
    }

    public int GetCurrentUpgradeLevel(UpgradeSO upgrade)
    {
        if (upgrade == null)
            return 0;

        return upgrade.GetCurrentLevel();
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
}
