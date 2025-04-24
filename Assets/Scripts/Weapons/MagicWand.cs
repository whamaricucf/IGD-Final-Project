using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicWand : MonoBehaviour
{
    private float fireTimer;
    public string projectileTag = "MagicWandBullet";
    public WeaponData wandStats;

    private Transform target;

    private void Start()
    {
        fireTimer = 0f;
    }

    void Update()
    {
        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f && wandStats != null)
        {
            FindNearestEnemy();

            if (target != null)
            {
                StartCoroutine(FireProjectilesWithInterval(target));
                fireTimer = wandStats.cd;
            }
        }
    }

    void FindNearestEnemy()
    {
        float closestDistance = float.MaxValue;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        target = null;

        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                target = enemy.transform;
            }
        }
    }

    IEnumerator FireProjectilesWithInterval(Transform enemy)
    {
        Vector3 baseDir = (enemy.position - transform.position).normalized;

        for (int i = 0; i < wandStats.amount; i++)
        {
            // Optional: slight angle offset
            float angleOffset = (i - (wandStats.amount - 1) / 2f) * 5f;
            Vector3 dir = Quaternion.Euler(0, angleOffset, 0) * baseDir;

            Vector3 spawnPos = transform.position + dir * 1f;
            GameObject bullet = ObjectPooler.Instance.SpawnFromPool(projectileTag, spawnPos, Quaternion.identity);

            MagicWandProjectile proj = bullet.GetComponent<MagicWandProjectile>();
            if (proj != null)
            {
                proj.Launch(
                    dir,
                    wandStats.spd,
                    wandStats.baseDMG,
                    wandStats.pierce,
                    wandStats.knockback,
                    wandStats.critChance,
                    wandStats.critMulti
                );
            }

            // Wait before firing next projectile if interval is greater than 0
            if (wandStats.projInterval > 0f)
            {
                yield return new WaitForSeconds(wandStats.projInterval);
            }
        }
    }
}
