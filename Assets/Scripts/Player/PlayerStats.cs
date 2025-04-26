using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance; // Singleton instance

    public PlayerData playerData; // Reference to the selected player's data

    // Player's stats pulled from PlayerData
    public float health;
    public float damage;
    public float speed;
    public float luck;
    public float regen;
    public float area;
    public float projSpd;
    public float duration;
    public float cd;
    public float magnet;
    public float growth;
    public int armor;
    public int revival;
    public int amount;

    // Track passive upgrade levels
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

    public string startingWeaponTag; // The initial weapon for the character

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep PlayerStats persistent across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeStats(PlayerData stats)
    {
        if (stats != null)
        {
            health = stats.maxHP;
            damage = stats.str;
            speed = stats.movSpd;
            luck = stats.luck;
            regen = stats.regen;
            area = stats.area;
            projSpd = stats.projSpd;
            duration = stats.duration;
            cd = stats.cd;
            armor = stats.armor;
            magnet = stats.magnet;
            revival = stats.revival;
            amount = stats.amount;
            startingWeaponTag = stats.startingWeaponTag;
        }
    }

    // Increase functions for passive upgrades
    public void IncreaseHealth()
    {
        healthLevel++;
        Debug.Log($"Health upgraded! New health level: {healthLevel}");
    }

    public void IncreaseSpeed()
    {
        speedLevel++;
        Debug.Log($"Speed upgraded! New speed level: {speedLevel}");
    }

    public void IncreaseRegen()
    {
        regenLevel++;
        Debug.Log($"Regen upgraded! New regen level: {regenLevel}");
    }

    public void IncreaseDamage()
    {
        damageLevel++;
        Debug.Log($"Damage upgraded! New damage level: {damageLevel}");
    }

    public void IncreaseArea()
    {
        areaLevel++;
        Debug.Log($"Area upgraded! New area level: {areaLevel}");
    }

    public void IncreaseProjectileSpeed()
    {
        projSpdLevel++;
        Debug.Log($"Projectile speed upgraded! New projectile speed level: {projSpdLevel}");
    }

    public void IncreaseDuration()
    {
        durationLevel++;
        Debug.Log($"Duration upgraded! New duration level: {durationLevel}");
    }

    public void IncreaseCooldown()
    {
        cdLevel++;
        Debug.Log($"Cooldown upgraded! New cooldown level: {cdLevel}");
    }

    public void IncreaseArmor(int armorIncrease)
    {
        armorLevel += armorIncrease;
        Debug.Log($"Armor upgraded! New armor level: {armorLevel}");
    }

    public void IncreaseRevival()
    {
        revivalLevel++;
        Debug.Log($"Revival upgraded! New revival level: {revivalLevel}");
    }

    public void IncreaseAmount()
    {
        amountLevel++;
        Debug.Log($"Amount upgraded! New amount level: {amountLevel}");
    }

    // Methods for increasing stats dynamically (if needed in gameplay)
    public void IncreaseLuck(float amount)
    {
        luck += amount;
        luckLevel++;  // Track luck upgrade level
        Debug.Log("Luck increased! Current luck: " + luck);
    }

    public void IncreaseMagnet(float amount)
    {
        magnet += amount;
        Debug.Log("Magnet range increased! Current magnet: " + magnet);
    }

    public void IncreaseRevivals(int amount)
    {
        revival += amount;
        Debug.Log("Revivals increased! Current revivals: " + revival);
    }

    public void ApplyExperienceMultiplier(float multiplier)
    {
        growth += multiplier;
        Debug.Log("Experience multiplier increased! Current multiplier: " + growth);
    }
}
