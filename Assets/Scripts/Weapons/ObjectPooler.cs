using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
        public bool allowGrowth = true; // Optional: allow pool to grow if empty
    }

    public static ObjectPooler Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, GameObject> prefabLookup;

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        prefabLookup = new Dictionary<string, GameObject>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
            prefabLookup.Add(pool.tag, pool.prefab);
        }
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
            // Fallback: instantiate a new one if allowed
            Pool pool = pools.Find(p => p.tag == tag);
            if (pool != null && pool.allowGrowth)
            {
                objToSpawn = Instantiate(prefabLookup[tag]);
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

        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;
        objToSpawn.SetActive(true);

        return objToSpawn;
    }

    public void ReturnToPool(string tag, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Trying to return to unknown pool with tag {tag}");
            Destroy(obj); // Fallback to destroy if pool not found
            return;
        }

        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);
    }
}
