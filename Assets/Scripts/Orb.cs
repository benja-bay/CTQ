using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class Orb : MonoBehaviour
{
    [Header("Configuración del Orbe")]
    [Tooltip("Tiempo en segundos que tarda el orbe en reaparecer tras ser recogido.")]
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
        // Buscamos si el objeto que colisionó tiene un inventario
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        
        // Si tiene inventario y además tiene el slot vacío...
        if (inventory != null && inventory.HasEmptySlot())
        {
            // Obtenemos el rol del jugador para saber qué pool de objetos usar
            PlayerRole role = other.GetComponent<PlayerRole>();
            inventory.AssignRandomItem(role.currentRole);
            
            // Apagamos el orbe temporalmente
            StartCoroutine(RespawnRoutine());
        }
    }

    private IEnumerator RespawnRoutine()
    {
        // Desactivamos visuales y físicas
        spriteRenderer.enabled = false;
        col.enabled = false;
        
        // Esperamos el tiempo establecido
        yield return new WaitForSeconds(respawnTime);
        
        // Lo volvemos a activar
        spriteRenderer.enabled = true;
        col.enabled = true;
    }
}