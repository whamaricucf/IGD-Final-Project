using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpiderAI : MonoBehaviour, IDamageable
{
    public Transform player;
    private NavMeshAgent agent;

    [Header("Spider Settings")]
    public EnemyData baseData; // ScriptableObject
    private EnemyData runtimeData;

    [Header("Web Settings")]
    public GameObject webPrefab;
    public float webDropInterval = 5f;
    private float webDropTimer = 0f;

    public static event System.Action OnEnemyDied; // <-- New event for when spider dies

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = false;

        if (runtimeData == null)
        {
            runtimeData = ScriptableObject.CreateInstance<EnemyData>();

            runtimeData.speed = baseData.speed;
            runtimeData.health = baseData.health;
            runtimeData.power = baseData.power;
            runtimeData.knockback = baseData.knockback;
            runtimeData.experience = baseData.experience;

            runtimeData.speed += Random.Range(-0.5f, 0.5f);
            runtimeData.health += Random.Range(-10, 10);

            ScaleRuntimeStats();
        }

        agent.speed = runtimeData.speed;
    }

    private void Update()
    {
        if (player == null) return;

        if (!agent.enabled) return; // <-- Skip if agent disabled

        agent.SetDestination(player.position);

        webDropTimer += Time.deltaTime;
        if (webDropTimer >= webDropInterval)
        {
            DropWeb();
            webDropTimer = 0f;
        }
    }

    private void DropWeb()
    {
        if (webPrefab != null)
        {
            Vector3 spawnPosition = transform.position;
            spawnPosition.y -= 0.5f; // Drop it slightly lower

            Instantiate(webPrefab, spawnPosition, Quaternion.identity);
        }
    }

    private void ScaleRuntimeStats()
    {
        float minutesSurvived = Time.timeSinceLevelLoad / 60f;
        float healthMultiplier = 1f + (minutesSurvived * 0.2f);
        runtimeData.health = Mathf.RoundToInt(runtimeData.health * healthMultiplier);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(runtimeData.power);

                // Bounce away
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 knockbackDirection = (transform.position - other.transform.position).normalized;
                    knockbackDirection.y = 0f; // Keep bounce horizontal
                    rb.velocity = Vector3.zero; // Cancel current motion
                    rb.AddForce(knockbackDirection * (runtimeData.knockback * 2f), ForceMode.Impulse);

                    StartCoroutine(DisableAgentTemporarily());
                }
            }
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 awayFromOtherEnemy = (transform.position - collision.transform.position).normalized;
                awayFromOtherEnemy.y = 0f; // Only slide horizontally
                rb.AddForce(awayFromOtherEnemy * 2f, ForceMode.Force); // Light continuous push
            }
        }
    }

    private IEnumerator DisableAgentTemporarily()
    {
        agent.enabled = false;

        yield return new WaitForSeconds(0.3f); // Wait during knockback

        // Stop any lingering momentum
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }

        agent.enabled = true;

        // Wait a few frames to ensure the NavMeshAgent is fully re-registered
        yield return null;
        yield return null;

        if (agent.isOnNavMesh && player != null && agent.enabled)
        {
            agent.SetDestination(player.position);
        }
    }



    public void TakeDamage(int dmg, float knockback, Vector3 sourcePos, float critChance, float critMulti)
    {
        // Crit logic
        float finalDamage = dmg;
        if (Random.value < critChance)
        {
            finalDamage *= critMulti;
        }

        runtimeData.health -= Mathf.RoundToInt(finalDamage);

        // Knockback
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 dir = (transform.position - sourcePos).normalized;
            dir.y = 0f;
            rb.AddForce(dir * knockback, ForceMode.Impulse);
        }
        StartCoroutine(DisableAgentTemporarily());

        if (runtimeData.health <= 0)
        {
            Die();
        }
    }


    private void Die()
    {
        PlayerExperience.Instance?.GainExperience(runtimeData.experience);

        // Fire global death event
        OnEnemyDied?.Invoke(); // <-- Broadcast that an enemy died

        Destroy(gameObject);
    }

}
