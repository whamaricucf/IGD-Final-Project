using UnityEngine;
using System.Collections;
using static UpgradeTypes;

public class KingBible : Weapon, IWeaponUpgradeable
{
    private Coroutine orbitCoroutine;
    public Transform playerTransform;
    private GameObject[] activeBibles;

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
        StartOrbit(); // Start firing behavior
    }


    private void OnDisable()
    {
        if (orbitCoroutine != null)
            StopCoroutine(orbitCoroutine);

        if (activeBibles != null)
        {
            foreach (var bible in activeBibles)
            {
                if (bible != null)
                    bible.SetActive(false);
            }
        }
    }

    public void StartOrbit()
    {
        if (orbitCoroutine != null)
            StopCoroutine(orbitCoroutine);

        orbitCoroutine = StartCoroutine(OrbitBible());
    }

    private IEnumerator OrbitBible()
    {
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            SpawnOrbitingBibles();

            // Bibles active for 'duration' seconds
            yield return new WaitForSeconds(duration);

            // Tell each bible to start fading out
            if (activeBibles != null)
            {
                foreach (var bible in activeBibles)
                {
                    if (bible != null && bible.TryGetComponent(out KingBibleProjectile proj))
                    {
                        proj.StartFadeOut();
                    }
                }
            }

            // Wait a little extra to allow fade to complete (optional, if your fade time is short)
            yield return new WaitForSeconds(0.5f);

            // Wait for cooldown before respawning
            yield return new WaitForSeconds(Mathf.Max(0.05f, cooldown));
        }
    }

    private void SpawnOrbitingBibles()
    {
        if (playerTransform == null) return;

        if (activeBibles != null)
        {
            foreach (var bible in activeBibles)
            {
                if (bible != null)
                    bible.SetActive(false);
            }
        }

        activeBibles = new GameObject[Mathf.Max(1, amount)];

        for (int i = 0; i < activeBibles.Length; i++)
        {
            GameObject bible = ObjectPooler.Instance.SpawnFromPool("KingBible", playerTransform.position, Quaternion.identity);
            if (bible.TryGetComponent(out KingBibleProjectile proj))
            {
                float startAngle = i * (360f / activeBibles.Length);
                proj.Activate(playerTransform, 1.5f * area, startAngle, damage, knockback, critChance, critMulti, speed);
            }
            activeBibles[i] = bible;
        }
    }

    public void ApplyWeaponUpgrade(WeaponUpgradeType type, float amount, bool isPercentage)
    {
        switch (type)
        {
            case WeaponUpgradeType.Amount:
                weaponAmountBonus += Mathf.RoundToInt(isPercentage ? weaponAmountBonus * (amount / 100f) : amount);
                break;
            case WeaponUpgradeType.Area:
                baseArea = isPercentage ? baseArea * (1f + amount / 100f) : baseArea + amount;
                break;
            case WeaponUpgradeType.Cooldown:
                baseCooldown = isPercentage ? baseCooldown * (1f - amount / 100f) : baseCooldown - amount;
                break;
            case WeaponUpgradeType.Damage:
                baseDamage = isPercentage ? baseDamage * (1f + amount / 100f) : baseDamage + amount;
                break;
            case WeaponUpgradeType.Duration:
                baseDuration = isPercentage ? baseDuration * (1f + amount / 100f) : baseDuration + amount;
                break;
            case WeaponUpgradeType.Speed:
                baseSpeed = isPercentage ? baseSpeed * (1f + amount / 100f) : baseSpeed + amount;
                break;
        }
    }


    public override void RefreshWeaponStats()
    {
        base.RefreshWeaponStats();
    }

    public void ReinitializeWeaponAfterUpgrade()
    {
        if (orbitCoroutine != null)
            StopCoroutine(orbitCoroutine);

        RefreshWeaponStats();
        StartOrbit();
    }

    public string GetWeaponIdentifier()
    {
        return weaponType;
    }
}
