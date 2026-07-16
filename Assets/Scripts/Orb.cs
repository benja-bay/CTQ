using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class Orb : MonoBehaviour
{
    [Header("Configuración del Orbe")]
    public float respawnTime = 5f;
    
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        
        if (inventory != null && inventory.HasEmptySlot())
        {
            if (AudioManager.instance != null) 
            {
                AudioManager.instance.PlaySFX(AudioManager.instance.orbPickupSound);
            }

            PlayerRole role = other.GetComponent<PlayerRole>();
            inventory.AssignRandomItem(role.currentRole);
            
            StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        spriteRenderer.enabled = false;
        col.enabled = false;
        yield return new WaitForSeconds(respawnTime);
        spriteRenderer.enabled = true;
        col.enabled = true;
    }
}