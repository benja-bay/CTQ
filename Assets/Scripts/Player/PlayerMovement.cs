using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
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
    
        // Nuevo buffer para bajar de plataformas
        private float dropDownBufferCounter;

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
    
        // Variable pública leída por OneWayModule.cs
        public bool isTryingToDropDown { get; private set; } 

        private Rigidbody2D rb;
        private float horizontalInput;
        private bool isGrounded;
        private bool isFastFalling;
        private bool isHoldingJump;

        private PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction jumpAction;
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
        
            if (confusionIcon != null) confusionIcon.gameObject.SetActive(false);
        }

        void Update()
        {
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
                isTryingToDropDown = false;
                dropDownBufferCounter = 0f;
                stepTimer = 0f;
                playerVFX.DeactivateVFX(playerVFX.vfx.Dust);
                if (walkAudioSource != null && walkAudioSource.isPlaying) walkAudioSource.Stop();
                UpdateAnimator(0f);
                return;
            }

            // ==========================================
            // PROCESAMIENTO DE INPUTS (INVERSIÓN Y COMBOS)
            // ==========================================
            Vector2 moveInput = moveAction.ReadValue<Vector2>();
        
            // Inversión Horizontal
            horizontalInput = isConfused ? -moveInput.x : moveInput.x;

            // Dirección lógica "Hacia Abajo"
            bool logicalDown = isConfused ? (moveInput.y >= 0.5f) : (moveInput.y <= -0.5f);

            // Salto
            bool jumpPressed = jumpAction.WasPressedThisFrame();
            bool jumpReleased = jumpAction.WasReleasedThisFrame();
            isHoldingJump = jumpAction.IsInProgress();
        
            // Fast Fall puramente ejecutado con el eje vertical lógico hacia abajo
            isFastFalling = logicalDown && !isGrounded;

            // === LÓGICA DE DROP-DOWN CON BUFFER ===
            bool isKeyboard = playerInput.currentControlScheme != null && playerInput.currentControlScheme.Contains("Keyboard");

            if (isKeyboard)
            {
                // En Teclado: Basta con mirar hacia abajo (múltiples frames asegurados)
                isTryingToDropDown = logicalDown;
            }
            else
            {
                // En Joystick: Almacenamos la intención en un Buffer para que las Físicas no lo pierdan
                if (logicalDown && jumpPressed)
                {
                    dropDownBufferCounter = jumpBufferTime; 
                }
                else
                {
                    dropDownBufferCounter -= Time.deltaTime;
                }
            
                isTryingToDropDown = dropDownBufferCounter > 0f;
            }

            // CRUCIAL: Consumimos el salto para no saltar en el aire después de atravesar
            if (logicalDown && jumpPressed && !isKeyboard)
            {
                jumpPressed = false;
            }

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
}