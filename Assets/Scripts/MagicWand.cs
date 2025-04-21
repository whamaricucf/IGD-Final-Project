using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicWand : MonoBehaviour
{
    public float fireRate = 1f; // How often it fires
    private float fireTimer;

    public string projectileTag = "MagicWandBullet";
    public float projectileSpeed = 10f;
    public float damage = 10f;

    private Transform target;

    void Update()
    {
        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f)
        {
            FindNearestEnemy();

            if (target != null)
            {
                FireProjectileAt(target);
                fireTimer = fireRate;
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

    void FireProjectileAt(Transform enemy)
    {
        Vector3 spawnPos = transform.position + (enemy.position - transform.position).normalized * 1f;
        GameObject bullet = ObjectPooler.Instance.SpawnFromPool(projectileTag, spawnPos, Quaternion.identity);

        MagicWandProjectile proj = bullet.GetComponent<MagicWandProjectile>();
        if (proj != null)
        {
            proj.Launch(enemy.position - transform.position, projectileSpeed, damage);
        }
    }
}
