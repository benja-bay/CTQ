using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Ajustes de Velocidad")]
    public float moveSpeed = 8f;
    public float jumpForce = 15f;
    public float fastFallForce = 40f;

    [Header("Ajustes de Better Jump")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 3.5f;
    
    [Header("Controles")]
    public KeyCode keyLeft;
    public KeyCode keyRight;
    public KeyCode keyJump;
    public KeyCode keyDown;

    [Header("Detección de Suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    
    // --- VARIABLES DE ESTADO ---
    public float facingDirection { get; private set; } = 1f; 
    public bool canMove = true;
    public bool isDashing = false; 
    public bool isKnockedBack = false;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;
    private bool isFastFalling;
    private bool isHoldingJump;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!canMove)
        {
            horizontalInput = 0f;
            isFastFalling = false;
            isHoldingJump = false;
            
            // Aunque no se pueda mover (ej: por Stun), debemos actualizar el Animator
            UpdateAnimator(0f);
            return;
        }

        horizontalInput = 0f;
        if (Input.GetKey(keyLeft)) horizontalInput = -1f;
        if (Input.GetKey(keyRight)) horizontalInput = 1f;

        if (horizontalInput != 0)
        {
            facingDirection = horizontalInput;
            FlipActiveVisuals(facingDirection); // <-- Volteamos el dibujo
        }

        if (Input.GetKeyDown(keyJump) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        
        isHoldingJump = Input.GetKey(keyJump);
        isFastFalling = Input.GetKey(keyDown) && !isGrounded;

        // Actualizamos las variables del Animator en cada frame
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

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !isHoldingJump)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
        
        if (isFastFalling)
        {
            rb.linearVelocity += Vector2.down * fastFallForce * Time.fixedDeltaTime;
        }
    }

    // ==========================================
    // NUEVO: SISTEMA DE ANIMACIÓN Y VISUALES
    // ==========================================

    // Busca cuál es el Animator que está encendido actualmente (Héroe o Banderín)
    private Animator GetActiveAnimator()
    {
        // GetComponentsInChildren con (false) ignora los objetos apagados
        Animator[] animators = GetComponentsInChildren<Animator>(false); 
        if (animators.Length > 0) return animators[0];
        return null;
    }

    private void UpdateAnimator(float input)
    {
        Animator activeAnim = GetActiveAnimator();
        if (activeAnim == null) return;

        // 1. Velocidad (siempre en positivo gracias a Mathf.Abs)
        float currentSpeed = Mathf.Abs(input * moveSpeed);
        activeAnim.SetFloat("Speed", currentSpeed);

        // 2. Suelo
        activeAnim.SetBool("isGrounded", isGrounded);

        // 3. Stun (Si no puede moverse y no está usando el dash ni volando por los aires)
        bool isStunned = !canMove && !isDashing && !isKnockedBack;
        activeAnim.SetBool("isStunned", isStunned);

        // 4. Dash
        activeAnim.SetBool("isDashing", isDashing);
    }

    private void FlipActiveVisuals(float direction)
    {
        Animator activeAnim = GetActiveAnimator();
        if (activeAnim != null)
        {
            // Solo invertimos la escala del hijo que tiene el dibujo, protegiendo al padre
            activeAnim.transform.localScale = new Vector3(direction, 1f, 1f);
        }
    }

    // Esta función la puede llamar PlayerInventory cuando disparas un ítem
    public void TriggerCastAnimation()
    {
        Animator activeAnim = GetActiveAnimator();
        if (activeAnim != null) 
        {
            activeAnim.SetTrigger("Cast");
        }
    }

    // ==========================================
    // FÍSICAS Y ESTADOS DE ALTERACIÓN
    // ==========================================

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