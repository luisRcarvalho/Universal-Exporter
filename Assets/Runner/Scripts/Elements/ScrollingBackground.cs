using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ScrollingBackground : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector2 scrollDirection = new Vector2(0f, 1f);
    [SerializeField] private float parallaxMultiplier = 0.1f;

    private Renderer _bgRenderer;
    private Material _material;
    private Vector2 _currentOffset;
    private void Awake()
    {
        _bgRenderer = GetComponent<Renderer>();
        
        if (_bgRenderer != null)
        {
            _material = _bgRenderer.material;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.HasGameStarted || GameManager.Instance.IsGameOver) 
            return;

        if (RunnerGameSpeedController.Instance == null || _material == null) 
            return;
        
        float currentSpeed = RunnerGameSpeedController.Instance.GetSpeed() * parallaxMultiplier;
        
        _currentOffset += scrollDirection * (currentSpeed * Time.deltaTime * 0.01f);
        
        _material.mainTextureOffset = _currentOffset;
    }
}