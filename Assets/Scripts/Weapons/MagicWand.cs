using UnityEngine;
using System.Collections;
using static UpgradeTypes;

public class MagicWand : Weapon, IWeaponUpgradeable
{
    private Coroutine attackCoroutine;
    public Transform firePoint;

    private void OnEnable()
    {
        if (weaponData != null)
        {
            weaponData = Instantiate(weaponData); // Fresh clone
            ApplyWeaponData(); // Initialize base stats
        }

        // Assign firePoint = Player
        if (firePoint == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                firePoint = player.transform;
            else
                Debug.LogWarning("[MagicWand] Player not found! FirePoint cannot be assigned.");
        }

        UpgradeManager.Instance?.RefreshWeaponUpgradesOnWeapon(this); // Reapply any active upgrades
        RefreshWeaponStats(); // Refresh live stats
        StartAttacking(); // Begin firing
    }



    private void OnDisable()
    {
        if (attackCoroutine != null)
            StopCoroutine(attackCoroutine);
    }

    public void StartAttacking()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        attackCoroutine = StartCoroutine(FireProjectiles());
    }

    private IEnumerator FireProjectiles()
    {
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            GameObject target = FindClosestEnemy();
            if (target != null)
            {
                int totalProjectiles = Mathf.Max(1, amount);
                float maxArc = 3f; // Max arc spread in degrees
                float arcStep = totalProjectiles > 1 ? maxArc / (totalProjectiles - 1) : 0f;
                float startingAngle = -maxArc / 2f;

                for (int i = 0; i < totalProjectiles; i++)
                {
                    float angleOffset = startingAngle + arcStep * i;
                    SpawnProjectile(angleOffset);

                    if (i < totalProjectiles - 1)
                        yield return new WaitForSeconds(projInterval);
                }
            }

            yield return new WaitForSeconds(Mathf.Max(0.05f, cooldown));
        }
    }


    private void SpawnProjectile(float angleOffset)
    {
        if (firePoint == null) return;

        GameObject proj = ObjectPooler.Instance.SpawnFromPool("MagicWandBullet", firePoint.position, Quaternion.identity);

        if (proj.TryGetComponent(out MagicWandProjectile projectile))
        {
            Vector3 shootDir = GetShootDirection();

            // Apply small rotation
            shootDir = Quaternion.Euler(0, angleOffset, 0) * shootDir;

            projectile.Launch(shootDir, speed, damage, pierce, knockback, critChance, critMulti);
        }
    }


    private Vector3 GetShootDirection()
    {
        GameObject target = FindClosestEnemy();
        if (target != null)
        {
            Vector3 targetPoint = target.transform.position;

            if (target.TryGetComponent(out Collider targetCollider))
            {
                targetPoint = targetCollider.bounds.center;
            }

            return (targetPoint - firePoint.position).normalized;
        }
        else
        {
            return firePoint.forward;
        }
    }


    private GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float minDistance = Mathf.Infinity;
        Vector3 pos = firePoint.position;

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(enemy.transform.position, pos);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = enemy;
            }
        }

        return closest;
    }

    public void ApplyWeaponUpgrade(WeaponUpgradeType type, float amount, bool isPercentage)
    {
        switch (type)
        {
            case WeaponUpgradeType.Amount:
                weaponAmountBonus += Mathf.RoundToInt(isPercentage ? weaponAmountBonus * (amount / 100f) : amount);
                break;
            case WeaponUpgradeType.Cooldown:
                baseCooldown = isPercentage ? baseCooldown * (1f - amount / 100f) : baseCooldown - amount;
                break;
            case WeaponUpgradeType.Damage:
                baseDamage = isPercentage ? baseDamage * (1f + amount / 100f) : baseDamage + amount;
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
        RefreshWeaponStats();
        StartAttacking();
    }

    public string GetWeaponIdentifier()
    {
        return weaponType;
    }
}
