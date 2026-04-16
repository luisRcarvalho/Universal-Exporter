using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerSettings", menuName = "Runner/Player Settings")]
public class PlayerSettingsSO : ScriptableObject
{
    [field: Header("Jump Settings")]
    [field: SerializeField] public float JumpForce { get; private set; } = 8f;
    [field: SerializeField] public float DoubleJumpForce { get; private set; } = 7f;
    [field: SerializeField] public int MaxJumps { get; private set; } = 2;

    [field: Header("Physics & Gravity")]
    [field: SerializeField, Range(1f, 10f)] public float FallGravityMultiplier { get; private set; } = 2.5f;
    [field: SerializeField, Range(0f, 1f)] public float GlideGravityMultiplier { get; private set; } = 0.2f;
    
    [field: Header("Safety Net")]
    [field: SerializeField] public float MinYThreshold { get; private set; } = -5f;
    [field: SerializeField] public float ResetYPosition { get; private set; } = 1.5f;
}