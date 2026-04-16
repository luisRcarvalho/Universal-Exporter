using UnityEngine;

public class RunnerPlayerController : MonoBehaviour
{
    private enum AerialState { Grounded, Jumping, Gliding, DoubleJumping }
    
    [Header("Data")]
    [SerializeField] private PlayerSettingsSO settings;

    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerVisualController visualController;
    public bool IsStunned { get; private set; } 

    private bool _isGrounded = true;
    private int _jumpsRemaining;
    private AerialState _currentAerialState = AerialState.Grounded;

    private void OnEnable()
    {
        GameManager.OnGameEnded += HandleGameOver;
    }

    private void OnDisable()
    {
        GameManager.OnGameEnded -= HandleGameOver;
    }
    private void Start()
    {
        _jumpsRemaining = settings.MaxJumps;
    }
    
    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.HasGameStarted) return;
        
        if (IsStunned) return;
        
        if (Input.GetButtonDown("Jump") || Input.GetMouseButtonDown(0))
        {
            if (_jumpsRemaining > 0)
            {
                PerformJump();
            }
        }
        
        UpdateAerialState();
    }
    private void FixedUpdate()
    {
        if (GameManager.Instance == null || !GameManager.Instance.HasGameStarted) return;
        
        CheckOutOfBounds();

        if (IsStunned) return;
        
        ApplyPhysics();
        
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
    }
    private void CheckOutOfBounds()
    {
        if (transform.position.y < settings.MinYThreshold)
        {
            transform.position = new Vector3(transform.position.x, settings.ResetYPosition, transform.position.z);
            rb.linearVelocity = Vector3.zero;
        }
    }
    private void PerformJump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        bool isFirstJump = (_jumpsRemaining == settings.MaxJumps);
        float force = isFirstJump ? settings.JumpForce : settings.DoubleJumpForce;

        rb.AddForce(Vector3.up * force, ForceMode.VelocityChange);
        
        _isGrounded = false;
        _jumpsRemaining--;

        _currentAerialState = isFirstJump ? AerialState.Jumping : AerialState.DoubleJumping;
        visualController.SetIsJumping(true);
    }
    private void ApplyPhysics()
    {
        if (_isGrounded) return;
        
        if (rb.linearVelocity.y > 0) return;
        
        bool isJumpHeld = Input.GetButton("Jump") || Input.GetMouseButton(0);
        bool canGlide = (_jumpsRemaining == 0);

        if (isJumpHeld && canGlide) 
            ApplyGravity(settings.GlideGravityMultiplier);
        else 
            ApplyGravity(settings.FallGravityMultiplier);
    }
    private void ApplyGravity(float multiplier)
    {
        rb.AddForce(Vector3.up * (Physics.gravity.y * (multiplier - 1)), ForceMode.Acceleration);
    }
    private void UpdateAerialState()
    {
        if (_isGrounded) return;

        if (rb.linearVelocity.y >= 0)
        {
            if (_currentAerialState != AerialState.DoubleJumping)
                _currentAerialState = AerialState.Jumping;
            return;
        }

        _currentAerialState = (_jumpsRemaining == 0 && (Input.GetButton("Jump") || Input.GetMouseButton(0))) 
            ? AerialState.Gliding 
            : AerialState.Grounded;
        
        if (_currentAerialState == AerialState.Gliding)
        {
                visualController.SetIsGliding(true);
            
        }
        else
        {
            visualController.SetIsFalling(true); 
        }
    }
    
    public void TakeStun()
    {
        IsStunned = true;
        rb.linearVelocity = Vector3.zero;
        visualController.SetIsStunned(true);
    }
    
    public void RecoverFromStun()
    {
        IsStunned = false;
        
        if (_isGrounded)
        {
            _currentAerialState = AerialState.Grounded;
            _jumpsRemaining = settings.MaxJumps;
        }
        else
        {
            _currentAerialState = AerialState.Jumping; 
        }
    }
    private void HandleGameOver()
    {
        rb.linearVelocity = Vector3.zero; 
        
        if (visualController != null)
        {
            visualController.Idle();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _isGrounded = true; 
            _jumpsRemaining = settings.MaxJumps; 
            _currentAerialState = AerialState.Grounded;
            
            visualController.Land();
            visualController.Run();
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) 
        {
            _isGrounded = false;
        }
    }
}