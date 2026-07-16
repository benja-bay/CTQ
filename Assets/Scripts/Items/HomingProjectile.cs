using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class HomingProjectile : MonoBehaviour
{
    [Header("Sonidos del Proyectil")]
    public AudioClip shootSound;
    public AudioClip impactSound;

    private float speed;
    private float rotateSpeed;
    private float stunDuration;

    private Transform target;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; 
    }

    public void Initialize(ItemData data, float facingDirection)
    {
        speed = data.mainPower;
        rotateSpeed = data.secondaryPower;
        stunDuration = data.effectDuration;
        
        if (shootSound != null && AudioManager.instance != null) AudioManager.instance.PlaySFX(shootSound);

        GameObject banner = GameObject.FindGameObjectWithTag("Banner");
        if (banner != null) target = banner.transform;

        if (facingDirection < 0) transform.rotation = Quaternion.Euler(0, 0, 180f);
    }

    void FixedUpdate()
    {
        if (target != null)
        {
            Vector2 direction = (Vector2)target.position - rb.position;
            direction.Normalize();
            float rotateAmount = Vector3.Cross(direction, transform.right).z;
            rb.angularVelocity = -rotateAmount * rotateSpeed;
        }
        else rb.angularVelocity = 0f;

        rb.linearVelocity = transform.right * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Banner"))
        {
            if (impactSound != null && AudioManager.instance != null) AudioManager.instance.PlaySFX(impactSound);

            PlayerMovement pm = other.GetComponent<PlayerMovement>();
            if (pm != null) pm.ApplyStun(stunDuration);
            
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}