using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class CobwebTrap : MonoBehaviour
{
    private float slowMultiplier;
    private float effectDuration;
    private float bounceForce;
    
    private int trampolinUses = 2; 

    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    public void Initialize(ItemData data)
    {
        effectDuration = data.effectDuration;
        slowMultiplier = data.mainPower;     
        bounceForce = data.secondaryPower;   
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si la pisa el Héroe Ralentización, uso y se destruye
        if (other.CompareTag("Hero")) 
        {
            PlayerMovement heroMovement = other.GetComponent<PlayerMovement>();
            if (heroMovement != null)
            {
                StartCoroutine(ApplySlowdown(heroMovement));
            }
        }
        // i la pisa el Banderín Trampolín automático
        else if (other.CompareTag("Banner")) 
        {
            PlayerMovement bannerMovement = other.GetComponent<PlayerMovement>();
            if (bannerMovement != null)
            {
                // Lo hace saltar automáticamente
                bannerMovement.ApplyBounce(bounceForce);
                
                // Restamos un uso
                trampolinUses--;
                
                // Si nos quedamos sin usos, la destruimos
                if (trampolinUses <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    private IEnumerator ApplySlowdown(PlayerMovement pm)
    {
        // Ocultamos la telaraña (el collider también se apaga, así que nadie más puede pisarla)
        spriteRenderer.enabled = false;
        col.enabled = false;

        float originalSpeed = pm.moveSpeed;
        pm.moveSpeed *= slowMultiplier;

        yield return new WaitForSeconds(effectDuration);

        pm.moveSpeed = originalSpeed;
        
        // Se destruye tras cumplir su efecto
        Destroy(gameObject);
    }
}