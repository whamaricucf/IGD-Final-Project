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
        if (player == null || !agent.enabled) return;

        if (!isMiniGhost && !hasSplit && runtimeData.health <= splitHealthThreshold)
        {
            SplitIntoMiniGhosts();
            return;
        }

        agent.SetDestination(player.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(runtimeData.power);
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
            GameObject miniGhost = Instantiate(miniGhostPrefab, transform.position, Quaternion.identity);
            if (miniGhost.TryGetComponent(out EnemyGhostAI ghostAI))
            {
                ghostAI.InitializeAsMini(runtimeData);
                ghostAI.player = this.player;
            }
        }

        Destroy(gameObject);
    }

    public void InitializeAsMini(EnemyData parentData)
    {
        isMiniGhost = true;

        runtimeData = ScriptableObject.CreateInstance<EnemyData>();
        runtimeData.speed = 5f + UnityEngine.Random.Range(-0.5f, 0.5f);
        runtimeData.health = 50 + UnityEngine.Random.Range(-5, 5);
        runtimeData.power = parentData.power;
        runtimeData.knockback = parentData.knockback;
        runtimeData.experience = Mathf.CeilToInt(parentData.experience / 3f);

        agent = GetComponent<NavMeshAgent>();
        agent.speed = runtimeData.speed;
    }

    public override void TakeDamage(int dmg, float knockback, Vector3 sourcePos, float critChance, float critMulti)
    {
        base.TakeDamage(dmg, knockback, sourcePos, critChance, critMulti);

        if (runtimeData.health <= 0)
        {
            OnEnemyDied?.Invoke();
        }
    }
}
