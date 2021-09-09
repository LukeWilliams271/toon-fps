using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    // attempted singleton MIGHT RETURN ERRORS WHEN TRYING TO SPAWN AFTER CHANGING SCENES
    // when in doubt, get a referance to this component on the GAMEMASTER and use that instead
    public static ObjectPooler Instance;
    private void Awake()
    {
        Instance = this;  
    }


    [System.Serializable]
    public class Pool
    {
        public string tag;   // name of the pool itself
        public GameObject prefab;    // prefab of the objects in the pool
        public int size;     // max number before the pool begins to reuse objects
    }

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    // Start is called before the first frame update
    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach(Pool p in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            GameObject parent = new GameObject(p.tag + " Pool Parent");
            parent.transform.SetParent(gameObject.transform);
            for (int i = 0; i < p.size; i++)
            {
                GameObject obj = Instantiate(p.prefab);
                obj.transform.SetParent(parent.transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(p.tag, objectPool);
        }
    }

    GameObject objToSpawn;
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag (" + tag + ") not found.");
            return null;
        }
        objToSpawn = poolDictionary[tag].Dequeue();
        objToSpawn.SetActive(true);
        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;

        if(tag != "Decal") objToSpawn.GetComponent<IPooledObject>().OnSpawn(0.001f);
        poolDictionary[tag].Enqueue(objToSpawn);
        return objToSpawn;
    }
    private void RunObjOnSpawn()
    {
        //if (objToSpawn.GetComponent<IPooledObject>() != null /*&& objToSpawn.GetComponent<Projectile>() == null*/) objToSpawn.GetComponent<IPooledObject>().OnSpawn();
    }
    public void RemoveFromWorld(GameObject obj)
    {
        obj.transform.position = new Vector3(0, 0, 0);
        transform.rotation = Quaternion.identity;
        obj.SetActive(false);
    }
}
