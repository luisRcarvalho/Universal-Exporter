using System.Collections.Generic;
using UnityEngine;
public class VFXFeedbackController : MonoBehaviour
{
    public static VFXFeedbackController Instance { get; private set; }

    [Header("Data")]
    [SerializeField] private FeedbackSettingsSO feedbackSettings;
    
    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 10;

    private Dictionary<GameObject, Queue<GameObject>> _vfxPools;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializePools();
    }

    private void OnEnable()
    {
        GameManager.OnCoinCollected += PlayCoinVFX;
        GameManager.OnObstacleHit += PlayObstacleVFX;
    }

    private void OnDisable()
    {
        GameManager.OnCoinCollected -= PlayCoinVFX;
        GameManager.OnObstacleHit -= PlayObstacleVFX;
    }

    private void InitializePools()
    {
        if (feedbackSettings == null) return;

        _vfxPools = new Dictionary<GameObject, Queue<GameObject>>();

        if (feedbackSettings.CoinCollectVFXPrefab != null)
            CreatePool(feedbackSettings.CoinCollectVFXPrefab);

        if (feedbackSettings.ObstacleHitVFXPrefab != null)
            CreatePool(feedbackSettings.ObstacleHitVFXPrefab);
    }

    private void CreatePool(GameObject prefab)
    {
        _vfxPools[prefab] = new Queue<GameObject>();

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(prefab, transform); 
            obj.SetActive(false);
            _vfxPools[prefab].Enqueue(obj);
        }
    }

    private GameObject GetVFXFromPool(GameObject prefab, Vector3 position)
    {
        if (!_vfxPools.ContainsKey(prefab)) return null;

        GameObject obj;
        if (_vfxPools[prefab].Count > 0)
        {
            obj = _vfxPools[prefab].Dequeue();
            obj.transform.position = position;
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, position, Quaternion.identity, transform);
        }

        if (obj.TryGetComponent<ParticleSystem>(out var ps))
        {
            ps.Play();
        }

        return obj;
    }

    public void ReturnVFXToPool(GameObject prefab, GameObject obj)
    {
        obj.SetActive(false);
        if (_vfxPools.ContainsKey(prefab))
        {
            _vfxPools[prefab].Enqueue(obj);
        }
    }

    private void PlayCoinVFX(Vector3 position)
    {
        if (feedbackSettings != null && feedbackSettings.CoinCollectVFXPrefab != null)
        {
            GetVFXFromPool(feedbackSettings.CoinCollectVFXPrefab, position);
        }
    }

    private void PlayObstacleVFX(Vector3 position)
    {
        if (feedbackSettings != null && feedbackSettings.ObstacleHitVFXPrefab != null)
        {
            GetVFXFromPool(feedbackSettings.ObstacleHitVFXPrefab, position);
        }
    }
}