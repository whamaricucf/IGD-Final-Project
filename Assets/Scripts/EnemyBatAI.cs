using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBatAI : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent agent;

    [Header("Bat Settings")]
    public EnemyData baseData; // <-- ScriptableObject for original bat stats
    private EnemyData runtimeData; // Runtime copy of data

    [Header("Bombing Run Settings")]
    public float timeBeforeBombingRun = 60f; // Time before switching to bombing run
    private bool bombingRunActive = false;
    private Vector3 bombingTarget;

    public static event System.Action OnEnemyDied; // <-- New event for when bat dies

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = false;

        if (runtimeData == null)
        {
            // Create a runtime copy so we don't modify the original ScriptableObject
            runtimeData = ScriptableObject.CreateInstance<EnemyData>();

            runtimeData.speed = baseData.speed;
            runtimeData.health = baseData.health;
            runtimeData.power = baseData.power;
            runtimeData.knockback = baseData.knockback;
            runtimeData.experience = baseData.experience;

            // Randomize speed slightly (small natural variation)
            runtimeData.speed += Random.Range(-0.5f, 0.5f);
            runtimeData.health += Random.Range(-10, 10);

            // Apply scaling based on survival time
            ScaleRuntimeStats();
        }
        // Set initial speed of NavMeshAgent
        agent.speed = runtimeData.speed;
    }

    private void ScaleRuntimeStats()
    {
        float minutesSurvived = Time.timeSinceLevelLoad / 60f; // How many minutes survived

        // Health Scaling
        float healthMultiplier = 1f + (minutesSurvived * 0.2f); // +20% per minute
        runtimeData.health = Mathf.RoundToInt(runtimeData.health * healthMultiplier);

        // Optional: Scale speed (commented out for now)
        // runtimeData.speed += minutesSurvived * 0.1f; // Slightly faster over time

        // Power Scaling
        runtimeData.power += Mathf.FloorToInt(minutesSurvived * 0.5f); // Stronger hits
    }

    private void Update()
    {
        if (player == null) return;

        if (!agent.enabled) return; // <-- Skip if agent disabled

        if (bombingRunActive)
        {
            BombingRunUpdate();
        }
        else
        {
            ChasePlayer();
        }

        // Check if it's time to activate bombing runs
        if (Time.timeSinceLevelLoad > timeBeforeBombingRun && !bombingRunActive)
        {
            ActivateBombingRun();
        }
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
        yield return new WaitForSeconds(0.3f); // Pause for 0.3 seconds
        agent.enabled = true;
    }

    private void ActivateBombingRun()
    {
        bombingRunActive = true;
        agent.speed = runtimeData.speed + Random.Range(10f, 15f); // Randomized fast bombing speed
        ChooseBombingTarget();
    }

    private void ChooseBombingTarget()
    {
        // Pick a random far position beyond the screen centered on player
        Vector3 direction = (transform.position - player.position).normalized;
        bombingTarget = player.position + direction * 30f; // 30 units away from player
        agent.SetDestination(bombingTarget);
    }

    private void BombingRunUpdate()
    {
        if (Vector3.Distance(transform.position, bombingTarget) < 2f)
        {
            // Bombing run complete, choose a new one
            ChooseBombingTarget();
        }
    }

    // Example method to take damage
    public void TakeDamage(int damage)
    {
        runtimeData.health -= damage;

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