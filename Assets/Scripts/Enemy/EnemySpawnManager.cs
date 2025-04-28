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
    public AnimationCurve spawnCurve; // Controls time between spawns
    public AnimationCurve spawnCountCurve; // 🆕 Controls how many enemies to spawn
    public float maxSpawnTime = 10f;
    private float spawnTimer = 0f;

    private List<Transform> allSpawnPoints = new();

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.2f);
        player = GameObject.FindWithTag("Player")?.transform;

        if (player == null)
            Debug.LogWarning("EnemySpawnManager: Could not find player!");

        foreach (var obj in GameObject.FindGameObjectsWithTag("SpawnPoint"))
            allSpawnPoints.Add(obj.transform);

        tilesReady = true;
    }

    private void Update()
    {
        if (!tilesReady || player == null || !ObjectPooler.Instance.IsInitialized) return;

        spawnTimer += Time.deltaTime;
        float secondsElapsed = Time.timeSinceLevelLoad;

        float currentInterval = spawnCurve.Evaluate(Mathf.Min(secondsElapsed, maxSpawnTime));
        currentInterval = Mathf.Clamp(currentInterval, 0.5f, 5f);

        if (spawnTimer >= currentInterval)
        {
            SpawnEnemies(secondsElapsed); // 🆕 Renamed to plural
            spawnTimer = 0f;
        }
    }

    private void SpawnEnemies(float secondsElapsed)
    {
        if (spawnableEnemies.Count == 0 || allSpawnPoints.Count == 0) return;

        List<SpawnableEnemy> eligible = spawnableEnemies.FindAll(e => secondsElapsed >= e.minSpawnTime);
        if (eligible.Count == 0) return;

        allSpawnPoints.Sort((a, b) => Vector3.Distance(a.position, player.position).CompareTo(Vector3.Distance(b.position, player.position)));
        int closestCount = Mathf.Min(5, allSpawnPoints.Count);

        // 🆕 How many enemies should we spawn this cycle?
        int enemiesToSpawn = Mathf.Max(1, Mathf.RoundToInt(spawnCountCurve.Evaluate(secondsElapsed)));

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnableEnemy chosen = GetWeightedRandomEnemy(eligible);

            Transform basePoint = allSpawnPoints[Random.Range(0, closestCount)];

            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = basePoint.position + new Vector3(offset.x, 0f, offset.y);

            if (!NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                Debug.LogWarning($"[EnemySpawn] Failed to find NavMesh near spawnPos: {spawnPos}");
                continue;
            }

            GameObject enemy = ObjectPooler.Instance.SpawnFromPool(chosen.poolTag, hit.position, Quaternion.identity);
            if (enemy != null)
            {
                enemy.SetActive(true);
                if (enemy.TryGetComponent(out BaseEnemyAI enemyAI))
                {
                    enemyAI.ForcePlaceOnNavMesh();
                }
            }
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
