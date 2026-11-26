
using UnityEngine;
using System.Collections.Generic;

public class WeightedObjectSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnObject
    {
        public GameObject prefab;

        [Min(0.01f)]
        public float weight = 1f;
    }

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Random Objects")]
    public List<SpawnObject> objects = new List<SpawnObject>();

    [Header("Settings")]
    public bool spawnOnStart = true;
    public bool spawnAtEveryPoint = true;
    public bool randomRotation = false;

    [Header("Spawned Objects")]
    public List<GameObject> spawnedObjects = new List<GameObject>();

    private void Start()
    {
        if (spawnOnStart)
            SpawnObjects();
    }

    public void SpawnObjects()
    {
        ClearSpawnedObjects();

        if (spawnPoints.Length == 0 || objects.Count == 0)
            return;

        if (spawnAtEveryPoint)
        {
            foreach (Transform point in spawnPoints)
            {
                if (point == null) continue;

                SpawnAtPoint(point);
            }
        }
        else
        {
            Transform point = spawnPoints[
                Random.Range(0, spawnPoints.Length)
            ];

            if (point != null)
                SpawnAtPoint(point);
        }
    }

    private void SpawnAtPoint(Transform point)
    {
        GameObject prefab = GetRandomObject();

        if (prefab == null)
            return;

        Quaternion rotation = randomRotation
            ? Random.rotation
            : point.rotation;

        GameObject spawned = Instantiate(
            prefab,
            point.position,
            rotation,
            point
        );

        spawnedObjects.Add(spawned);
    }

    private GameObject GetRandomObject()
    {
        float totalWeight = 0f;

        foreach (SpawnObject obj in objects)
        {
            if (obj.prefab != null && obj.weight > 0)
                totalWeight += obj.weight;
        }

        if (totalWeight <= 0)
            return null;

        float randomWeight = Random.Range(0f, totalWeight);

        foreach (SpawnObject obj in objects)
        {
            if (obj.prefab == null || obj.weight <= 0)
                continue;

            randomWeight -= obj.weight;

            if (randomWeight <= 0)
                return obj.prefab;
        }

        return null;
    }

    public void ClearSpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }

        spawnedObjects.Clear();
    }
}
