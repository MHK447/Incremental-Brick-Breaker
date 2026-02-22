using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public interface IPool<T> where T : Component
{
    T Get();
    void Return(T obj);
}

public class ObjectPool<T> : IPool<T> where T : MonoBehaviour
{
    private Transform parent;
    private AssetReference assetRef = null;
    private Queue<T> poolQueue = new Queue<T>();

    private HashSet<T> spawnedObjects = new();

    public void Init(AssetReference _ref, Transform _parent, int size)
    {
        parent = _parent;
        assetRef = _ref;
        Create(size);
    }

    private void Create(int size)
    {
        if (assetRef != null)
        {
            for (var i = 0; i < size; ++i)
            {
                var handle = assetRef.InstantiateAsync(parent, false);
                handle.WaitForCompletion();
                var obj = handle.Result.GetComponent<T>();
                poolQueue.Enqueue(obj);
                obj.gameObject.SetActive(false);
            }
        }
    }

    public T Get()
    {
        if (poolQueue.Count < 1)
        {
            Create(1);
        }

        var obj = poolQueue.Dequeue();
        obj.gameObject.SetActive(true);
        spawnedObjects.Add(obj);
        return obj;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        poolQueue.Enqueue(obj);
        spawnedObjects.Remove(obj);
    }

    public void Clear()
    {
        foreach (var obj in spawnedObjects)
        {
            poolQueue.Enqueue(obj);
        }

        foreach (var obj in poolQueue)
            if (obj != null && obj.gameObject != null)
                if (!Addressables.ReleaseInstance(obj.gameObject))
                    GameObject.Destroy(obj.gameObject);

        spawnedObjects.Clear();
        poolQueue.Clear();
    }
}

public class PrefabPool<T> : IPool<T> where T : Component
{
    private GameObject prefab;
    private Queue<T> poolQueue = new Queue<T>();
    private Transform parent;
    private HashSet<T> spawnedItems = new();

    public void Init(GameObject prefab, Transform _parent, int size)
    {
        parent = _parent;
        this.prefab = prefab;
        Create(size);
    }

    private void Create(int size)
    {
        if (prefab == null) return;
        for (int i = 0; i < size; i++)
        {
            var newObj = UnityEngine.Object.Instantiate(prefab);
            if (parent != null)
                newObj.transform.SetParent(parent, false);

            var component = newObj.GetComponent<T>();
            spawnedItems.Add(component);
            if (component != null)
            {
                newObj.SetActive(false);
                poolQueue.Enqueue(component);
            }
            else
            {
                UnityEngine.Object.Destroy(newObj);
            }
        }
    }

    public T Get()
    {
        if (poolQueue.Count < 1)
        {
            Create(1);
        }

        var obj = poolQueue.Dequeue();
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        poolQueue.Enqueue(obj);
    }

    public void Clear()
    {
        foreach (var item in spawnedItems) poolQueue.Enqueue(item);
        foreach (var item in poolQueue) Object.Destroy(item.gameObject);
        spawnedItems.Clear();
        poolQueue.Clear();
    }
}

public class PrefabPool
{
    private GameObject prefab;
    private Queue<GameObject> poolQueue = new Queue<GameObject>();
    private Transform parent;

    public void Init(GameObject prefab, Transform _parent, int size)
    {
        parent = _parent;
        this.prefab = prefab;
        Create(size);
    }

    private void Create(int size)
    {
        if (prefab == null) return;
        for (int i = 0; i < size; i++)
        {
            var newObj = UnityEngine.Object.Instantiate(prefab);
            if (parent != null)
                newObj.transform.SetParent(parent, false);

            var component = newObj;
            if (component != null)
            {
                newObj.SetActive(false);
                poolQueue.Enqueue(component);
            }
            else
            {
                UnityEngine.Object.Destroy(newObj);
            }
        }
    }

    public GameObject Get()
    {
        if (poolQueue.Count < 1)
        {
            Create(1);
        }

        var obj = poolQueue.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        poolQueue.Enqueue(obj);
    }

    public void Clear()
    {
        foreach (var item in poolQueue) Object.Destroy(item);
        poolQueue.Clear();
    }
}
