using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Ajustes de Velocidad")]
    public float moveSpeed = 8f;
    public float jumpForce = 15f;
    
    [Tooltip("Velocidad fija de caída al presionar la tecla ABAJO")]
    public float fastFallSpeed = 25f;

    [Header("Ajustes de Better Jump")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 3.5f;
    
    [Header("Ajustes de Game Feel")]
    public float coyoteTime = 0.15f;
    private float coyoteTimeCounter;

    public float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    [Header("Detección de Suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    
    public float facingDirection { get; private set; } = 1f; 
    public bool canMove = true;
    public bool isDashing = false; 
    public bool isKnockedBack = false;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;
    private bool isFastFalling;
    private bool isHoldingJump;

    // --- VARIABLES DEL INPUT SYSTEM ---
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction fastFallAction;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        playerInput.neverAutoSwitchControlSchemes = true; 
        
        if (gameObject.name == "Player1")
        {
            playerInput.SwitchCurrentControlScheme("Keyboard_P1", Keyboard.current);
        }
        else if (gameObject.name == "Player2")
        {
            playerInput.SwitchCurrentControlScheme("Keyboard_P2", Keyboard.current);
        }
        
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        fastFallAction = playerInput.actions["FastFall"];
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
        
        if (jumpAction.WasPressedThisFrame())
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (!canMove)
        {
            horizontalInput = 0f;
            isFastFalling = false;
            isHoldingJump = false;
            
            UpdateAnimator(0f);
            return;
        }

        // Lectura del nuevo Input System
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        horizontalInput = moveInput.x;

        if (horizontalInput != 0)
        {
            facingDirection = Mathf.Sign(horizontalInput);
            FlipActiveVisuals(facingDirection); 
        }
        
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }
        
        if (jumpAction.WasReleasedThisFrame() && rb.linearVelocity.y > 0f)
        {
            coyoteTimeCounter = 0f;
        }
        
        isHoldingJump = jumpAction.IsInProgress();
        isFastFalling = (fastFallAction.IsInProgress() || moveInput.y < -0.5f) && !isGrounded;

        UpdateAnimator(horizontalInput);
    }

    void FixedUpdate()
    {
        if (!canMove && !isDashing && !isKnockedBack && isGrounded)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (canMove)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }
        
        if (isFastFalling)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -fastFallSpeed);
        }
        else if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !isHoldingJump)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    private Animator GetActiveAnimator()
    {
        Animator[] animators = GetComponentsInChildren<Animator>(false); 
        if (animators.Length > 0) return animators[0];
        return null;
    }

    private void UpdateAnimator(float input)
    {
        Animator activeAnim = GetActiveAnimator();
        if (activeAnim == null) return;

        float currentSpeed = Mathf.Abs(input * moveSpeed);
        activeAnim.SetFloat("Speed", currentSpeed);
        activeAnim.SetBool("isGrounded", isGrounded);

        bool isStunned = !canMove && !isDashing && !isKnockedBack;
        activeAnim.SetBool("isStunned", isStunned);
        activeAnim.SetBool("isDashing", isDashing);
    }

    private void FlipActiveVisuals(float direction)
    {
        Animator activeAnim = GetActiveAnimator();
        if (activeAnim != null)
        {
            activeAnim.transform.localScale = new Vector3(direction, 1f, 1f);
        }
    }

    public void TriggerCastAnimation()
    {
        Animator activeAnim = GetActiveAnimator();
        if (activeAnim != null) 
        {
            activeAnim.SetTrigger("Cast");
        }
    }

    public void ApplyBounce(float bounceForce)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
    }

    public void ApplyStun(float duration)
    {
        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        canMove = false;
        yield return new WaitForSeconds(duration);
        canMove = true;
    }

    public void ApplyKnockback(Vector2 force)
    {
        isKnockedBack = true; 
        rb.linearVelocity = Vector2.zero; 
        rb.AddForce(force, ForceMode2D.Impulse);
        Invoke(nameof(EndKnockback), 0.3f); 
    }

    private void EndKnockback()
    {
        isKnockedBack = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}