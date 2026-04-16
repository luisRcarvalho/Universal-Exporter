using UnityEngine;
public class ReturnToPoolOnParticleStop : MonoBehaviour
{
    [SerializeField] private GameObject myPrefabSource;
    private void OnParticleSystemStopped()
    {
        if (VFXFeedbackController.Instance != null && myPrefabSource != null)
        {
            VFXFeedbackController.Instance.ReturnVFXToPool(myPrefabSource, gameObject);
        }
    }
}