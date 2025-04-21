using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarlicAura : MonoBehaviour
{
    public float damage = 5f;
    public float hitDelay = 0.5f;
    public float radius = 2f;

    private SphereCollider garlicCollider;
    private Dictionary<GameObject, float> enemyHitTimers = new Dictionary<GameObject, float>();

    void Start()
    {
        garlicCollider = gameObject.AddComponent<SphereCollider>();
        garlicCollider.isTrigger = true;
        garlicCollider.radius = radius;
    }

    void Update()
    {
        // Clean up expired timers
        List<GameObject> keysToRemove = new List<GameObject>();
        foreach (var entry in enemyHitTimers)
        {
            if (Time.time >= entry.Value)
                keysToRemove.Add(entry.Key);
        }

        foreach (var key in keysToRemove)
            enemyHitTimers.Remove(key);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        if (!enemyHitTimers.ContainsKey(other.gameObject) || Time.time >= enemyHitTimers[other.gameObject])
        {
            enemyHitTimers[other.gameObject] = Time.time + hitDelay;
            other.GetComponent<MonoBehaviour>()?.SendMessage("TakeDamage", (int)damage, SendMessageOptions.DontRequireReceiver);
        }
    }
}
