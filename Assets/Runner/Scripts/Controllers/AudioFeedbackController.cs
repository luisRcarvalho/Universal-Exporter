using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioFeedbackController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private FeedbackSettingsSO feedbackSettings;
    
    private AudioSource _sfxAudioSource;

    private void Awake()
    {
        _sfxAudioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        GameManager.OnCoinCollected += PlayCoinSound;
        GameManager.OnObstacleHit += PlayObstacleSound;
    }

    private void OnDisable()
    {
        GameManager.OnCoinCollected -= PlayCoinSound;
        GameManager.OnObstacleHit -= PlayObstacleSound;
    }

    private void PlayCoinSound(Vector3 position)
    {
        if (feedbackSettings != null && feedbackSettings.CoinSound != null)
        {
            _sfxAudioSource.PlayOneShot(feedbackSettings.CoinSound, feedbackSettings.SfxVolume);
        }
    }

    private void PlayObstacleSound(Vector3 position)
    {
        if (feedbackSettings != null && feedbackSettings.ObstacleHitSound != null)
        {
            _sfxAudioSource.PlayOneShot(feedbackSettings.ObstacleHitSound, feedbackSettings.SfxVolume);
        }
    }
}