using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public static event Action<float> OnScoreUpdated;
    public static event Action OnGameEnded;
    
    public static event Action<Vector3> OnCoinCollected;
    public static event Action<Vector3> OnObstacleHit;

    [SerializeField] private float pointsPerSecond = 1f;
    
    private float _currentScore = 0f;
    public bool HasGameStarted { get; private set; } = false;
    public bool IsGameOver { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if (!HasGameStarted || IsGameOver) return;
        
        _currentScore += pointsPerSecond * Time.deltaTime;
        
        OnScoreUpdated?.Invoke(_currentScore);
    }
    
    public void StartGame()
    {
        HasGameStarted = true;
        IsGameOver = false;
        
        RunnerSpawnController.Instance.StartSpawning();
    }

    public void CollectCoin(float amount, Vector3 position)
    {
        if (!HasGameStarted || IsGameOver) return;

        _currentScore += amount;
        OnScoreUpdated?.Invoke(_currentScore);
        
        OnCoinCollected?.Invoke(position);
    }
    
    public void HitObstacle(Vector3 position)
    {
        if (IsGameOver) return;
        
        OnObstacleHit?.Invoke(position);
        GameOver();
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        HasGameStarted = false;
        
        RunnerSpawnController.Instance.StopSpawning();
        OnGameEnded?.Invoke();
    }
}