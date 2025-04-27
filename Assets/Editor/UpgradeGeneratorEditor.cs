using UnityEngine;
using UnityEditor;
using System.IO;
using static UpgradeTypes;
using System.Linq;

public class UpgradeGeneratorEditor : EditorWindow
{
    [MenuItem("Tools/Generate Upgrades")]
    public static void GenerateUpgrades()
    {
        string upgradePath = "Assets/Scripts/Upgrades/Generated/";

        if (!Directory.Exists(upgradePath))
            Directory.CreateDirectory(upgradePath);

        GeneratePassiveUpgrades(upgradePath);
        GenerateWeaponStatUpgrades(upgradePath);
        GenerateWeaponUpgrades(upgradePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Upgrades generated successfully!");
    }

    private static void GeneratePassiveUpgrades(string path)
    {
        CreatePassiveUpgrade(path, "Damage", PassiveUpgradeType.Damage, 5, 100, 10, true);
        CreatePassiveUpgrade(path, "Armor", PassiveUpgradeType.Armor, 5, 100, 1, false);
        CreatePassiveUpgrade(path, "MaxHealth", PassiveUpgradeType.Health, 5, 90, 20, true);
        CreatePassiveUpgrade(path, "Regen", PassiveUpgradeType.Regen, 5, 90, 0.2f, false);
        CreatePassiveUpgrade(path, "MoveSpeed", PassiveUpgradeType.MoveSpeed, 5, 50, 10, true);
        CreatePassiveUpgrade(path, "Magnet", PassiveUpgradeType.Magnet, 5, 100, 10, true);
        CreatePassiveUpgrade(path, "Luck", PassiveUpgradeType.Luck, 5, 100, 10, true);
        CreatePassiveUpgrade(path, "Growth", PassiveUpgradeType.Growth, 5, 80, 8, true);
        CreatePassiveUpgrade(path, "Greed", PassiveUpgradeType.Greed, 5, 80, 10, true);
        CreatePassiveUpgrade(path, "Revival", PassiveUpgradeType.Revival, 2, 40, 1, false);
        CreatePassiveUpgrade(path, "Amount", PassiveUpgradeType.Amount, 5, 80, 1, false);
    }


    private static void GenerateWeaponStatUpgrades(string path)
    {
        CreateWeaponStatUpgrade(path, "Cooldown", WeaponUpgradeType.Cooldown, 5, 50, 8, true);
        CreateWeaponStatUpgrade(path, "Area", WeaponUpgradeType.Area, 5, 100, 10, true);
        CreateWeaponStatUpgrade(path, "ProjectileSpeed", WeaponUpgradeType.Speed, 5, 100, 10, true);
        CreateWeaponStatUpgrade(path, "Duration", WeaponUpgradeType.Duration, 5, 100, 10, true);
        // No general "Amount" upgrade here!
    }

    private static void CreatePassiveUpgrade(string path, string name, PassiveUpgradeType type, int maxLevel, float weight, float amount, bool isPercentage)
    {
        PassiveUpgradeSO asset = ScriptableObject.CreateInstance<PassiveUpgradeSO>();
        asset.upgradeName = name;
        asset.weight = weight;
        asset.maxLevel = maxLevel;
        asset.upgradeLevels = new PassiveUpgradeSO.PassiveUpgradeLevelData[maxLevel];

        for (int i = 0; i < maxLevel; i++)
        {
            asset.upgradeLevels[i] = new PassiveUpgradeSO.PassiveUpgradeLevelData
            {
                upgradeType = type,
                upgradeAmount = amount,
                isPercentageBased = isPercentage,
                description = GetPassiveUpgradeDescription(type, amount, isPercentage)
            };
        }

        AssetDatabase.CreateAsset(asset, path + name.Replace(" ", "") + ".asset");
    }

    private static void CreateWeaponStatUpgrade(string path, string name, WeaponUpgradeType type, int maxLevel, float weight, float amount, bool isPercentage)
    {
        WeaponUpgradeSO asset = ScriptableObject.CreateInstance<WeaponUpgradeSO>();
        asset.upgradeName = name;
        asset.weight = weight;
        asset.maxLevel = maxLevel;
        asset.upgradeLevels = new WeaponUpgradeSO.WeaponUpgradeLevelData[maxLevel];

        for (int i = 0; i < maxLevel; i++)
        {
            asset.upgradeLevels[i] = new WeaponUpgradeSO.WeaponUpgradeLevelData
            {
                upgradeTypes = new[] { type },
                upgradeAmounts = new[] { amount },
                isPercentageBased = new[] { isPercentage },
                description = GetWeaponUpgradeDescription(type, amount, isPercentage)
            };
        }

        asset.compatibleWeaponTags = GetCompatibleWeaponTagsForStat(name).ToList();

        AssetDatabase.CreateAsset(asset, path + name.Replace(" ", "") + ".asset");
    }

    private static void GenerateWeaponUpgrades(string path)
    {
        CreateWeaponUpgradeMagicWand(path);
        CreateWeaponUpgradeKingBible(path);
        CreateWeaponUpgradeGarlic(path);
    }

    private static void CreateWeaponUpgradeMagicWand(string path)
    {
        WeaponUpgradeSO asset = ScriptableObject.CreateInstance<WeaponUpgradeSO>();
        asset.upgradeName = "Magic Wand";
        asset.maxLevel = 8;
        asset.weight = 100;

        asset.upgradeLevels = new WeaponUpgradeSO.WeaponUpgradeLevelData[8];

        asset.upgradeLevels[0] = CreateWeaponLevel(new[] { WeaponUpgradeType.None }, new float[] { 0 }, new bool[] { false }, "Fires at nearest enemy.");
        asset.upgradeLevels[1] = CreateWeaponLevel(new[] { WeaponUpgradeType.Amount }, new float[] { 1 }, new bool[] { false }, "Fires 1 more projectile.");
        asset.upgradeLevels[2] = CreateWeaponLevel(new[] { WeaponUpgradeType.Cooldown }, new float[] { 20 }, new bool[] { true }, "Cooldown reduced by 20%.");
        asset.upgradeLevels[3] = CreateWeaponLevel(new[] { WeaponUpgradeType.Amount }, new float[] { 1 }, new bool[] { false }, "Fires 1 more projectile.");
        asset.upgradeLevels[4] = CreateWeaponLevel(new[] { WeaponUpgradeType.Damage }, new float[] { 10 }, new bool[] { false }, "Base damage up by 10.");
        asset.upgradeLevels[5] = CreateWeaponLevel(new[] { WeaponUpgradeType.Amount }, new float[] { 1 }, new bool[] { false }, "Fires 1 more projectile.");
        asset.upgradeLevels[6] = CreateWeaponLevel(new[] { WeaponUpgradeType.Pierce }, new float[] { 1 }, new bool[] { false }, "Pierces 1 more enemy.");
        asset.upgradeLevels[7] = CreateWeaponLevel(new[] { WeaponUpgradeType.Damage }, new float[] { 10 }, new bool[] { false }, "Base damage up by 10.");

        AssetDatabase.CreateAsset(asset, path + "MagicWand.asset");
    }

    private static void CreateWeaponUpgradeKingBible(string path)
    {
        WeaponUpgradeSO asset = ScriptableObject.CreateInstance<WeaponUpgradeSO>();
        asset.upgradeName = "King Bible";
        asset.maxLevel = 8;
        asset.weight = 80;

        asset.upgradeLevels = new WeaponUpgradeSO.WeaponUpgradeLevelData[8];

        asset.upgradeLevels[0] = CreateWeaponLevel(new[] { WeaponUpgradeType.None }, new float[] { 0 }, new bool[] { false }, "Orbits around the character.");
        asset.upgradeLevels[1] = CreateWeaponLevel(new[] { WeaponUpgradeType.Amount }, new float[] { 1 }, new bool[] { false }, "Fires 1 more projectile.");
        asset.upgradeLevels[2] = CreateWeaponLevel(new[] { WeaponUpgradeType.Speed, WeaponUpgradeType.Area }, new float[] { 30, 25 }, new bool[] { true, true }, "Speed +30%, Area +25%.");
        asset.upgradeLevels[3] = CreateWeaponLevel(new[] { WeaponUpgradeType.Duration, WeaponUpgradeType.Damage }, new float[] { 0.5f, 10 }, new bool[] { false, false }, "Duration +0.5s, Damage +10.");
        asset.upgradeLevels[4] = CreateWeaponLevel(new[] { WeaponUpgradeType.Amount }, new float[] { 1 }, new bool[] { false }, "Fires 1 more projectile.");
        asset.upgradeLevels[5] = CreateWeaponLevel(new[] { WeaponUpgradeType.Speed, WeaponUpgradeType.Area }, new float[] { 30, 25 }, new bool[] { true, true }, "Speed +30%, Area +25%.");
        asset.upgradeLevels[6] = CreateWeaponLevel(new[] { WeaponUpgradeType.Duration, WeaponUpgradeType.Damage }, new float[] { 0.5f, 10 }, new bool[] { false, false }, "Duration +0.5s, Damage +10.");
        asset.upgradeLevels[7] = CreateWeaponLevel(new[] { WeaponUpgradeType.Amount }, new float[] { 1 }, new bool[] { false }, "Fires 1 more projectile.");

        AssetDatabase.CreateAsset(asset, path + "KingBible.asset");
    }

    private static void CreateWeaponUpgradeGarlic(string path)
    {
        WeaponUpgradeSO asset = ScriptableObject.CreateInstance<WeaponUpgradeSO>();
        asset.upgradeName = "Garlic";
        asset.maxLevel = 8;
        asset.weight = 90;

        asset.upgradeLevels = new WeaponUpgradeSO.WeaponUpgradeLevelData[8];

        asset.upgradeLevels[0] = CreateWeaponLevel(new[] { WeaponUpgradeType.None }, new float[] { 0 }, new bool[] { false }, "Damages nearby enemies.");
        asset.upgradeLevels[1] = CreateWeaponLevel(new[] { WeaponUpgradeType.Area, WeaponUpgradeType.Damage }, new float[] { 40, 2 }, new bool[] { true, false }, "Area +40%, Damage +2.");
        asset.upgradeLevels[2] = CreateWeaponLevel(new[] { WeaponUpgradeType.Cooldown, WeaponUpgradeType.Damage }, new float[] { 10, 1 }, new bool[] { true, false }, "Cooldown reduced by 10%, Damage +1.");
        asset.upgradeLevels[3] = CreateWeaponLevel(new[] { WeaponUpgradeType.Area, WeaponUpgradeType.Damage }, new float[] { 20, 1 }, new bool[] { true, false }, "Area +20%, Damage +1.");
        asset.upgradeLevels[4] = CreateWeaponLevel(new[] { WeaponUpgradeType.Cooldown, WeaponUpgradeType.Damage }, new float[] { 10, 2 }, new bool[] { true, false }, "Cooldown reduced by 10%, Damage +2.");
        asset.upgradeLevels[5] = CreateWeaponLevel(new[] { WeaponUpgradeType.Area, WeaponUpgradeType.Damage }, new float[] { 20, 1 }, new bool[] { true, false }, "Area +20%, Damage +1.");
        asset.upgradeLevels[6] = CreateWeaponLevel(new[] { WeaponUpgradeType.Cooldown, WeaponUpgradeType.Damage }, new float[] { 10, 1 }, new bool[] { true, false }, "Cooldown reduced by 10%, Damage +1.");
        asset.upgradeLevels[7] = CreateWeaponLevel(new[] { WeaponUpgradeType.Area, WeaponUpgradeType.Damage }, new float[] { 20, 1 }, new bool[] { true, false }, "Area +20%, Damage +1.");

        AssetDatabase.CreateAsset(asset, path + "Garlic.asset");
    }

    private static WeaponUpgradeSO.WeaponUpgradeLevelData CreateWeaponLevel(WeaponUpgradeType[] types, float[] amounts, bool[] percentages, string description)
    {
        return new WeaponUpgradeSO.WeaponUpgradeLevelData
        {
            upgradeTypes = types,
            upgradeAmounts = amounts,
            isPercentageBased = percentages,
            description = description
        };
    }

    private static string[] GetCompatibleWeaponTagsForStat(string upgradeName)
    {
        switch (upgradeName.Replace(" ", "").ToLower())
        {
            case "projectilespeed":
                return new[] { "MagicWand", "KingBible" };
            case "cooldown":
                return new[] { "MagicWand", "KingBible", "Garlic" };
            case "area":
                return new[] { "KingBible", "Garlic" };
            case "duration":
                return new[] { "KingBible" };
            default:
                return new string[0];
        }
    }

    private static string GetPassiveUpgradeDescription(PassiveUpgradeType type, float amount, bool isPercentage)
    {
        switch (type)
        {
            case PassiveUpgradeType.Damage: return $"Raises inflicted damage by {amount}%.";
            case PassiveUpgradeType.Armor: return $"Reduces incoming damage by {amount}.";
            case PassiveUpgradeType.Health: return $"Augments max health by {amount}%.";
            case PassiveUpgradeType.Regen: return $"Restores {amount} health per second.";
            case PassiveUpgradeType.MoveSpeed: return $"Character moves {amount}% faster.";
            case PassiveUpgradeType.Magnet: return $"Pickup range increases by {amount}%.";
            case PassiveUpgradeType.Luck: return $"Raises luck by {amount}%.";
            case PassiveUpgradeType.Growth: return $"Gain {amount}% more experience.";
            case PassiveUpgradeType.Greed: return $"Gain {amount}% more coins.";
            case PassiveUpgradeType.Revival: return $"Revive with {amount} extra life.";
            case PassiveUpgradeType.Amount: return $"Fires {Mathf.RoundToInt(amount)} additional projectiles."; 
            default: return $"{type} +{amount}{(isPercentage ? "%" : "")}";
        }
    }


    private static string GetWeaponUpgradeDescription(WeaponUpgradeType type, float amount, bool isPercentage)
    {
        switch (type)
        {
            case WeaponUpgradeType.Cooldown: return $"Reduces cooldown by {amount}%.";
            case WeaponUpgradeType.Area: return $"Increases attack area by {amount}%.";
            case WeaponUpgradeType.Speed: return $"Increases projectile speed by {amount}%.";
            case WeaponUpgradeType.Duration: return $"Extends effect duration by {amount}%.";
            case WeaponUpgradeType.Amount: return $"Fires {Mathf.RoundToInt(amount)} additional projectiles.";
            default: return $"{type} +{amount}{(isPercentage ? "%" : "")}";
        }
    }
}
