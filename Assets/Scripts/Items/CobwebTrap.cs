using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class CobwebTrap : MonoBehaviour
{
    private float effectDuration;

    [Header("Sonidos")]
    public AudioClip triggerSound;

    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    public void Initialize(ItemData data)
    {
        // Usamos la duración del ScriptableObject
        effectDuration = data.effectDuration; 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hero"))
        {
            PlayerVFX playerVFX = other.GetComponent<PlayerVFX>();
            
            if (triggerSound != null && AudioManager.instance != null) 
                AudioManager.instance.PlaySFX(triggerSound);
                
            PlayerMovement heroMovement = other.GetComponent<PlayerMovement>();
            
            if (heroMovement != null)
            {
                StartCoroutine(ApplyConfusionTrap(heroMovement, playerVFX));
            }
        }
    }

    private IEnumerator ApplyConfusionTrap(PlayerMovement pm, PlayerVFX playerVFX)
    {
        // Escondemos la trampa del mapa
        spriteRenderer.enabled = false;
        col.enabled = false;

        // Disparamos el nuevo estado de confusión (reutilizable)
        pm.ApplyConfusion(effectDuration);
        
        // Mantenemos el VFX de la telaraña como feedback adicional
        playerVFX.ActivateVFX(playerVFX.vfx.Cobweb);

        yield return new WaitForSeconds(effectDuration);

        playerVFX.DeactivateVFX(playerVFX.vfx.Cobweb);
        Destroy(gameObject);
    }
}