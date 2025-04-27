using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    public event Action OnStatsChanged;

    [Header("Base Stats (from PlayerData)")]
    public PlayerData playerData;

    public float health;
    public float damage;
    public float speed;
    public float luck;
    public float regen;
    public float area;
    public float projSpd;
    public float duration;
    public float magnet;
    public float growth;
    public int armor;
    public int revival;
    public int amount;

    [Header("Cooldown Handling")]
    public float baseCooldownReduction;
    public float cd;

    [Header("Upgrade Tracking")]
    public int healthLevel = 0;
    public int damageLevel = 0;
    public int speedLevel = 0;
    public int luckLevel = 0;
    public int regenLevel = 0;
    public int areaLevel = 0;
    public int projSpdLevel = 0;
    public int durationLevel = 0;
    public int cdLevel = 0;
    public int armorLevel = 0;
    public int revivalLevel = 0;
    public int amountLevel = 0;
    public int growthLevel = 0;
    public int magnetRangeLevel = 0;

    public string startingWeaponTag;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeStats(PlayerData stats)
    {
        if (stats == null)
        {
            Debug.LogError("PlayerStats: PlayerData is null during initialization!");
            return;
        }
        playerData = stats;

        health = stats.maxHP;
        damage = stats.str;
        speed = stats.movSpd;
        luck = stats.luck;
        regen = stats.regen;
        area = stats.area;
        projSpd = stats.projSpd;
        duration = stats.duration;
        magnet = stats.magnet;
        growth = stats.growth;
        armor = stats.armor;
        revival = stats.revival;
        amount = stats.amount;
        startingWeaponTag = stats.startingWeaponTag;

        baseCooldownReduction = stats.cd;

        // If your PlayerData.cd is 1, you need to treat it as "no bonus", not 95% reduction.
        cd = Mathf.Clamp(1f - baseCooldownReduction, 0f, 0.95f);


        OnStatsChanged?.Invoke();
    }

    public enum StatType
    {
        Health, Damage, Speed, Luck, Regen, Area, ProjSpd, Duration, Cooldown,
        Armor, Revival, Amount, Magnet, Growth
    }

    public void ApplyStatUpgrade(StatType statType, float amount, bool isPercentage)
    {
        switch (statType)
        {
            case StatType.Health:
                health = isPercentage ? health * (1f + amount / 100f) : health + amount;
                healthLevel++;
                break;
            case StatType.Damage:
                damage = isPercentage ? damage * (1f + amount / 100f) : damage + amount;
                damageLevel++;
                break;
            case StatType.Speed:
                speed = isPercentage ? speed * (1f + amount / 100f) : speed + amount;
                speedLevel++;
                break;
            case StatType.Luck:
                luck = isPercentage ? luck * (1f + amount / 100f) : luck + amount;
                luckLevel++;
                break;
            case StatType.Regen:
                regen = isPercentage ? regen * (1f + amount / 100f) : regen + amount;
                regenLevel++;
                break;
            case StatType.Area:
                area = isPercentage ? area * (1f + amount / 100f) : area + amount;
                areaLevel++;
                break;
            case StatType.ProjSpd:
                projSpd = isPercentage ? projSpd * (1f + amount / 100f) : projSpd + amount;
                projSpdLevel++;
                break;
            case StatType.Duration:
                duration = isPercentage ? duration * (1f + amount / 100f) : duration + amount;
                durationLevel++;
                break;
            case StatType.Cooldown:
                baseCooldownReduction += isPercentage ? (amount / 100f) : amount;
                cd = Mathf.Clamp(baseCooldownReduction, 0f, 0.95f);
                cdLevel++;
                break;
            case StatType.Armor:
                armor += Mathf.RoundToInt(amount);
                armorLevel++;
                break;
            case StatType.Revival:
                revival += Mathf.RoundToInt(amount);
                revivalLevel++;
                break;
            case StatType.Amount:
                this.amount += Mathf.RoundToInt(isPercentage ? this.amount * (amount / 100f) : amount);
                amountLevel++;
                break;
            case StatType.Magnet:
                magnet = isPercentage ? magnet * (1f + amount / 100f) : magnet + amount;
                magnetRangeLevel++;
                break;
            case StatType.Growth:
                growth = isPercentage ? growth * (1f + amount / 100f) : growth + amount;
                growthLevel++;
                break;
        }

        OnStatsChanged?.Invoke();
    }

    public void ResetStats()
    {
        healthLevel = damageLevel = speedLevel = luckLevel = regenLevel = areaLevel = projSpdLevel = 0;
        durationLevel = cdLevel = armorLevel = revivalLevel = amountLevel = growthLevel = magnetRangeLevel = 0;
        OnStatsChanged?.Invoke();
    }
}