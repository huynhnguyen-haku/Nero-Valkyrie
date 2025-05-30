using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;

    [SerializeField] private int poolSize = 10;

    // Maps prefab to its pool of inactive objects
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary =
        new Dictionary<GameObject, Queue<GameObject>>();

    [Header("To Initialize")]
    [SerializeField] private GameObject weaponPickup;
    [SerializeField] private GameObject ammoPickup;
    [SerializeField] private GameObject healthPickUp;

    #region Unity Methods

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        InitializeNewPool(weaponPickup);
        InitializeNewPool(ammoPickup);
        InitializeNewPool(healthPickUp);
    }

    #endregion

    #region Pool Access

    // Get an object from the pool, or create if needed
    public GameObject GetObject(GameObject prefab, Transform target)
    {
        if (!poolDictionary.ContainsKey(prefab))
            InitializeNewPool(prefab);

        if (poolDictionary[prefab].Count == 0)
            CreateNewObject(prefab); // Expand pool if empty

        GameObject objectToGet = poolDictionary[prefab].Dequeue();

        objectToGet.transform.position = target.position;
        objectToGet.transform.parent = null;
        objectToGet.SetActive(true);

        return objectToGet;
    }

    // Return an object to the pool after a delay
    public void ReturnObject(GameObject objectToReturn, float delay = .001f)
    {
        StartCoroutine(DelayReturn(delay, objectToReturn));
    }

    private IEnumerator DelayReturn(float delay, GameObject objectToReturn)
    {
        yield return new WaitForSeconds(delay);
        ReturnToPool(objectToReturn);
    }

    // Deactivate and enqueue the object
    private void ReturnToPool(GameObject objectToReturn)
    {
        GameObject originalPrefab = objectToReturn.GetComponent<PooledObject>().originalPrefab;

        objectToReturn.SetActive(false);
        objectToReturn.transform.parent = transform;

        poolDictionary[originalPrefab].Enqueue(objectToReturn);
    }

    #endregion

    #region Pool Initialization

    // Create a new pool for a prefab
    private void InitializeNewPool(GameObject prefab)
    {
        poolDictionary[prefab] = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            CreateNewObject(prefab);
        }
    }

    // Instantiate and add a new object to the pool
    private void CreateNewObject(GameObject prefab)
    {
        GameObject newObject = Instantiate(prefab, transform);
        newObject.AddComponent<PooledObject>().originalPrefab = prefab;
        newObject.SetActive(false);

        poolDictionary[prefab].Enqueue(newObject);
    }

    #endregion
}
