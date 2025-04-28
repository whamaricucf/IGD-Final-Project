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
        if (weaponData != null)
        {
            weaponData = Instantiate(weaponData); // Fresh clone
            ApplyWeaponData(); // Apply base stats
            weaponType = weaponData.wepName;
        }

        UpgradeManager.Instance?.RefreshWeaponUpgradesOnWeapon(this); // Reapply upgrades

        RefreshWeaponStats(); // Refresh final stats
        StartAura(); // Start aura damaging enemies

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnStatsChanged += RefreshAuraVisual;
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
        if (damageCoroutine != null)
            StopCoroutine(damageCoroutine);

        damageCoroutine = StartCoroutine(DamageEnemies());
    }

    private IEnumerator DamageEnemies()
    {
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, area, enemyLayerMask);

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out IDamageable target))
                {
                    // For Garlic: disableAgent = false
                    target.TakeDamage(Mathf.RoundToInt(damage), knockback, transform.position, critChance, critMulti, false);
                }
            }

            yield return new WaitForSeconds(Mathf.Max(0.05f, cooldown));
        }
    }

    public void ApplyWeaponUpgrade(WeaponUpgradeType type, float amount, bool isPercentage)
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
        }
        else
        {
            Debug.LogWarning("GarlicAura: GarlicVisual not found!");
        }
    }

    private void RefreshAuraVisual()
    {
        RefreshWeaponStats();
    }

    public void ReinitializeWeaponAfterUpgrade()
    {
        if (damageCoroutine != null)
            StopCoroutine(damageCoroutine);

        RefreshWeaponStats();
        StartAura();
    }

    public string GetWeaponIdentifier()
    {
        return weaponType;
    }
}