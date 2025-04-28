using UnityEngine;

public class Weapon : MonoBehaviour
{
    // Live runtime stats
    public float damage;
    public float speed;
    public float area;
    public float cooldown;
    public float duration;
    public float projInterval;
    public float hitDelay;
    public float knockback; // ✅
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

    protected int weaponAmountBonus = 0; // Used for weapon upgrades like "fires 1 more projectile"

    protected virtual void Start()
    {
        if (weaponData != null)
        {
            weaponData = Instantiate(weaponData); // Clone it
            ApplyWeaponData();
            weaponType = weaponData.wepName;
        }

        // AFTER cloning weaponData, tell UpgradeManager to reapply bonuses
        UpgradeManager.Instance?.RefreshWeaponUpgradesOnWeapon(this);
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

        // Initialize live stats to base stats
        RefreshWeaponStats();
    }

    public virtual void RefreshWeaponStats()
    {
        if (PlayerStats.Instance == null)
            return;

        // Correct scaling based on base stats
        damage = baseDamage * PlayerStats.Instance.damage;
        area = baseArea * PlayerStats.Instance.area;
        cooldown = Mathf.Max(0.05f, baseCooldown * (1f - PlayerStats.Instance.cd));
        projInterval = Mathf.Max(0.05f, baseProjInterval * (1f - PlayerStats.Instance.cd));
        speed = baseSpeed * PlayerStats.Instance.projSpd;
        duration = baseDuration * PlayerStats.Instance.duration;
        critChance = baseCritChance + (PlayerStats.Instance.luck * 0.01f);

        knockback = baseKnockback;

        amount = baseAmount + weaponAmountBonus + PlayerStats.Instance.amount;
        pierce = basePierce; // (optional — if you want pierce upgrades, add here)
    }
}
