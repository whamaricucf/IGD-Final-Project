using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(100)] // Ensure ObjectPooler initializes AFTER NavMesh and Tiles
public class ObjectPooler : MonoBehaviour
{
    public bool IsInitialized { get; private set; } = false;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
        public bool allowGrowth = true;
    }

    public static ObjectPooler Instance;

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, GameObject> prefabLookup;
    private Dictionary<string, Transform> poolParents;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(DelayedInitializePools());
    }

    private IEnumerator DelayedInitializePools()
    {
        // Wait until NavMeshManager exists and is active
        while (FindObjectOfType<NavMeshSurface>() == null || !FindObjectOfType<NavMeshSurface>().isActiveAndEnabled)
            yield return null;

        // Wait until at least one tile is active
        while (FindObjectsOfType<TileSpawnPoints>().Length == 0)
            yield return null;

        // Wait another frame to allow NavMesh baking to complete
        yield return null;

        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        prefabLookup = new Dictionary<string, GameObject>();
        poolParents = new Dictionary<string, Transform>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // Create a parent GameObject for this pool
            GameObject parentObj = new GameObject($"Pool_{pool.tag}");
            SceneManager.MoveGameObjectToScene(parentObj, gameObject.scene);
            parentObj.transform.parent = this.transform;
            poolParents[pool.tag] = parentObj.transform;

            for (int i = 0; i < pool.size; i++)
            {
                Vector3 safeInitPosition = new Vector3(9999, 9999, 9999); // Far from game area
                GameObject obj = Instantiate(pool.prefab, safeInitPosition, Quaternion.identity, poolParents[pool.tag]);

                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
            prefabLookup.Add(pool.tag, pool.prefab);
        }
        // Optional: extra delay to let initial NavMesh agent cache finish
        yield return new WaitForSeconds(0.25f);

        IsInitialized = true;
    }


    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        Queue<GameObject> poolQueue = poolDictionary[tag];
        GameObject objToSpawn;

        if (poolQueue.Count > 0 && !poolQueue.Peek().activeInHierarchy)
        {
            objToSpawn = poolQueue.Dequeue();
        }
        else if (prefabLookup.ContainsKey(tag))
        {
            Pool pool = pools.Find(p => p.tag == tag);
            if (pool != null && pool.allowGrowth)
            {
                objToSpawn = Instantiate(prefabLookup[tag], poolParents[tag]);
            }
            else
            {
                Debug.LogWarning($"No available objects in pool [{tag}] and resizing is disabled.");
                return null;
            }
        }
        else
        {
            Debug.LogWarning($"No prefab found for tag {tag}.");
            return null;
        }

        objToSpawn.transform.SetPositionAndRotation(position, rotation);
        objToSpawn.SetActive(true);
        return objToSpawn;
    }

    public void ReturnToPool(string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Trying to return to unknown pool with tag {tag}");
            Destroy(obj);
            return;
        }

        obj.SetActive(false);
        obj.transform.parent = poolParents[tag];
        poolDictionary[tag].Enqueue(obj);
    }
}
