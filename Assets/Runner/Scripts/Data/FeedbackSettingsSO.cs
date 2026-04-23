using UnityEngine;

[CreateAssetMenu(fileName = "NewFeedbackSettings", menuName = "Runner/Feedback Settings")]
public class FeedbackSettingsSO : ScriptableObject
{
    [field: Header("Audio")] 
    
    [field: SerializeField] private AudioClip coinSound;
    public AudioClip CoinSound => coinSound;

    [field: SerializeField] private AudioClip obstacleHitSound;
    public AudioClip ObstacleHitSound => obstacleHitSound;
    
    [field: SerializeField, Range(0f, 1f)] private float sfxVolume;
    public float SfxVolume => sfxVolume;

    [field: Header("Visual Effects (VFX)")] 
    
    [field: SerializeField] private GameObject coinCollectVFXPrefab;
    public GameObject CoinCollectVFXPrefab => coinCollectVFXPrefab;
    
    [field: SerializeField] private GameObject obstacleHitVFXPrefab;
    public GameObject ObstacleHitVFXPrefab => obstacleHitVFXPrefab;
}