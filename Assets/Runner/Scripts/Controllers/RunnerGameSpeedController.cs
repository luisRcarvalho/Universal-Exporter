using UnityEngine;

public class RunnerGameSpeedController : MonoBehaviour
{
    public static RunnerGameSpeedController Instance { get; private set; }

    [Header("Data")]
    [SerializeField] private RunnerGameSettingsSO gameSettings;

    private float _currentSpeed;

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
        _currentSpeed = gameSettings.InitialSpeed;;
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.HasGameStarted && !GameManager.Instance.IsGameOver)
        {
            if (_currentSpeed < gameSettings.MaxSpeed)
            {
                _currentSpeed += gameSettings.Acceleration * Time.deltaTime;
            }
        }
    }

    public float GetSpeed()
    {
        if (GameManager.Instance == null || !GameManager.Instance.HasGameStarted || GameManager.Instance.IsGameOver) 
            return 0f;

        return _currentSpeed;
    }
}
