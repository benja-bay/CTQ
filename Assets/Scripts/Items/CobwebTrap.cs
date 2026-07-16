using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class CobwebTrap : MonoBehaviour
{
    private float slowMultiplier;
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
        effectDuration = data.effectDuration;
        slowMultiplier = data.mainPower;     
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hero")) 
        {
            if (triggerSound != null && AudioManager.instance != null) AudioManager.instance.PlaySFX(triggerSound);

            PlayerMovement heroMovement = other.GetComponent<PlayerMovement>();
            if (heroMovement != null) StartCoroutine(ApplySlowdown(heroMovement));
        }
    }

    private IEnumerator ApplySlowdown(PlayerMovement pm)
    {
        spriteRenderer.enabled = false;
        col.enabled = false;
        
        pm.moveSpeed = 8f * slowMultiplier;

        yield return new WaitForSeconds(effectDuration);
        
        pm.moveSpeed = 8f;
        
        Destroy(gameObject);
    }
}