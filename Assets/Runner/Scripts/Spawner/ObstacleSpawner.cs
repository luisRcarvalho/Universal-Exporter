using System.Linq;
using UnityEngine;

public class ObstacleSpawner : Spawner
{
    [SerializeField] private SpawnerSettings spawnerSettings;
    [SerializeField] private GameObject[] obstaclePrefabs; 

    public override bool Spawn()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return false;
        if (RunnerSpawnController.Instance == null) return false;

        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        ObstacleSpawnRestriction restriction = prefab.GetComponent<ObstacleSpawnRestriction>();
        LanePosition targetLane;

        if (restriction != null && restriction.allowedLanes.Count > 0)
        {
            var validLanes = restriction.allowedLanes.Intersect(RunnerSpawnController.Instance.GetAllAvailableLanes()).ToList();
            if (validLanes.Count == 0) return false;
            targetLane = validLanes[Random.Range(0, validLanes.Count)];
        }
        else
        {
            var allLanes = RunnerSpawnController.Instance.GetAllAvailableLanes();
            targetLane = allLanes[Random.Range(0, allLanes.Count)];
        }
        
        Transform spawnPoint = RunnerSpawnController.Instance.GetLaneTransform(targetLane);
        if (spawnPoint == null) return false;

        Vector3 pos = new Vector3(transform.position.x, spawnPoint.position.y, spawnerSettings.spawnDistance);
        
        SpawnFromPool(prefab, pos, spawnerSettings.cleanupDistance);

        return true;
    }
}