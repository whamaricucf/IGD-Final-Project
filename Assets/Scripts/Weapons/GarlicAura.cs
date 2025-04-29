using UnityEngine;
using System.Collections;
using static UpgradeTypes;

public class GarlicAura : Weapon, IWeaponUpgradeable
{
    private Coroutine damageCoroutine;
    public LayerMask enemyLayerMask;
    private Transform garlicVisual;

    private void OnEnable()
    {
        Debug.Log("[GarlicAura] OnEnable called");

        if (weaponData != null)
        {
            weaponData = Instantiate(weaponData);
            ApplyWeaponData();
            weaponType = weaponData.wepName;
            Debug.Log("[GarlicAura] WeaponData instantiated and applied");
        }
        else
        {
            Debug.LogWarning("[GarlicAura] No WeaponData assigned!");
        }

        RefreshWeaponStats();
        Debug.Log($"[GarlicAura] Stats after RefreshWeaponStats: Damage={damage}, Area={area}, Cooldown={cooldown}");

        StartAura();
    }

    private void OnDisable()
    {
        if (damageCoroutine != null)
            StopCoroutine(damageCoroutine);

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnStatsChanged -= RefreshAuraVisual;
    }

    public void StartAura()
    {
        Debug.Log("[GarlicAura] StartAura called");

        if (damageCoroutine != null)
            StopCoroutine(damageCoroutine);

        damageCoroutine = StartCoroutine(DamageEnemies());
    }

    private IEnumerator DamageEnemies()
    {
        Debug.Log("[GarlicAura] DamageEnemies coroutine started");

        if (enemyLayerMask == 0)
        {
            Debug.LogWarning("[GarlicAura] EnemyLayerMask is NOT set!");
        }

        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            Debug.Log("[GarlicAura] Attempting to deal damage in radius " + area);

            Collider[] hits = Physics.OverlapSphere(transform.position, area, enemyLayerMask);

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out IDamageable target))
                {
                    Debug.Log("[GarlicAura] Hit enemy: " + hit.name);
                    target.TakeDamage(Mathf.RoundToInt(damage), knockback, transform.position, critChance, critMulti, false);
                }
            }

            yield return new WaitForSeconds(Mathf.Max(0.05f, cooldown));
        }
    }

    public override void ApplyWeaponUpgrade(WeaponUpgradeType type, float amount, bool isPercentage)
    {
        switch (type)
        {
            case WeaponUpgradeType.Area:
                baseArea = isPercentage ? baseArea * (1f + amount / 100f) : baseArea + amount;
                break;
            case WeaponUpgradeType.Damage:
                baseDamage = isPercentage ? baseDamage * (1f + amount / 100f) : baseDamage + amount;
                break;
            case WeaponUpgradeType.Cooldown:
                baseCooldown = isPercentage ? baseCooldown * (1f - amount / 100f) : baseCooldown - amount;
                break;
        }
        Debug.Log("[GarlicAura] Weapon upgrade applied: " + type);
    }

    public override void RefreshWeaponStats()
    {
        base.RefreshWeaponStats();
        UpdateAuraVisualScale();
    }

    private void UpdateAuraVisualScale()
    {
        if (garlicVisual == null)
            garlicVisual = transform.Find("GarlicVisual");

        if (garlicVisual != null)
        {
            float diameter = area * 2f;
            garlicVisual.localScale = new Vector3(diameter, 0.1f, diameter);
            Debug.Log("[GarlicAura] GarlicVisual scale updated");
        }
        else
        {
            Debug.LogWarning("[GarlicAura] GarlicVisual child not found!");
        }
    }

    private void RefreshAuraVisual()
    {
        RefreshWeaponStats();
    }

    public override void ReinitializeWeaponAfterUpgrade()
    {
        if (damageCoroutine != null)
            StopCoroutine(damageCoroutine);

        RefreshWeaponStats();
        StartAura();
    }

    public override string GetWeaponIdentifier()
    {
        return weaponType;
    }
}
