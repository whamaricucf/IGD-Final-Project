using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public Transform player;
    public List<GameObject> tilePrefabs;
    public int tileSize = 100;
    public int tilesVisibleInEachDirection = 1; // 1 = 3x3 grid

    private Vector2Int currentPlayerTile;
    private Dictionary<Vector2Int, GameObject> activeTiles = new();
    private Queue<GameObject> tilePool = new();
    
    private void Start()
    {
        UpdateTiles(forceRefresh: true);
    }

    private void Update()
    {
        Vector2Int newPlayerTile = GetTileCoord(player.position);
        if (newPlayerTile != currentPlayerTile)
        {
            currentPlayerTile = newPlayerTile;
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
                tile.SetActive(false);
                tilePool.Enqueue(tile);
                tilesToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in tilesToRemove)
        {
            activeTiles.Remove(key);
        }
    }

    private GameObject GetTileFromPool()
    {
        if (tilePool.Count > 0)
        {
            return tilePool.Dequeue();
        }
        else
        {
            // Pick a random tile variant from the list
            int index = Random.Range(0, tilePrefabs.Count);
            return Instantiate(tilePrefabs[index]);
        }
    }

}
