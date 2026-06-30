using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class HomingProjectile : MonoBehaviour
{
    private float speed;
    private float rotateSpeed;
    private float stunDuration;

    private Transform target;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // El proyectil flota, no debe ser afectado por la gravedad
        rb.gravityScale = 0f; 
    }

    // El inventario le pasa los datos y la dirección a la que mira el jugador
    public void Initialize(ItemData data, float facingDirection)
    {
        speed = data.mainPower;
        rotateSpeed = data.secondaryPower;
        stunDuration = data.effectDuration;

        // Buscar al Banderín automáticamente
        GameObject banner = GameObject.FindGameObjectWithTag("Banner");
        if (banner != null)
        {
            target = banner.transform;
        }

        // Si el héroe miraba a la izquierda, el proyectil nace mirando a la izquierda
        if (facingDirection < 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 180f);
        }
    }

    void FixedUpdate()
    {
        if (target != null)
        {
            // Calcular la dirección hacia el objetivo
            Vector2 direction = (Vector2)target.position - rb.position;
            direction.Normalize();

            // Producto Cruz para saber si debe girar en sentido horario o antihorario
            float rotateAmount = Vector3.Cross(direction, transform.right).z;

            // Aplicar la rotación (mientras más alto el rotateSpeed, más cerrado es el giro)
            rb.angularVelocity = -rotateAmount * rotateSpeed;
        }
        else
        {
            // Si el objetivo desapareció, deja de girar
            rb.angularVelocity = 0f;
        }

        // Impulsar siempre hacia "adelante" (transform.right es el frente local en Unity 2D)
        rb.linearVelocity = transform.right * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si impacta al Banderín
        if (other.CompareTag("Banner"))
        {
            PlayerMovement pm = other.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                pm.ApplyStun(stunDuration);
                Debug.Log("¡Banderín stuneado por el proyectil!");
            }
            
            // TODO: Más adelante pondremos aquí la instanciación de las partículas/sonido de explosión
            Destroy(gameObject);
        }
        // Si choca contra el suelo o una pared
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}