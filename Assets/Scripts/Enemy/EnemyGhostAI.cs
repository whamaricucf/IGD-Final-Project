using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyGhostAI : MonoBehaviour, IDamageable
{
    public Transform player;
    private NavMeshAgent agent;

    [Header("Ghost Settings")]
    public EnemyData baseData; // <-- ScriptableObject reference (was EnemyStats)
    private EnemyData runtimeData; // Runtime copy so we can modify
    private bool isMiniGhost = false;
    private bool hasSplit = false;

    [Header("Mini Ghost Settings")]
    public GameObject miniGhostPrefab; // Prefab for mini-ghosts
    public int numberOfMiniGhosts = 2;
    public float splitHealthThreshold = 50f; // When to split (health value)

    public static event System.Action OnEnemyDied; // <-- New event for when ghost dies


    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = false;

        // Create a runtime copy of data (so we don't modify the ScriptableObject directly)
        if (runtimeData == null )
        {
            runtimeData = ScriptableObject.CreateInstance<EnemyData>();

            runtimeData.speed = baseData.speed;
            runtimeData.health = baseData.health;
            runtimeData.power = baseData.power;
            runtimeData.knockback = baseData.knockback;
            runtimeData.experience = baseData.experience;

            // Slightly randomize speed and health
            runtimeData.speed += Random.Range(-0.5f, 0.5f);
            runtimeData.health += Random.Range(-10, 10);

            // Apply scaling based on survival time
            ScaleRuntimeStats();
        }

        agent.speed = runtimeData.speed;
    }
    private void ScaleRuntimeStats()
    {
        float minutesSurvived = Time.timeSinceLevelLoad / 60f; // How many minutes survived

        if (isMiniGhost)
        {
            // Mini ghosts scale slower (10% per minute)
            float healthMultiplier = 1f + (minutesSurvived * 0.1f);
            runtimeData.health = Mathf.RoundToInt(runtimeData.health * healthMultiplier);

            // Optional: Scale speed (commented out for now)
            // runtimeData.speed += minutesSurvived * 0.05f; // Slightly faster over time

            // Power Scaling
            runtimeData.power += Mathf.FloorToInt(minutesSurvived * 0.25f); // Stronger hits
        }
        else
        {
            // Health Scaling
            float healthMultiplier = 1f + (minutesSurvived * 0.2f); // +20% per minute
            runtimeData.health = Mathf.RoundToInt(runtimeData.health * healthMultiplier);

            // Optional: Scale speed (commented out for now)
            // runtimeData.speed += minutesSurvived * 0.1f; // Slightly faster over time

            // Power Scaling
            runtimeData.power += Mathf.FloorToInt(minutesSurvived * 0.5f); // Stronger hits
        }
    }

    private void Update()
    {
        if (player == null) return;

        if (!agent.enabled) return; // <-- Skip if agent disabled

        if (!isMiniGhost && !hasSplit && runtimeData.health <= splitHealthThreshold)
        {
            SplitIntoMiniGhosts();
            return; // Prevent further processing after splitting
        }

        ChasePlayer();
    }

    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
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



    private void SplitIntoMiniGhosts()
    {
        hasSplit = true; // Ensure it only splits once

        for (int i = 0; i < numberOfMiniGhosts; i++)
        {
            GameObject miniGhost = Instantiate(miniGhostPrefab, transform.position, Quaternion.identity);
            EnemyGhostAI miniGhostAI = miniGhost.GetComponent<EnemyGhostAI>();
            miniGhostAI.InitializeAsMini(this.runtimeData);
            miniGhostAI.player = this.player; // Assign player reference
        }

        Destroy(gameObject); // Remove original ghost (no EXP reward yet)
    }

    public void InitializeAsMini(EnemyData parentData)
    {
        isMiniGhost = true;

        runtimeData = ScriptableObject.CreateInstance<EnemyData>();

        runtimeData.speed = 5f + Random.Range(-0.5f, 0.5f);
        runtimeData.health = 50 + Random.Range(-5, 5);
        runtimeData.power = parentData.power;
        runtimeData.knockback = parentData.knockback;
        runtimeData.experience = Mathf.CeilToInt(parentData.experience / 3f);

        agent = GetComponent<NavMeshAgent>();
        agent.speed = runtimeData.speed;
    }

    // Example method to take damage (call this from weapon or player attack scripts)
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
        // Award experience points to player
        PlayerExperience.Instance?.GainExperience(runtimeData.experience);

        // Fire global death event
        OnEnemyDied?.Invoke(); // <-- Broadcast that an enemy died

        Destroy(gameObject);
    }

}