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

    [Header("Estado de Confusión")]
    public Transform confusionIcon; 
    public float confusionRotationSpeed = -360f; // Grados por segundo
    public bool isConfused { get; private set; }
    private Coroutine confusionCoroutine;

    [Header("Sonidos de Movimiento")]
    public AudioSource audioSource;
    public AudioSource walkAudioSource;
    public AudioClip jumpSound;
    public AudioClip[] fallSounds;
    public AudioClip[] walkSounds;

    public float stepInterval = 0.3f;
    private float stepTimer;
    private bool wasGrounded;

    public float facingDirection { get; private set; } = 1f;
    public bool canMove = true;
    public bool isDashing = false;
    public bool isKnockedBack = false;
    public bool isPreparing = false;
    public bool isCasting = false;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;
    private bool isFastFalling;
    private bool isHoldingJump;
    
    // Variable para trackear el stick en gamepads
    private float previousMoveY;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction fastFallAction;
    private PlayerVFX playerVFX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        playerVFX = GetComponent<PlayerVFX>();

        playerInput.neverAutoSwitchControlSchemes = true;
        string desiredScheme = "Keyboard";
        playerInput.user.UnpairDevices();

        if (gameObject.name == "Player1")
        {
            desiredScheme = GameManager.instance != null ? GameManager.instance.p1ControlScheme : "Keyboard_P1";
            if (desiredScheme == "Gamepad" && Gamepad.all.Count > 0)
            {
                playerInput.SwitchCurrentControlScheme("Gamepad", Gamepad.all[0]);
            }
            else
            {
                playerInput.SwitchCurrentControlScheme("Keyboard_P1", Keyboard.current);
            }
        }
        else if (gameObject.name == "Player2")
        {
            desiredScheme = GameManager.instance != null ? GameManager.instance.p2ControlScheme : "Keyboard_P2";
            if (desiredScheme == "Gamepad")
            {
                if (Gamepad.all.Count > 1)
                {
                    playerInput.SwitchCurrentControlScheme("Gamepad", Gamepad.all[1]);
                }
                else if (Gamepad.all.Count == 1)
                {
                    playerInput.SwitchCurrentControlScheme("Gamepad", Gamepad.all[0]);
                }
            }
            else
            {
                playerInput.SwitchCurrentControlScheme("Keyboard_P2", Keyboard.current);
            }
        }

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        fastFallAction = playerInput.actions["FastFall"];
        
        if (confusionIcon != null) confusionIcon.gameObject.SetActive(false);
    }

    void Update()
    {
        // 1. Rotación del icono de confusión
        if (isConfused && confusionIcon != null)
        {
            confusionIcon.Rotate(0, 0, confusionRotationSpeed * Time.deltaTime);
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!wasGrounded && isGrounded)
        {
            if (fallSounds.Length > 0 && audioSource != null)
                audioSource.PlayOneShot(fallSounds[Random.Range(0, fallSounds.Length)]);
        }
        wasGrounded = isGrounded;

        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        if (!canMove)
        {
            horizontalInput = 0f;
            isFastFalling = false;
            isHoldingJump = false;
            stepTimer = 0f;
            playerVFX.DeactivateVFX(playerVFX.vfx.Dust);
            if (walkAudioSource != null && walkAudioSource.isPlaying) walkAudioSource.Stop();
            UpdateAnimator(0f);
            return;
        }

        // ==========================================
        // PROCESAMIENTO DE INPUTS (INVERSIÓN)
        // ==========================================
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        
        // Simulación de "Botón" para el Joystick Vertical
        bool stickDownPressed = moveInput.y <= -0.5f && previousMoveY > -0.5f;
        bool stickDownReleased = moveInput.y > -0.5f && previousMoveY <= -0.5f;

        // Inversión Horizontal
        horizontalInput = isConfused ? -moveInput.x : moveInput.x;

        // Inversión de Salto y FastFall (Abajo salta, Arriba baja)
        bool jumpPressed = isConfused ? (fastFallAction.WasPressedThisFrame() || stickDownPressed) : jumpAction.WasPressedThisFrame();
        bool jumpReleased = isConfused ? (fastFallAction.WasReleasedThisFrame() || stickDownReleased) : jumpAction.WasReleasedThisFrame();
        
        isHoldingJump = isConfused ? (fastFallAction.IsInProgress() || moveInput.y <= -0.5f) : jumpAction.IsInProgress();
        
        bool fastFallInput = isConfused ? (jumpAction.IsInProgress() || moveInput.y >= 0.5f) : (fastFallAction.IsInProgress() || moveInput.y <= -0.5f);
        isFastFalling = fastFallInput && !isGrounded;

        previousMoveY = moveInput.y;

        // ==========================================
        // EJECUCIÓN FÍSICA
        // ==========================================
        if (jumpPressed) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        if (horizontalInput != 0)
        {
            facingDirection = Mathf.Sign(horizontalInput);
            FlipActiveVisuals(facingDirection);
        }

        bool isMovingOnGround = isGrounded && Mathf.Abs(horizontalInput) > 0.01f;

        if (isMovingOnGround)
        {
            playerVFX.ActivateVFX(playerVFX.vfx.Dust);
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                stepTimer = stepInterval;
                if (walkSounds.Length > 0)
                {
                    if (walkAudioSource != null)
                    {
                        walkAudioSource.clip = walkSounds[Random.Range(0, walkSounds.Length)];
                        walkAudioSource.Play();
                    }
                    else if (audioSource != null)
                    {
                        audioSource.PlayOneShot(walkSounds[Random.Range(0, walkSounds.Length)]);
                    }
                }
            }
        }
        else
        {
            playerVFX.DeactivateVFX(playerVFX.vfx.Dust);
            stepTimer = 0f;
            if (walkAudioSource != null && walkAudioSource.isPlaying) walkAudioSource.Stop();
        }

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (jumpSound != null && audioSource != null) audioSource.PlayOneShot(jumpSound);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        if (jumpReleased && rb.linearVelocity.y > 0f)
        {
            coyoteTimeCounter = 0f;
        }

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

        bool isStunned = !canMove && !isDashing && !isKnockedBack && !isPreparing && !isCasting;
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
        if (activeAnim != null) activeAnim.SetTrigger("Cast");
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

    // ==========================================
    // SISTEMA DE CONFUSIÓN
    // ==========================================
    public void ApplyConfusion(float duration)
    {
        if (confusionCoroutine != null) StopCoroutine(confusionCoroutine);
        confusionCoroutine = StartCoroutine(ConfusionRoutine(duration));
    }

    private IEnumerator ConfusionRoutine(float duration)
    {
        isConfused = true;
        if (confusionIcon != null) confusionIcon.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        isConfused = false;
        if (confusionIcon != null) confusionIcon.gameObject.SetActive(false);
        confusionCoroutine = null;
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