using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct SpawnerSettings
{
    public float spawnDistance;
    public float cleanupDistance;
}
public abstract class Spawner : MonoBehaviour
{
    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();
    private readonly List<GameObject> _activeObjects = new List<GameObject>();

    public abstract bool Spawn();

    protected GameObject SpawnFromPool(GameObject prefab, Vector3 position, float cleanupDistance)
    {
        if (!_pools.ContainsKey(prefab))
        {
            _pools[prefab] = new Queue<GameObject>();
        }

        GameObject obj;
        if (_pools[prefab].Count > 0)
        {
            obj = _pools[prefab].Dequeue();
            obj.transform.position = position;
            obj.SetActive(true);
            
            foreach (Transform child in obj.transform)
            {
                child.gameObject.SetActive(true);
            }
        }
        else
        {
            obj = Instantiate(prefab, position, Quaternion.identity, transform);
            
            if (!obj.TryGetComponent<RunnerMovable>(out var movable))
            {
                movable = obj.AddComponent<RunnerMovable>();
            }
        }
        if (obj.TryGetComponent<RunnerMovable>(out var runnerMovable))
        {
            runnerMovable.Initialize((returnedObj) => ReturnToPool(prefab, returnedObj), cleanupDistance);
        }

        _activeObjects.Add(obj);
        return obj;
    }

    private void ReturnToPool(GameObject prefab, GameObject obj)
    {
        obj.SetActive(false);
        _activeObjects.Remove(obj);
        _pools[prefab].Enqueue(obj);
    }

    public void ClearAll()
    {
        var objectsToDespawn = new List<GameObject>(_activeObjects);
        foreach (var obj in objectsToDespawn)
        {
            if (obj.TryGetComponent<RunnerMovable>(out var movable))
            {
                movable.Despawn();
            }
        }
    }
}
