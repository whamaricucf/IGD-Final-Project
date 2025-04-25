using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnableEnemy
    {
        public string enemyType;           // e.g. "Bat", "Ghost", "Spider"
        public string poolTag;             // Used by ObjectPooler (e.g. "Bat")
        public float spawnWeight = 1f;     // Controls relative spawn chance
        public float minSpawnTime = 0f;    // Time in minutes this enemy starts appearing
    }

    public List<SpawnableEnemy> spawnableEnemies;
    public Transform[] spawnPoints;

    [Header("Spawn Timing")]
    public AnimationCurve spawnCurve; // X: time (minutes), Y: interval
    public float maxSpawnTime = 10f;  // Max time considered in the curve
    private float spawnTimer = 0f;

    private void Update()
    {
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
        if (spawnPoints.Length == 0 || spawnableEnemies.Count == 0) return;

        // Filter eligible enemies
        List<SpawnableEnemy> eligible = new List<SpawnableEnemy>();
        foreach (var spawnable in spawnableEnemies)
        {
            if (minutesElapsed >= spawnable.minSpawnTime)
                eligible.Add(spawnable);
        }

        if (eligible.Count == 0) return;

        // Weighted random selection
        SpawnableEnemy chosen = GetWeightedRandomEnemy(eligible);
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject enemy = ObjectPooler.Instance.SpawnFromPool(chosen.poolTag, spawnPoint.position, Quaternion.identity);
        enemy.SetActive(true);
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

        return options[options.Count - 1]; // Fallback
    }
}
