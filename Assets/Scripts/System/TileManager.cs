using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public Transform player;
    public List<GameObject> tilePrefabs;
    public int tileSize = 100;
    public int tilesVisibleInEachDirection = 1; // 1 = 3x3 grid

    [Header("World Bounds")]
    public int minX = -100;
    public int maxX = 400;
    public int minZ = -100;
    public int maxZ = 400;

    private Vector2Int currentPlayerTile;
    private Dictionary<Vector2Int, GameObject> activeTiles = new();
    private Queue<GameObject> tilePool = new();

    private void Start()
    {
        currentPlayerTile = GetTileCoord(player.position);
        UpdateTiles(forceRefresh: true);
    }

    private void Update()
    {
        Vector2Int newPlayerTile = GetTileCoord(player.position);

        if (newPlayerTile != currentPlayerTile)
        {
            Vector3 newPosition = player.position;

            // Wrap X axis
            if (player.position.x < minX)
                newPosition.x += (maxX - minX);
            else if (player.position.x >= maxX)
                newPosition.x -= (maxX - minX);

            // Wrap Z axis
            if (player.position.z < minZ)
                newPosition.z += (maxZ - minZ);
            else if (player.position.z >= maxZ)
                newPosition.z -= (maxZ - minZ);

            if (newPosition != player.position)
            {
                player.position = newPosition;
            }

            currentPlayerTile = GetTileCoord(player.position);
            UpdateTiles(forceRefresh: false);
        }
    }

    private Vector2Int GetTileCoord(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / tileSize);
        int z = Mathf.FloorToInt(position.z / tileSize);
        return new Vector2Int(x, z);
    }

    private void UpdateTiles(bool forceRefresh)
    {
        HashSet<Vector2Int> neededTiles = new();

        // Determine which tiles should be active
        for (int dx = -tilesVisibleInEachDirection; dx <= tilesVisibleInEachDirection; dx++)
        {
            for (int dz = -tilesVisibleInEachDirection; dz <= tilesVisibleInEachDirection; dz++)
            {
                Vector2Int tileCoord = currentPlayerTile + new Vector2Int(dx, dz);
                neededTiles.Add(tileCoord);

                if (!activeTiles.ContainsKey(tileCoord))
                {
                    GameObject tile = GetTileFromPool();
                    tile.transform.position = new Vector3(tileCoord.x * tileSize, 0, tileCoord.y * tileSize);
                    tile.SetActive(true);
                    activeTiles[tileCoord] = tile;
                }
            }
        }

        // Remove and recycle unneeded tiles
        List<Vector2Int> tilesToRemove = new();
        foreach (var kvp in activeTiles)
        {
            if (!neededTiles.Contains(kvp.Key))
            {
                GameObject tile = kvp.Value;

                TileSpawnPoints tsp = tile.GetComponent<TileSpawnPoints>();
                if (tsp != null) tsp.ClearEnemies();

                tile.SetActive(false);
                tilePool.Enqueue(tile);
                tilesToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in tilesToRemove)
        {
            activeTiles.Remove(key);
        }

        FindObjectOfType<NavMeshManager>()?.BakeNavMesh();
    }

    private GameObject GetTileFromPool()
    {
        if (tilePool.Count > 0)
        {
            return tilePool.Dequeue();
        }
        else
        {
            int index = Random.Range(0, tilePrefabs.Count);
            return Instantiate(tilePrefabs[index]);
        }
    }
}
