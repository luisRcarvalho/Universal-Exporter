using UnityEngine;

[CreateAssetMenu(fileName = "NewRunnerSettings", menuName = "Runner/Game Settings")]
public class RunnerGameSettingsSO : ScriptableObject
{
    [Header("Speed Settings")]
    [SerializeField] private float initialSpeed = 10f;
    public float InitialSpeed => initialSpeed;
    
    [SerializeField] private float acceleration = 0.1f;
    public float Acceleration => acceleration;

    [SerializeField] private float maxSpeed = 20f;
    public float MaxSpeed => maxSpeed;


    [Header("Spawn Settings")]
    [SerializeField] private float minSpawnDelay = 0.7f;
    public float MinSpawnDelay => minSpawnDelay;

    [SerializeField] private float maxSpawnDelay = 1.5f;
    public float MaxSpawnDelay => maxSpawnDelay;

    [SerializeField, Range(0f, 1f)] private float coinChance = 0.3f;
    public float CoinChance => coinChance;
}