using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnableEnemy
    {
        public string enemyType;
        public string poolTag;
        public float spawnWeight = 1f;
        public float minSpawnTime = 0f;
    }

    private bool tilesReady = false;
    private Transform player;

    public List<SpawnableEnemy> spawnableEnemies;

    [Header("Spawn Settings")]
    public float spawnRadius = 2f;

    [Header("Spawn Timing")]
    public AnimationCurve spawnCurve;
    public float maxSpawnTime = 10f;
    private float spawnTimer = 0f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f);
        player = GameObject.FindWithTag("Player")?.transform;

        if (player == null)
            Debug.LogWarning("EnemySpawnManager: Could not find player!");

        tilesReady = true;
    }

    private void Update()
    {
        if (!tilesReady || player == null || !ObjectPooler.Instance.IsInitialized) return;

        spawnTimer += Time.deltaTime;
        float minutesElapsed = Time.timeSinceLevelLoad / 60f;
        float currentInterval = spawnCurve.Evaluate(Mathf.Clamp(minutesElapsed, 0f, maxSpawnTime));

        if (spawnTimer >= currentInterval)
        {
            SpawnEnemy(minutesElapsed);
            spawnTimer = 0f;
        }
    }

    private void SpawnEnemy(float minutesElapsed)
    {
        if (spawnableEnemies.Count == 0) return;

        List<SpawnableEnemy> eligible = spawnableEnemies.FindAll(e => minutesElapsed >= e.minSpawnTime);
        if (eligible.Count == 0) return;

        SpawnableEnemy chosen = GetWeightedRandomEnemy(eligible);

        GameObject[] pointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
        List<Transform> spawnPoints = new();

        foreach (GameObject obj in pointObjects)
        {
            spawnPoints.Add(obj.transform);
        }

        Debug.Log($"[EnemySpawnManager] Total valid tagged spawn points: {spawnPoints.Count}");
        if (spawnPoints.Count == 0) return;

        spawnPoints.Sort((a, b) =>
            Vector3.Distance(a.position, player.position).CompareTo(Vector3.Distance(b.position, player.position)));

        int count = Mathf.Min(5, spawnPoints.Count);
        for (int i = 0; i < count; i++)
        {
            float dist = Vector3.Distance(spawnPoints[i].position, player.position);
            //Debug.Log($"[SpawnPoint] #{i}: {spawnPoints[i].name} at {spawnPoints[i].position}, distance = {dist:F2}");
            Debug.DrawLine(player.position, spawnPoints[i].position, Color.yellow, 2f);
        }

        Transform basePoint = spawnPoints[Random.Range(0, count)];
        //Debug.Log($"[EnemySpawn] Chosen basePoint: {basePoint.name} at {basePoint.position}");

        Vector3 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = basePoint.position + new Vector3(offset.x, 0f, offset.y);

        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            spawnPos = hit.position;
        }
        else
        {
            Debug.LogWarning($"[EnemySpawn] Failed to find NavMesh near spawnPos: {spawnPos} (from {basePoint.name})");
            return;
        }

        GameObject enemy = ObjectPooler.Instance.SpawnFromPool(chosen.poolTag, spawnPos, Quaternion.identity);
        if (enemy != null)
        {
            enemy.SetActive(true);
        }
    }

    private SpawnableEnemy GetWeightedRandomEnemy(List<SpawnableEnemy> options)
    {
        float totalWeight = 0f;
        foreach (var enemy in options)
            totalWeight += enemy.spawnWeight;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var enemy in options)
        {
            cumulative += enemy.spawnWeight;
            if (roll <= cumulative)
                return enemy;
        }

        return options[options.Count - 1];
    }
}
