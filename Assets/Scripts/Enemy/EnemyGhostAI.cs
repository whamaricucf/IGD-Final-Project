using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyGhostAI : BaseEnemyAI, IDamageable
{
    [Header("Ghost Settings")]
    public bool isMiniGhost = false;
    private bool hasSplit = false;

    [Header("Mini Ghost Settings")]
    public GameObject miniGhostPrefab;
    public int numberOfMiniGhosts = 2;
    public float splitHealthThreshold = 50f;

    public static event System.Action OnEnemyDied;

    protected override void Start()
    {
        base.Start(); // Call the base class Start() to ensure player is assigned
        InitializeRuntimeData(); // Initialize runtime data for this specific enemy
    }

    private void Update()
    {
        if (player == null || agent == null) return;
        if (!agent.enabled || recentlyBounced) return;

        agent.SetDestination(player.position);

        if (needsDestinationReset)
        {
            needsDestinationReset = false;
        }
    }



    private void OnEnable()
    {
        hasSplit = false;
        isMiniGhost = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(runtimeData.power);

                // ✨ Always use ApplyBounceBack, even for mini-ghosts!
                ApplyBounceBack(other.transform);
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
                Vector3 pushDir = (transform.position - collision.transform.position).normalized;
                pushDir.y = 0f;
                rb.AddForce(pushDir * 2f, ForceMode.Force);
            }
        }
    }

    private void SplitIntoMiniGhosts()
    {
        hasSplit = true;

        for (int i = 0; i < numberOfMiniGhosts; i++)
        {
            GameObject miniGhost = ObjectPooler.Instance.SpawnFromPool("MiniGhost", transform.position, Quaternion.identity);
            if (miniGhost != null && miniGhost.TryGetComponent(out EnemyGhostAI ghostAI))
            {
                ghostAI.InitializeAsMini(runtimeData);
                ghostAI.player = this.player;
            }
        }

        ObjectPooler.Instance.ReturnToPool("Ghost", gameObject);
    }


    public void InitializeAsMini(EnemyData parentData)
    {
        isMiniGhost = true;

        runtimeData = ScriptableObject.CreateInstance<EnemyData>();
        runtimeData.speed = 5f + UnityEngine.Random.Range(-0.5f, 0.5f);
        runtimeData.health = 15 + UnityEngine.Random.Range(-5, 5);
        runtimeData.power = parentData.power;
        runtimeData.knockback = parentData.knockback;
        runtimeData.experience = Mathf.CeilToInt(parentData.experience / 3f);

        agent = GetComponent<NavMeshAgent>();
        agent.speed = runtimeData.speed;

        if (player != null)
            agent.SetDestination(player.position);
    }


    public override void TakeDamage(int dmg, float knockback, Vector3 sourcePos, float critChance, float critMulti, bool disableAgent)
    {
        base.TakeDamage(dmg, knockback, sourcePos, critChance, critMulti, disableAgent);

        if (!hasSplit && !isMiniGhost && runtimeData.health <= splitHealthThreshold && runtimeData.health > 0)
        {
            // Split into mini-ghosts instead of dying
            SplitIntoMiniGhosts();
        }
        else if (runtimeData.health <= 0)
        {
            // Only truly die if health is <= 0 after damage and no split
            OnEnemyDied?.Invoke();
        }
    }
}
