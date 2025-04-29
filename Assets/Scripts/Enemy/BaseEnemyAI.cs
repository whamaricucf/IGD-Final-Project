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
    protected bool needsDestinationReset = false;
    protected bool recentlyBounced = false;


    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        if (agent == null)
            Debug.LogError($"{name}: Missing NavMeshAgent!");

        if (player == null)
        {
            player = GameObject.FindWithTag("Player")?.transform;
            if (player == null)
                Debug.LogError($"{name}: Player reference is not assigned!");
        }

        if (rb != null)
            rb.isKinematic = true;

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
                transform.position = hit.position;
            else
                Debug.LogWarning($"{name}: Failed to find valid NavMesh position for spawn.");
        }

        if (rb != null)
            rb.isKinematic = false;

        agent.enabled = true;

        if (player != null)
            agent.SetDestination(player.position);
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
            pickup.expValue = runtimeData.experience;
    }

    protected virtual void Die()
    {
        DropExpGem();
        AudioManager.Instance.PlayEnemyDeath();
        if (TryGetComponent<BaseEnemyAI>(out var enemy))
        {
            string poolTag = GetEnemyPoolTag();
            ObjectPooler.Instance.ReturnToPool(poolTag, gameObject);
        }
        else
            Destroy(gameObject);
    }

    protected virtual string GetEnemyPoolTag()
    {
        if (GetComponent<EnemyGhostAI>() != null) return "Ghost";
        if (GetComponent<EnemyBatAI>() != null) return "Bat";
        if (GetComponent<EnemySpiderAI>() != null) return "Spider";
        return "Enemy";
    }

    protected void ApplyKnockback(Vector3 sourcePos, float knockback)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = (transform.position - sourcePos).normalized;
            direction.y = 0f;
            rb.velocity = Vector3.zero;
            rb.AddForce(direction * knockback, ForceMode.Impulse);
        }

        recentlyBounced = true;
        StartCoroutine(RecoverAfterKnockback());
    }

    protected IEnumerator RecoverAfterKnockback()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        float timer = 0f;

        // Wait until slow enough or timeout
        while (rb != null && rb.velocity.magnitude > 0.1f && timer < 0.4f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero;         // ✨ hard reset
            rb.angularVelocity = Vector3.zero;  // ✨ hard reset
        }

        if (agent != null)
        {
            agent.enabled = true;

            if (!agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                    transform.position = hit.position;
            }

            needsDestinationReset = true;
            recentlyBounced = false;
        }
    }


    public virtual void TakeDamage(int dmg, float knockback, Vector3 sourcePos, float critChance, float critMulti, bool disableAgent = true)
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

        if (disableAgent)
            StartCoroutine(TemporarilyDisableAgent(agent));
        else
            StartCoroutine(TemporarilyDisableAgentBrief(agent));

        if (runtimeData.health <= 0)
            Die();
    }

    protected IEnumerator TemporarilyDisableAgent(NavMeshAgent agent)
    {
        agent.enabled = false;

        yield return new WaitForSeconds(0.3f);

        if (rb != null)
            rb.velocity = Vector3.zero;

        agent.enabled = true;

        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                transform.position = hit.position;
        }

        if (player != null)
            agent.SetDestination(player.position);
    }

    protected IEnumerator TemporarilyDisableAgentBrief(NavMeshAgent agent)
    {
        agent.enabled = false;

        yield return new WaitForSeconds(0.1f);

        agent.enabled = true;

        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                transform.position = hit.position;
        }

        if (player != null)
            agent.SetDestination(player.position);
    }

    protected virtual void InitializeEnemy()
    {
        if (agent != null)
        {
            agent.enabled = false;
            StartCoroutine(StartAgentAfterDelay(agent, 0.5f));
        }

        if (player == null)
            Debug.LogWarning("BaseEnemyAI: Player reference is not assigned!");
    }

    private IEnumerator StartAgentAfterDelay(NavMeshAgent agent, float delay)
    {
        yield return new WaitForSeconds(delay);
        agent.enabled = true;

        if (player != null)
            agent.SetDestination(player.position);
    }

    protected virtual void ApplyBounceBack(Transform fromTarget)
    {
        if (agent != null)
            agent.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direction = (transform.position - fromTarget.position).normalized;
            direction.y = 0f;
            rb.isKinematic = false; // Enable physics
            rb.velocity = Vector3.zero;

            float knockbackForce = runtimeData.knockback * 2f;
            rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
        }

        recentlyBounced = true;
        StartCoroutine(ReenableAgentAfterBounce());
    }



    protected virtual IEnumerator ReenableAgentAfterBounce()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        // WAIT to allow bounce physics to apply
        yield return new WaitForSeconds(0.1f);

        float timer = 0f;

        // THEN wait until slow enough
        while (rb != null && rb.velocity.magnitude > 0.1f && timer < 0.5f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; // Freeze after physics settles
        }

        if (agent != null)
        {
            agent.enabled = true;

            if (!agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                    transform.position = hit.position;
            }

            needsDestinationReset = true;
            recentlyBounced = false;
        }
    }


    public void ForcePlaceOnNavMesh()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            return;

        if (!agent.isOnNavMesh)
        {
            transform.position += new Vector3(0, 2f, 0); // Raise slightly
            Rigidbody rb = GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.velocity = Vector3.down * 20f; // Forcefully push enemy down!
            }

            agent.enabled = false;
            StartCoroutine(EnableAgentAfterLanding());
        }
    }

    private IEnumerator EnableAgentAfterLanding()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        float timer = 0f;

        while (rb != null && rb.velocity.magnitude > 0.1f && timer < 0.5f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }

        if (agent != null)
            agent.enabled = true;

        if (player != null)
            agent.SetDestination(player.position);
    }
}
