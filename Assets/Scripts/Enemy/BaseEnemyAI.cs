using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class BaseEnemyAI : MonoBehaviour
{
    [Header("Shared Enemy Settings")]
    public EnemyData baseData;
    protected EnemyData runtimeData;

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

        yield return null;
        yield return null;

        if (agent.isOnNavMesh)
        {
            Transform player = GameObject.FindWithTag("Player")?.transform;
            if (player != null)
                agent.SetDestination(player.position);
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
}
