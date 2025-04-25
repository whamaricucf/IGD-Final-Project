using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class BaseEnemyAI : MonoBehaviour
{
    [Header("Shared Enemy Settings")]
    public EnemyData baseData;
    protected EnemyData runtimeData;
    protected NavMeshAgent agent;
    public Transform player;

    private Rigidbody rb;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        if (agent == null)
        {
            Debug.LogError($"{name}: Missing NavMeshAgent!");
        }

        if (player == null)
        {
            player = GameObject.FindWithTag("Player")?.transform;
            Debug.Log($"Player assigned: {player != null}");
            if (player == null)
            {
                Debug.LogError($"{name}: Player reference is not assigned!");
            }
        }

        if (rb != null)
        {
            rb.isKinematic = true;
        }

        StartCoroutine(PlaceEnemyOnNavMesh());
        InitializeRuntimeData();
        InitializeEnemy();
    }

    private IEnumerator PlaceEnemyOnNavMesh()
    {
        yield return new WaitForEndOfFrame();

        if (agent != null && !agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
            }
            else
            {
                Debug.LogWarning($"{name}: Failed to find valid NavMesh position for spawn.");
            }
        }

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        agent.enabled = true;

        if (player != null)
        {
            agent.SetDestination(player.position);
        }
    }

    protected virtual void InitializeRuntimeData()
    {
        if (runtimeData != null) return;

        runtimeData = ScriptableObject.CreateInstance<EnemyData>();

        runtimeData.speed = baseData.speed + UnityEngine.Random.Range(-0.5f, 0.5f);
        runtimeData.health = baseData.health + UnityEngine.Random.Range(-10, 10);
        runtimeData.power = baseData.power;
        runtimeData.knockback = baseData.knockback;
        runtimeData.experience = baseData.experience;

        ScaleStatsOverTime();
    }

    protected virtual void ScaleStatsOverTime()
    {
        float minutes = Time.timeSinceLevelLoad / 60f;
        float healthMultiplier = 1f + (minutes * 0.2f);
        runtimeData.health = Mathf.RoundToInt(runtimeData.health * healthMultiplier);
        runtimeData.power += Mathf.FloorToInt(minutes * 0.5f);
    }

    protected virtual void DropExpGem()
    {
        GameObject gem = ObjectPooler.Instance.SpawnFromPool("Gem", transform.position, Quaternion.identity);
        if (gem.TryGetComponent(out GemPickup pickup))
        {
            pickup.expValue = runtimeData.experience;
        }
    }

    protected virtual void Die()
    {
        DropExpGem();
        Destroy(gameObject);
    }

    protected IEnumerator TemporarilyDisableAgent(NavMeshAgent agent)
    {
        agent.enabled = false;

        yield return new WaitForSeconds(0.3f);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }

        agent.enabled = true;

        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
            }
            else
            {
                Debug.LogWarning($"{name} failed to reposition to NavMesh!");
            }
        }

        yield return null;

        if (agent.isOnNavMesh)
        {
            if (player != null)
                agent.SetDestination(player.position);
        }
    }

    protected virtual void InitializeEnemy()
    {
        if (agent != null)
        {
            agent.enabled = false;
            StartCoroutine(StartAgentAfterDelay(agent, 0.5f));
        }

        if (player == null)
        {
            Debug.LogWarning("BaseEnemyAI: Player reference is not assigned!");
        }
    }

    private IEnumerator StartAgentAfterDelay(NavMeshAgent agent, float delay)
    {
        yield return new WaitForSeconds(delay);
        agent.enabled = true;

        if (player != null)
        {
            agent.SetDestination(player.position);
        }
        else
        {
            Debug.LogError("BaseEnemyAI: Player is still not assigned after delay.");
        }
    }

    protected void ApplyKnockback(Vector3 sourcePos, float knockback)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = (transform.position - sourcePos).normalized;
            direction.y = 0f;
            rb.AddForce(direction * knockback, ForceMode.Impulse);
        }
    }

    protected void ApplyBounceBack(Transform fromTarget)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = (transform.position - fromTarget.position).normalized;
            direction.y = 0f;
            rb.velocity = Vector3.zero;
            rb.AddForce(direction * (runtimeData.knockback * 2f), ForceMode.Impulse);
        }
    }

    public virtual void TakeDamage(int dmg, float knockback, Vector3 sourcePos, float critChance, float critMulti)
    {
        if (agent == null || player == null)
        {
            Debug.LogError($"{name}: Agent or Player is null when taking damage.");
            return;
        }

        float finalDamage = dmg;
        if (UnityEngine.Random.value < critChance)
            finalDamage *= critMulti;

        runtimeData.health -= Mathf.RoundToInt(finalDamage);

        ApplyKnockback(sourcePos, knockback);

        StartCoroutine(TemporarilyDisableAgent(agent));

        if (runtimeData.health <= 0)
        {
            Die();
        }
    }
}
