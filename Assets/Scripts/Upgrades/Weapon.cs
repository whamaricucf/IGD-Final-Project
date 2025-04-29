using UnityEngine;
using System;
using System.Collections;
using System.Linq;

public class Weapon : MonoBehaviour, IWeaponUpgradeable
{
    // Live runtime stats
    public float damage;
    public float speed;
    public float area;
    public float cooldown;
    public float duration;
    public float projInterval;
    public float hitDelay;
    public float knockback;
    public float critChance;
    public float critMulti;
    public int maxLvl;
    public int rarity;
    public int amount;
    public int pierce;
    public int limit;
    public bool wallBlock;

    public string weaponType;
    public WeaponData weaponData;

    // Base stats (copied at start, never changed after)
    protected float baseDamage;
    protected float baseSpeed;
    protected float baseArea;
    protected float baseCooldown;
    protected float baseDuration;
    protected float baseProjInterval;
    protected float baseHitDelay;
    protected float baseKnockback;
    protected float baseCritChance;
    protected float baseCritMulti;
    protected int baseAmount;
    protected int basePierce;
    protected int baseLimit;

    protected int weaponAmountBonus = 0;

    // Public Getters
    public float BaseDamage => baseDamage;
    public float BaseSpeed => baseSpeed;
    public float BaseArea => baseArea;
    public float BaseCooldown => baseCooldown;
    public float BaseDuration => baseDuration;
    public float BaseProjInterval => baseProjInterval;
    public float BaseHitDelay => baseHitDelay;
    public float BaseKnockback => baseKnockback;
    public float BaseCritChance => baseCritChance;
    public float BaseCritMulti => baseCritMulti;
    public int BaseAmount => baseAmount;
    public int BasePierce => basePierce;
    public int BaseLimit => baseLimit;

    public int WeaponAmountBonus
    {
        get => weaponAmountBonus;
        set => weaponAmountBonus = value;
    }

    protected virtual void Start()
    {
        if (weaponData != null)
        {
            weaponData = Instantiate(weaponData);
            ApplyWeaponData();
            weaponType = weaponData.wepName;
        }
    }

    public virtual void ApplyWeaponData()
    {
        if (weaponData == null) return;

        baseDamage = weaponData.baseDMG;
        baseSpeed = weaponData.spd;
        baseArea = weaponData.area;
        baseCooldown = weaponData.cd;
        baseDuration = weaponData.duration;
        baseProjInterval = weaponData.projInterval;
        baseHitDelay = weaponData.hitDelay;
        baseKnockback = weaponData.knockback;
        baseCritChance = weaponData.critChance;
        baseCritMulti = weaponData.critMulti;
        baseAmount = weaponData.amount;
        basePierce = weaponData.pierce;
        baseLimit = weaponData.limit;
        wallBlock = weaponData.wallBlock;

        ResetToBaseStats();
    }

    // 🧹 NEW METHOD — Safely reset weapon to pure base stats
    public virtual void ResetToBaseStats()
    {
        weaponAmountBonus = 0;

        damage = baseDamage;
        area = baseArea;
        cooldown = baseCooldown;
        projInterval = baseProjInterval;
        speed = baseSpeed;
        duration = baseDuration;
        knockback = baseKnockback;
        critChance = baseCritChance;
        critMulti = baseCritMulti;
        amount = baseAmount;
        pierce = basePierce;
    }

    public virtual void RefreshWeaponStats()
    {
        if (PlayerStats.Instance == null)
            return;

        damage = baseDamage * PlayerStats.Instance.damage;
        area = baseArea * PlayerStats.Instance.area;
        cooldown = Mathf.Max(0.05f, baseCooldown * (1f - PlayerStats.Instance.cd));
        projInterval = Mathf.Max(0.05f, baseProjInterval * (1f - PlayerStats.Instance.cd));
        speed = baseSpeed * PlayerStats.Instance.projSpd;
        duration = baseDuration * PlayerStats.Instance.duration;
        critChance = baseCritChance + (PlayerStats.Instance.luck * 0.01f);

        knockback = baseKnockback;
        amount = baseAmount + weaponAmountBonus + PlayerStats.Instance.amount;
        pierce = basePierce;
    }

    public virtual string GetWeaponIdentifier()
    {
        return weaponType.Replace(" ", "");
    }

    public virtual void ApplyWeaponUpgrade(UpgradeTypes.WeaponUpgradeType type, float amount, bool isPercentage)
    {
        switch (type)
        {
            case UpgradeTypes.WeaponUpgradeType.Amount:
                WeaponAmountBonus += Mathf.RoundToInt(amount);
                break;
            case UpgradeTypes.WeaponUpgradeType.Cooldown:
                if (isPercentage)
                    baseCooldown *= (1f - amount / 100f);
                else
                    baseCooldown -= amount;
                baseCooldown = Mathf.Max(0.05f, baseCooldown);
                break;
            case UpgradeTypes.WeaponUpgradeType.Area:
                baseArea = isPercentage ? baseArea * (1f + amount / 100f) : baseArea + amount;
                break;
            case UpgradeTypes.WeaponUpgradeType.Damage:
                baseDamage = isPercentage ? baseDamage * (1f + amount / 100f) : baseDamage + amount;
                break;
            case UpgradeTypes.WeaponUpgradeType.Speed:
                baseSpeed = isPercentage ? baseSpeed * (1f + amount / 100f) : baseSpeed + amount;
                break;
            case UpgradeTypes.WeaponUpgradeType.Duration:
                baseDuration = isPercentage ? baseDuration * (1f + amount / 100f) : baseDuration + amount;
                break;
            case UpgradeTypes.WeaponUpgradeType.Pierce:
                basePierce = Mathf.Max(0, basePierce + Mathf.RoundToInt(amount));
                break;
        }

        RefreshWeaponStats();
    }
    public virtual void InitializeWeaponDataIfNeeded()
    {
        if (weaponData != null)
        {
            weaponData = Instantiate(weaponData);
            ApplyWeaponData();
        }
    }

    public virtual void ReinitializeWeaponAfterUpgrade()
    {
        if (this is MagicWand wand)
        {
            wand.RefreshWeaponStats();
            wand.ApplyAllActiveUpgrades();
            wand.StartAttacking();
        }
        else if (this is KingBible bible)
        {
            bible.RefreshWeaponStats();
            bible.ApplyAllActiveUpgrades();
            bible.StartOrbit();
        }
        else if (this is GarlicAura garlic)
        {
            garlic.RefreshWeaponStats();
            garlic.ApplyAllActiveUpgrades();
            garlic.StartAura();
        }
    }
    public void ApplyAllActiveUpgrades()
    {
        foreach (var upgrade in UpgradeManager.Instance.GetPickedUpgrades())
        {
            if (upgrade.upgrade is WeaponUpgradeSO weaponUpgrade)
            {
                if (weaponUpgrade.compatibleWeaponTags.Any(tag => tag.Replace(" ", "") == GetWeaponIdentifier()))
                {
                    if (weaponUpgrade.GetCurrentLevel() > 0)
                    {
                        weaponUpgrade.ApplyUpgrade(this as IWeaponUpgradeable);
                    }
                }
            }
            else if (upgrade.upgrade is PassiveUpgradeSO passiveUpgrade)
            {
                passiveUpgrade.ApplyUpgrade(this as IWeaponUpgradeable);
            }
        }
    }

}
