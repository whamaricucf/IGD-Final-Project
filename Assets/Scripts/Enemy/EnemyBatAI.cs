using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBatAI : BaseEnemyAI, IDamageable
{
    public Transform player;
    private NavMeshAgent agent;

    [Header("Bombing Run Settings")]
    public float timeBeforeBombingRun = 60f;
    private bool bombingRunActive = false;
    private Vector3 bombingTarget;

    public static event System.Action OnEnemyDied;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = false;

        InitializeRuntimeData();
        agent.speed = runtimeData.speed;
    }

    private void Update()
    {
        if (player == null || !agent.enabled) return;

        if (bombingRunActive)
        {
            BombingRunUpdate();
        }
        else
        {
            agent.SetDestination(player.position);
        }

        if (Time.timeSinceLevelLoad > timeBeforeBombingRun && !bombingRunActive)
        {
            ActivateBombingRun();
        }
    }

    private void ActivateBombingRun()
    {
        bombingRunActive = true;
        agent.speed = runtimeData.speed + UnityEngine.Random.Range(10f, 15f);
        ChooseBombingTarget();
    }

    private void ChooseBombingTarget()
    {
        Vector3 direction = (transform.position - player.position).normalized;
        bombingTarget = player.position + direction * 30f;
        agent.SetDestination(bombingTarget);
    }

    private void BombingRunUpdate()
    {
        if (Vector3.Distance(transform.position, bombingTarget) < 2f)
        {
            ChooseBombingTarget();
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
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 away = (transform.position - collision.transform.position).normalized;
                away.y = 0f;
                rb.AddForce(away * 2f, ForceMode.Force);
            }
        }
    }

    public void TakeDamage(int dmg, float knockback, Vector3 sourcePos, float critChance, float critMulti)
    {
        float finalDamage = dmg;
        if (UnityEngine.Random.value < critChance)
            finalDamage *= critMulti;

        runtimeData.health -= Mathf.RoundToInt(finalDamage);

        ApplyKnockback(sourcePos, knockback);
        StartCoroutine(TemporarilyDisableAgent(agent));

        if (runtimeData.health <= 0)
        {
            OnEnemyDied?.Invoke();
            Die();
        }
    }
}
