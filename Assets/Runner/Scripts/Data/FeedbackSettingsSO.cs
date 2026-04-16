using UnityEngine;

[CreateAssetMenu(fileName = "NewFeedbackSettings", menuName = "Runner/Feedback Settings")]
public class FeedbackSettingsSO : ScriptableObject
{
    [field: Header("Audio")]
    [field: SerializeField] public AudioClip CoinSound { get; private set; }
    [field: SerializeField] public AudioClip ObstacleHitSound { get; private set; }
    [field: SerializeField, Range(0f, 1f)] public float SfxVolume { get; private set; } = 0.8f;

    [field: Header("Visual Effects (VFX) - Para o Futuro")]
    [field: SerializeField] public GameObject CoinCollectVFXPrefab { get; private set; }
    [field: SerializeField] public GameObject ObstacleHitVFXPrefab { get; private set; }
}