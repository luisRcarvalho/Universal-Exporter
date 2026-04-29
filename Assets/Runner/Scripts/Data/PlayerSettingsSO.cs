using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerSettings", menuName = "Runner/Player Settings")]
public class PlayerSettingsSO : ScriptableObject
{
    [field: Header("Jump Settings")]
    
    [field: SerializeField] private float jumpForce;
    public float JumpForce => jumpForce;
    
    [field: SerializeField] private float doubleJumpForce;
    public float DoubleJumpForce => doubleJumpForce;

    [field: SerializeField] private int maxJumps;
    public int MaxJumps => maxJumps;

    [field: Header("Physics & Gravity")] 
    
    [field: SerializeField, Range(1f, 10f)] private float fallGravityMultiplier;
    public float FallGravityMultiplier => fallGravityMultiplier;
    
    [field: SerializeField, Range(0f, 1f)] private float glideGravityMultiplier;
    public float GlideGravityMultiplier => glideGravityMultiplier;
    
    [field: Header("Safety Net")]
    
    [field: SerializeField] private float minYThreshold;
    public float MinYThreshold => minYThreshold;
    
    [field: SerializeField] private float resetYPosition;
    public float ResetYPosition => resetYPosition;
}