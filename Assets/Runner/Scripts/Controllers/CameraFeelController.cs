using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFeelController : MonoBehaviour
{
    [Header("Dynamic FOV")]
    [SerializeField] private float baseFOV = 60f;
    [SerializeField] private float maxFOV = 75f;
    
    [SerializeField] private float speedForMaxFOV = 20f; 
    [SerializeField] private float fovLerpSpeed = 2f;

    [Header("Hit Stop")]
    [SerializeField] private float hitStopDuration = 0.15f;
    
    private Camera _cam;
    private void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.fieldOfView = baseFOV;
    }

    private void OnEnable()
    {
        GameManager.OnObstacleHit += TriggerHitStop;
    }

    private void OnDisable()
    {
        GameManager.OnObstacleHit -= TriggerHitStop;
    }

    private void Update()
    { 
        UpdateDynamicFOV();
    }

    private void UpdateDynamicFOV()
    {
        if (RunnerGameSpeedController.Instance == null || GameManager.Instance == null) return;
        
        if (!GameManager.Instance.HasGameStarted || GameManager.Instance.IsGameOver)
        {
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, baseFOV, Time.unscaledDeltaTime * fovLerpSpeed);
            return;
        }

        float currentSpeed = RunnerGameSpeedController.Instance.GetSpeed();
        float speedPercent = Mathf.Clamp01(currentSpeed / speedForMaxFOV);
        float targetFOV = Mathf.Lerp(baseFOV, maxFOV, speedPercent);

        _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFOV, Time.deltaTime * fovLerpSpeed);
    }

    private void TriggerHitStop(Vector3 position)
    {
        StartCoroutine(HitStopRoutine());
    }

    private IEnumerator HitStopRoutine()
    {
        Time.timeScale = 0f;
        
        yield return new WaitForSecondsRealtime(hitStopDuration);
        
        Time.timeScale = 1f;
    }
}