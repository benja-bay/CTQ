using UnityEngine;
using System.Collections;

public class PlayerInventory : MonoBehaviour
{
    [Header("Configuración de Controles")]
    public KeyCode useItemKey;
    public KeyCode discardItemKey;

    [Header("Estado del Inventario")]
    public bool hasItem = false;
    public ItemData currentItemData; 

    [Header("Listas de Objetos (Loot Pools)")]
    public ItemData[] heroPool;
    public ItemData[] bannerPool;

    private PlayerMovement playerMovement;
    private Rigidbody2D rb;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
    }

    public bool HasEmptySlot()
    {
        return !hasItem;
    }

    public void AssignRandomItem(Role currentRole)
    {
        if (currentRole == Role.Hero && heroPool.Length > 0)
        {
            currentItemData = heroPool[Random.Range(0, heroPool.Length)];
        }
        else if (currentRole == Role.Banner && bannerPool.Length > 0)
        {
            currentItemData = bannerPool[Random.Range(0, bannerPool.Length)];
        }

        hasItem = true;
        Debug.Log($"[{gameObject.name}] received item: {currentItemData.itemName}");
    }

    void Update()
    {
        if (Input.GetKeyDown(useItemKey) && hasItem)
        {
            UseItem();
        }

        if (Input.GetKeyDown(discardItemKey) && hasItem)
        {
            Debug.Log($"[{gameObject.name}] DISCARDED: {currentItemData.itemName}");
            ClearSlot();
        }
    }

    private void UseItem()
    {
        Debug.Log($"[{gameObject.name}] USED: {currentItemData.itemName}");

        switch (currentItemData.itemType)
        {
            case ItemType.Dash:
                StartCoroutine(ExecuteDashRoutine());
                break;
            case ItemType.SpeedPotion:
                StartCoroutine(ExecuteSpeedPotionRoutine());
                break;
            case ItemType.Cobweb:
            case ItemType.Decoy:
            case ItemType.StunProjectile:
            case ItemType.Hook:
                SpawnItemPrefab();
                break;
        }

        ClearSlot();
    }

    private void ClearSlot()
    {
        hasItem = false;
        currentItemData = null;
    }

    // ==========================================
    // LÓGICA DE ÍTEMS DIRECTOS (BUFFS / MOVIMIENTO)
    // ==========================================

    private IEnumerator ExecuteDashRoutine()
    {
        // Bloqueamos el input y avisamos que es un Dash
        playerMovement.canMove = false;
        playerMovement.isDashing = true;
        
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        // Aplicamos la velocidad usando el mainPower del Scriptable Object
        rb.linearVelocity = new Vector2(playerMovement.facingDirection * currentItemData.mainPower, 0f);

        // Esperamos lo que dicte effectDuration (ej: 0.15 o 0.2 segundos)
        yield return new WaitForSeconds(currentItemData.effectDuration);

        // Frenamos en seco para mayor precisión y restauramos todo
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.gravityScale = originalGravity;
        
        playerMovement.isDashing = false;
        playerMovement.canMove = true;
    }

    private IEnumerator ExecuteSpeedPotionRoutine()
    {
        float originalSpeed = playerMovement.moveSpeed;
        
        // Multiplicamos la velocidad usando el mainPower
        playerMovement.moveSpeed *= currentItemData.mainPower;

        yield return new WaitForSeconds(currentItemData.effectDuration);

        // Importante: Chequeamos el rol al terminar por si hubo un cambio de roles a mitad del buff
        PlayerRole role = GetComponent<PlayerRole>();
        playerMovement.moveSpeed = role.currentRole == Role.Hero ? 8f : 9f;
    }

    // ==========================================
    // LÓGICA DE INSTANCIACIÓN (TRAMPAS Y PROYECTILES)
    // ==========================================

    private void SpawnItemPrefab()
    {
        if (currentItemData.itemPrefab != null)
        {
            // Tomamos la posición base del jugador
            Vector3 spawnPos = transform.position;

            // Si el ítem es el Gancho, lo bajamos un poco para que salga del pecho
            if (currentItemData.itemType == ItemType.Hook)
            {
                spawnPos.y += 0.2f;
                spawnPos.x += playerMovement.facingDirection * 0.5f;
            }
            // Si el ítem es una Trampa (Telaraña o Señuelo), lo ponemos por la espalda
            else if (currentItemData.itemType == ItemType.Cobweb || currentItemData.itemType == ItemType.Decoy)
            {
                // Usamos facingDirection. Si mira a la derecha, resta 1.5 a la X (lo pone a la izquierda).
                // Ajusta este "1.5f" si quieres que caiga más cerca o más lejos de su espalda.
                spawnPos.x -= playerMovement.facingDirection * 1.5f;
            }

            // Instanciamos el prefab en la nueva posición calculada
            GameObject spawnedItem = Instantiate(currentItemData.itemPrefab, spawnPos, Quaternion.identity);
            
            // Intentamos inicializar la Telaraña
            CobwebTrap cobwebScript = spawnedItem.GetComponent<CobwebTrap>();
            if (cobwebScript != null)
            {
                cobwebScript.Initialize(currentItemData);
            }

            // Intentamos inicializar el Proyectil Teledirigido
            HomingProjectile projectileScript = spawnedItem.GetComponent<HomingProjectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(currentItemData, playerMovement.facingDirection);
            }

            // Intentamos inicializar el Gancho
            GrapplingHook hookScript = spawnedItem.GetComponent<GrapplingHook>();
            if (hookScript != null)
            {
                hookScript.Initialize(currentItemData, transform, playerMovement);
            }
            
            // Intentamos inicializar el Señuelo
            DecoyOrb decoyScript = spawnedItem.GetComponent<DecoyOrb>();
            if (decoyScript != null)
            {
                decoyScript.Initialize(currentItemData);
            }
        }
        else
        {
            Debug.LogWarning($"El item {currentItemData.itemName} no tiene un Prefab asignado en el Inspector.");
        }
    }
}