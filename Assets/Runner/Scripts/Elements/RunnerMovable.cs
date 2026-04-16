using System;
using UnityEngine;

public class RunnerMovable : MonoBehaviour
{
    private Action<GameObject> _onDespawn;
    private float _cleanupDistance;

    public void Initialize(Action<GameObject> onDespawn, float cleanupDistance)
    {
        _onDespawn = onDespawn;
        _cleanupDistance = cleanupDistance;
    }

    private void Update()
    {
        if (RunnerGameSpeedController.Instance == null) return;
        
        float speed = RunnerGameSpeedController.Instance.GetSpeed() * Time.deltaTime;
        transform.Translate(Vector3.back * speed, Space.World);
        
        if (-transform.position.z > _cleanupDistance)
        {
            Despawn();
        }
    }

    public void Despawn()
    {
        _onDespawn?.Invoke(gameObject);
    }
}