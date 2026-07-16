using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class DecoyOrb : MonoBehaviour
{
    private float stunDuration;
    private float knockbackForce;
    
    [Header("Sonidos")]
    public AudioClip explosionSound;

    // El inventario llama a este método al poner la trampa
    public void Initialize(ItemData data)
    {
        stunDuration = data.effectDuration;
        knockbackForce = data.mainPower;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hero"))
        {
            if (explosionSound != null && AudioManager.instance != null) 
            {
                AudioManager.instance.PlaySFX(explosionSound);
            }

            PlayerMovement heroMovement = other.GetComponent<PlayerMovement>();
            if (heroMovement != null)
            {
                // Tomamos hacia dónde mira el Héroe y lo invertimos (para tirarlo hacia atrás)
                float empujeX = -heroMovement.facingDirection;
                // Le damos una fuerza fija hacia arriba
                float empujeY = 1f; 
                // Creamos el vector diagonal y lo normalizamos
                Vector2 knockbackDirection = new Vector2(empujeX, empujeY).normalized;
                // Aplicamos la fuerza física multiplicada por nuestro mainPower
                heroMovement.ApplyKnockback(knockbackDirection * knockbackForce);
                // Aplicamos el Stun
                heroMovement.ApplyStun(stunDuration);
                
                Debug.Log("¡El Héroe cayó en la trampa del señuelo y salió volando!");
            }
            
            Destroy(gameObject);
        }
    }
}