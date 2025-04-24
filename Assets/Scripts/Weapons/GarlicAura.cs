using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarlicAura : MonoBehaviour
{
    private SphereCollider garlicCollider;
    private Dictionary<GameObject, float> enemyHitTimers = new Dictionary<GameObject, float>();

    public WeaponData garlicStats;

    void Start()
    {
        if (garlicStats != null)
        {
            garlicCollider = gameObject.AddComponent<SphereCollider>();
            garlicCollider.isTrigger = true;
            garlicCollider.radius = garlicStats.area;
        }
    }

    void Update()
    {
        List<GameObject> toRemove = new List<GameObject>();
        foreach (var entry in enemyHitTimers)
        {
            if (Time.time >= entry.Value)
                toRemove.Add(entry.Key);
        }

        foreach (var key in toRemove)
            enemyHitTimers.Remove(key);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        // Wall block logic
        if (garlicStats.wallBlock)
        {
            Vector3 dirToEnemy = (other.transform.position - transform.position).normalized;
            float dist = Vector3.Distance(transform.position, other.transform.position);
            if (Physics.Raycast(transform.position, dirToEnemy, dist, LayerMask.GetMask("Wall")))
                return;
        }

        if (!enemyHitTimers.ContainsKey(other.gameObject) || Time.time >= enemyHitTimers[other.gameObject])
        {
            enemyHitTimers[other.gameObject] = Time.time + garlicStats.hitDelay;

            IDamageable target = other.GetComponent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(
                    garlicStats.baseDMG,
                    garlicStats.knockback,
                    transform.position,
                    garlicStats.critChance,
                    garlicStats.critMulti
                );
            }
        }
    }
}
