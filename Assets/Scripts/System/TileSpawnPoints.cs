using System.Collections.Generic;
using UnityEngine;

public class TileSpawnPoints : MonoBehaviour
{
    public List<Transform> spawnPoints = new();

    private List<GameObject> activeEnemies = new();

    private void Awake()
    {
        // Auto-detect spawn points by tag
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            if (child.CompareTag("SpawnPoint"))
            {
                spawnPoints.Add(child);
            }
        }

        //Debug.Log($"[TileSpawnPoints] {gameObject.name} has {spawnPoints.Count} spawn points");
    }

    public List<Transform> GetSpawnPoints() => spawnPoints;

    public void RegisterEnemy(GameObject enemy) => activeEnemies.Add(enemy);

    public void ClearEnemies()
    {
        foreach (var enemy in activeEnemies)
            if (enemy != null)
                enemy.SetActive(false);
        activeEnemies.Clear();
    }

    public Transform GetRandomSpawnPoint()
    {
        if (spawnPoints.Count == 0) return null;
        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }
}
