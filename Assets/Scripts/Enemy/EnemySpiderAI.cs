using UnityEngine;
using UnityEngine.AI;

public class EnemySpiderAI : BaseEnemyAI, IDamageable
{
    [Header("Web Settings")]
    public GameObject webPrefab;
    public float webDropInterval = 5f;
    private float webDropTimer = 0f;

    public static event System.Action OnEnemyDied;

    protected override void Start()
    {
        base.Start(); // Call the base class Start() to ensure player is assigned
        InitializeRuntimeData(); // Initialize runtime data for this specific enemy
    }

    private void Update()
    {
        if (player == null || !agent.enabled) return;

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
            Vector3 spawnPos = transform.position;
            spawnPos.y -= 0.5f;
            ObjectPooler.Instance.SpawnFromPool("SpiderWeb", spawnPos, Quaternion.identity);
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
                ApplyBounceBack(other.transform); // From BaseEnemyAI
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

    public override void TakeDamage(int dmg, float knockback, Vector3 sourcePos, float critChance, float critMulti, bool disableAgent)
    {
        base.TakeDamage(dmg, knockback, sourcePos, critChance, critMulti, disableAgent);

        if (runtimeData.health <= 0)
        {
            OnEnemyDied?.Invoke();
        }
    }

}
