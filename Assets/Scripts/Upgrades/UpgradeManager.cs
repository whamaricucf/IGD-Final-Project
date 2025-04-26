using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    public Upgrade[] availableUpgrades; // Array of upgrades to choose from
    public GameObject upgradeButtonPrefab;
    public Transform upgradeButtonContainer;

    private Weapon playerWeapon;
    private int upgradesSelected = 0;  // Counter for the selected upgrades
    private int upgradesRequired = 0;  // How many upgrades are required based on current level

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);  // Destroy any duplicate instances
        }
    }

    void Start()
    {
        InitializePlayerWeapon();
        ShowUpgrades();  // Directly invoke ShowUpgrades() instead of calling SetupUpgradeUI()
    }

    private void InitializePlayerWeapon()
    {
        // Find the player object
        GameObject playerObject = GameObject.FindWithTag("Player");

        if (playerObject != null)
        {
            // Find the weapon child (e.g., MagicWand, KingBible, or GarlicAura)
            GameObject activeWeaponObject = playerObject.transform.Find("MagicWand").gameObject; // Example for MagicWand

            if (activeWeaponObject != null)
            {
                playerWeapon = activeWeaponObject.GetComponent<Weapon>(); // Get the Weapon component from the active weapon
            }
            else
            {
                Debug.LogError("Weapon child not found.");
                return;  // Exit if no weapon is found
            }
        }
        else
        {
            Debug.LogError("Player object not found or is not tagged with 'Player'.");
            return;  // Exit if the player object is not found
        }
    }

    public void ShowUpgrades()
    {
        // Ensure upgrades are not shown if they've all been maxed
        if (AreAllUpgradesMaxed()) return;

        // Clear existing UI buttons before creating new ones
        foreach (Transform child in upgradeButtonContainer)
        {
            Destroy(child.gameObject);  // Destroy all existing buttons
        }

        upgradesRequired = Mathf.Max(1, PlayerExperience.Instance.currentLevel - 1);

        // Create a list that contains all upgrades weighted by their weight
        List<Upgrade> weightedUpgrades = new List<Upgrade>();

        // Populate the weighted list by adding upgrades based on their weight
        foreach (var upgrade in availableUpgrades)
        {
            int weight = Mathf.CeilToInt(upgrade.weight); // Get the weight and round it to an integer
            for (int i = 0; i < weight; i++)
            {
                weightedUpgrades.Add(upgrade);  // Add the upgrade to the list based on its weight
            }
        }

        // Ensure we don't select more than 3 unique upgrades
        List<Upgrade> selectedUpgrades = new List<Upgrade>();
        HashSet<Upgrade> selectedUpgradeSet = new HashSet<Upgrade>();  // To ensure uniqueness

        // Randomly select 3 unique upgrades from the weighted list
        while (selectedUpgrades.Count < 3 && weightedUpgrades.Count > 0)
        {
            int randomIndex = Random.Range(0, weightedUpgrades.Count);
            var selectedUpgrade = weightedUpgrades[randomIndex];

            if (!selectedUpgradeSet.Contains(selectedUpgrade))
            {
                selectedUpgrades.Add(selectedUpgrade);
                selectedUpgradeSet.Add(selectedUpgrade);
            }

            weightedUpgrades.RemoveAt(randomIndex);  // Remove the selected upgrade to avoid selecting it again
        }

        // Create buttons for each selected upgrade
        foreach (var upgrade in selectedUpgrades)
        {
            if (ShouldUpgradeBeAvailable(upgrade))
            {
                GameObject upgradeButton = Instantiate(upgradeButtonPrefab, upgradeButtonContainer);
                UpgradeButton buttonScript = upgradeButton.GetComponent<UpgradeButton>();

                string weaponType = playerWeapon?.GetWeaponType();  // Safe null check for playerWeapon
                int currentLevel = playerWeapon?.GetCurrentUpgradeLevel(weaponType) ?? 0;  // Safe null check

                buttonScript.SetUpgradeDetails(upgrade, currentLevel);

                buttonScript.GetComponent<Button>().onClick.AddListener(() => ApplyUpgrade(upgrade));
            }
        }
    }




    private bool AreAllUpgradesMaxed()
    {
        if (playerWeapon == null || PlayerStats.Instance == null)
        {
            Debug.LogError("Player weapon or PlayerStats is null.");
            return true;
        }

        return IsWeaponUpgradesMaxed() && IsPassiveUpgradesMaxed();
    }

    private bool IsWeaponUpgradesMaxed()
    {
        if (playerWeapon == null)
        {
            Debug.LogError("Player weapon is not initialized.");
            return false;
        }

        return playerWeapon.GetCurrentUpgradeLevel("Damage") >= 8 &&
               playerWeapon.GetCurrentUpgradeLevel("Speed") >= 8;
    }

    private bool IsPassiveUpgradesMaxed()
    {
        return PlayerStats.Instance.healthLevel >= 5 && PlayerStats.Instance.speedLevel >= 5;
    }

    public void ApplyUpgrade(Upgrade selectedUpgrade)
    {
        if (selectedUpgrade is WeaponUpgrade weaponUpgrade)
        {
            playerWeapon?.ApplyUpgrade(weaponUpgrade);  // Apply the weapon upgrade
            Debug.Log($"Weapon upgrade applied: {weaponUpgrade.upgradeName}");
        }
        else if (selectedUpgrade is PassiveUpgrade passiveUpgrade)
        {
            ApplyPassiveUpgrade(passiveUpgrade);
            Debug.Log($"Passive upgrade applied: {passiveUpgrade.upgradeName}");
        }

        upgradesSelected++;

        // Check if the required number of upgrades has been selected
        if (upgradesSelected >= upgradesRequired)
        {
            CloseUpgradeMenu(); // Close the upgrade menu when all required upgrades are selected.
            ResumeGame();  // Call GameManager's ResumeGame to resume the game after upgrade
        }
    }

    public void ResumeGame()
    {
        // Ensure that the GameManager correctly resumes the game
        GameManager gameManager = GameObject.FindWithTag("GameManager")?.GetComponent<GameManager>();

        if (gameManager != null)
        {
            gameManager.ResumeGame();  // Resume GameManager's logic without resetting the timer
            Debug.Log("Game resumed after upgrade.");
        }
        else
        {
            Debug.LogError("GameManager not found in the scene.");
        }
    }




    private void ApplyPassiveUpgrade(PassiveUpgrade upgrade)
    {
        if (PlayerStats.Instance == null)
        {
            Debug.LogError("PlayerStats.Instance is null.");
            return;
        }

        switch (upgrade.upgradeType)
        {
            case "Health":
                PlayerStats.Instance.IncreaseHealth();
                break;
            case "Speed":
                PlayerStats.Instance.IncreaseSpeed();
                break;
            case "Damage":
                PlayerStats.Instance.IncreaseDamage();
                break;
            case "Regen":
                PlayerStats.Instance.IncreaseRegen();
                break;
            case "Magnet":
                PlayerStats.Instance.IncreaseMagnet(1);
                break;
            case "Luck":
                PlayerStats.Instance.IncreaseLuck(1);
                break;
            case "Armor":
                PlayerStats.Instance.IncreaseArmor(1);
                break;
            case "Growth":
                PlayerStats.Instance.ApplyExperienceMultiplier(1);
                break;
            case "Revival":
                PlayerStats.Instance.IncreaseRevival();
                break;
        }
    }

    private bool ShouldUpgradeBeAvailable(Upgrade upgrade)
    {
        string weaponType = playerWeapon?.GetWeaponType();  // Safe null check

        if (upgrade is WeaponUpgrade)
        {
            return true;  // Weapon upgrades are always available if unlocked
        }

        // Passive upgrades are available based on player progression
        return upgrade is PassiveUpgrade;
    }

    private void CloseUpgradeMenu()
    {
        AdditiveScenes additiveScenes = FindObjectOfType<AdditiveScenes>();
        if (additiveScenes != null)
        {
            additiveScenes.CloseUpgradeMenu();
        }
    }
}
