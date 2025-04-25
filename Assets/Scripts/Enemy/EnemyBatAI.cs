using UnityEngine;
using UnityEngine.AI;

public class EnemyBatAI : BaseEnemyAI, IDamageable
{
    [Header("Chase Settings")]
    public float maxSpeedMultiplier = 4f; // Max speed multiplier (4x base speed)
    public float accelerationRate = 0.5f; // Rate at which the bat accelerates
    private float currentSpeedMultiplier = 1f; // Current speed multiplier (starts at 1x base speed)

    private bool accelerating = true;

    public static event System.Action OnEnemyDied;

    protected override void Start()
    {
        base.Start(); // Call the base class Start() to ensure player is assigned
        InitializeRuntimeData(); // Initialize runtime data for this specific enemy
        EnsureNavMeshPosition(); // Ensure the enemy is on the NavMesh before doing anything
    }

    private void EnsureNavMeshPosition()
    {
        if (agent != null && !agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                agent.Warp(hit.position);
                Debug.Log("EnemyBatAI: Position adjusted to NavMesh.");
            }
            else
            {
                Debug.LogWarning("EnemyBatAI: Could not find a valid NavMesh position.");
            }
        }
    }

    private void Update()
    {
        if (player == null || !agent.enabled) return;

        // If the bat is accelerating, increase its speed
        if (accelerating)
        {
            Accelerate();
        }

        // Update the agent's destination to the player's position
        agent.SetDestination(player.position);

        // Ensure the agent's speed is updated with the current multiplier
        agent.speed = baseData.speed * currentSpeedMultiplier;

        // Keep updating the agent's path, avoid stopping, and force continuous movement
        agent.isStopped = false;
    }

    private void Accelerate()
    {
        if (currentSpeedMultiplier < maxSpeedMultiplier)
        {
            currentSpeedMultiplier += accelerationRate * Time.deltaTime; // Gradually increase the speed
        }
        else
        {
            currentSpeedMultiplier = maxSpeedMultiplier; // Cap at the max speed multiplier
            accelerating = false; // Stop accelerating once max speed is reached
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(runtimeData.power);
                ApplyBounceBack(other.transform);
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        // Handle collisions with other enemies and player
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Ignore collision with other enemies
            return;
        }

        if (collision.gameObject.CompareTag("Player"))
        {
            // Bounce back from the player
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 away = (transform.position - collision.transform.position).normalized;
                away.y = 0f;
                rb.AddForce(away * 2f, ForceMode.Force); // Bounce back force
            }
        }
    }

    public override void TakeDamage(int dmg, float knockback, Vector3 sourcePos, float critChance, float critMulti)
    {
        // Apply normal damage and knockback behavior
        base.TakeDamage(dmg, knockback, sourcePos, critChance, critMulti);

        if (runtimeData.health <= 0)
        {
            OnEnemyDied?.Invoke();
        }
    }
}
