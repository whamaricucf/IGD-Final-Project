using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnableEnemy
    {
        public GameObject enemyPrefab;
        public string enemyType; // "Bat", "Ghost", "Spider"
    }

    public SpawnableEnemy[] allEnemies;
    public Transform[] spawnPoints;

    private float spawnTimer = 0f;
    public float spawnInterval = 2f; // Start with 2 seconds between spawns
    public float minSpawnInterval = 0.5f; // Fastest possible spawn rate
    public float spawnRateDecreaseSpeed = 0.05f; // How much faster spawns get per minute

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }

        // Decrease spawnInterval over time
        spawnInterval -= spawnRateDecreaseSpeed * Time.deltaTime;
        spawnInterval = Mathf.Max(spawnInterval, minSpawnInterval); // Clamp to min
    }

    private void SpawnEnemy()
    {
        if (spawnPoints.Length == 0) return;

        // Determine allowed enemies based on time
        SpawnableEnemy[] allowedEnemies = GetAllowedEnemies();

        if (allowedEnemies.Length == 0) return;

        // Pick random enemy
        int enemyIndex = Random.Range(0, allowedEnemies.Length);
        GameObject enemyToSpawn = allowedEnemies[enemyIndex].enemyPrefab;

        // Pick random spawn point
        int spawnPointIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[spawnPointIndex];

        Instantiate(enemyToSpawn, spawnPoint.position, Quaternion.identity);
    }

    private SpawnableEnemy[] GetAllowedEnemies()
    {
        float minutes = Time.timeSinceLevelLoad / 60f;

        if (minutes < 1)
        {
            // Minute 0–1: Bats only
            return FilterEnemiesByType("Bat");
        }
        else if (minutes < 2)
        {
            // Minute 1–2: Bats + Ghosts
            return FilterEnemiesByTypes("Bat", "Ghost");
        }
        else if (minutes < 3)
        {
            // Minute 2–3: Bats + Spiders
            return FilterEnemiesByTypes("Bat", "Spider");
        }
        else if (minutes < 4)
        {
            // Minute 3–4: Spiders + Ghosts
            return FilterEnemiesByTypes("Spider", "Ghost");
        }
        else
        {
            // Minute 4+: All 3
            return allEnemies;
        }
    }

    private SpawnableEnemy[] FilterEnemiesByType(string type)
    {
        return System.Array.FindAll(allEnemies, enemy => enemy.enemyType == type);
    }

    private SpawnableEnemy[] FilterEnemiesByTypes(string type1, string type2)
    {
        return System.Array.FindAll(allEnemies, enemy => enemy.enemyType == type1 || enemy.enemyType == type2);
    }
}