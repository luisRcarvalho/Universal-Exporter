using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RunnerSpawnController : MonoBehaviour
{
    public static RunnerSpawnController Instance { get; private set; }

    [Serializable]
    public struct LaneSetup
    {
        public LanePosition laneIdentifier;
        public Transform spawnPointTransform;
    }
    
    [Header("Data")]
    [SerializeField] private RunnerGameSettingsSO gameSettings;
    
    [Header("Scene Setup")]
    [SerializeField] private List<LaneSetup> lanesConfiguration;
    
    [Header("References")]
    [SerializeField] private ObstacleSpawner obstacleSpawner;
    [SerializeField] private CoinSpawner coinSpawner;
    
    private bool _isRunning;
    private float _nextSpawnTime;
    private Dictionary<LanePosition, Transform> _laneMap;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _laneMap = new Dictionary<LanePosition, Transform>();
        foreach (var lane in lanesConfiguration)
        {
            if (lane.spawnPointTransform != null && !_laneMap.ContainsKey(lane.laneIdentifier))
                _laneMap.Add(lane.laneIdentifier, lane.spawnPointTransform);
        }
    }

    private void Update()
    {
        if (!_isRunning || !GameManager.Instance.HasGameStarted) return;

        if (Time.time >= _nextSpawnTime)
        {
            SpawnOne();
            _nextSpawnTime = Time.time + Random.Range(gameSettings.MinSpawnDelay, gameSettings.MaxSpawnDelay);
        }
    }

    public void StartSpawning()
    {
        _isRunning = true;
        _nextSpawnTime = Time.time + Random.Range(gameSettings.MinSpawnDelay, gameSettings.MaxSpawnDelay);
    }

    public void StopSpawning() => _isRunning = false;

    public Transform GetLaneTransform(LanePosition lane)
    {
        if (_laneMap.TryGetValue(lane, out Transform t)) return t;
        return lanesConfiguration.Count > 0 ? lanesConfiguration[0].spawnPointTransform : null;
    }
    
    public List<LanePosition> GetAllAvailableLanes() => new List<LanePosition>(_laneMap.Keys);

    public void ClearAllSpawns()
    {
        if (obstacleSpawner != null) obstacleSpawner.ClearAll();
        if (coinSpawner != null) coinSpawner.ClearAll();
    }

    private void SpawnOne()
    {
        if (Random.value < gameSettings.CoinChance) coinSpawner.Spawn();
        else obstacleSpawner.Spawn();
    }
}