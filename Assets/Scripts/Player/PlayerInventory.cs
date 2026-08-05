using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInventory : MonoBehaviour
{
    [Header("Estado del Inventario")]
    public bool hasItem = false;
    public ItemData currentItemData;

    [Header("Estado de Efectos")]
    public float currentSpeedDuration { get; private set; }
    public float maxSpeedDuration { get; private set; }
    private Coroutine speedCoroutine;

    [Header("Listas de Objetos (Loot Pools)")]
    public ItemData[] heroPool;
    public ItemData[] bannerPool;

    private PlayerMovement playerMovement;
    private GhostTrail ghostTrail;
    private Rigidbody2D rb;

    [Header("Sonidos de Habilidades")]
    public AudioClip dashSound;
    public AudioClip speedPotionSound;
    public AudioClip placeTrapSound;

    // --- VARIABLES DEL INPUT SYSTEM ---
    private PlayerInput playerInput;
    private InputAction useItemAction;
    private InputAction discardItemAction;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        ghostTrail = GetComponent<GhostTrail>();
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        useItemAction = playerInput.actions["UseItem"];
        discardItemAction = playerInput.actions["DiscardItem"];
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
    }

    void Update()
    {
        if (useItemAction != null && useItemAction.WasPressedThisFrame() && hasItem)
        {
            UseItem();
        }

        if (discardItemAction != null && discardItemAction.WasPressedThisFrame() && hasItem)
        {
            ClearSlot();
        }
    }

    private void UseItem()
    {
        switch (currentItemData.itemType)
        {
            case ItemType.Dash:
                if (dashSound != null && AudioManager.instance != null) AudioManager.instance.PlaySFX(dashSound);
                StartCoroutine(ExecuteDashRoutine());
                break;
            case ItemType.SpeedPotion:
                if (speedPotionSound != null && AudioManager.instance != null) AudioManager.instance.PlaySFX(speedPotionSound);
                ActivateSpeedPotion(currentItemData); 
                break;
            case ItemType.Cobweb:
            case ItemType.Decoy:
                if (placeTrapSound != null && AudioManager.instance != null) AudioManager.instance.PlaySFX(placeTrapSound);
                SpawnItemPrefab();
                break;
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

    private IEnumerator ExecuteDashRoutine()
    {
        if (playerMovement != null)
        {
            playerMovement.canMove = false;
            playerMovement.isDashing = true;
        }
        
        if (ghostTrail != null) ghostTrail.SetTrailActive(true);

        float originalGravity = 1f;
        if (rb != null)
        {
            originalGravity = rb.gravityScale;
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(playerMovement.facingDirection * currentItemData.mainPower, 0f);
        }

        yield return new WaitForSeconds(currentItemData.effectDuration);

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            rb.gravityScale = originalGravity;
        }

        if (playerMovement != null)
        {
            playerMovement.isDashing = false;
            playerMovement.canMove = true;
        }
        
        if (ghostTrail != null) ghostTrail.SetTrailActive(false);
    }

    private void ActivateSpeedPotion(ItemData potionData)
    {
        if (speedCoroutine != null)
        {
            StopCoroutine(speedCoroutine);
        }

        maxSpeedDuration = potionData.effectDuration;
        currentSpeedDuration = maxSpeedDuration;
        
        speedCoroutine = StartCoroutine(SpeedPotionRoutine(potionData.mainPower));
    }

    private IEnumerator SpeedPotionRoutine(float speedMultiplier)
    {
        if (ghostTrail != null) ghostTrail.SetTrailActive(true);
        
        PlayerRole role = GetComponent<PlayerRole>();
        float baseSpeed = 8f; 
        if (role != null)
        {
            baseSpeed = role.currentRole == Role.Hero ? 8f : 9f;
        }
        
        if (playerMovement != null) playerMovement.moveSpeed = baseSpeed * speedMultiplier;
        
        while (currentSpeedDuration > 0)
        {
            currentSpeedDuration -= Time.deltaTime;
            yield return null;
        }
        
        if (playerMovement != null) playerMovement.moveSpeed = baseSpeed;
        if (ghostTrail != null) ghostTrail.SetTrailActive(false);
        
        currentSpeedDuration = 0f;
        speedCoroutine = null; 
    }

    private void SpawnItemPrefab()
    {
        if (currentItemData.itemPrefab != null)
        {
            Vector3 spawnPos = transform.position;

            if (currentItemData.itemType == ItemType.Hook)
            {
                spawnPos.y += 0.2f;
                spawnPos.x += playerMovement.facingDirection * 0.5f;
            }
            else if (currentItemData.itemType == ItemType.Cobweb || currentItemData.itemType == ItemType.Decoy)
            {
                spawnPos.x -= playerMovement.facingDirection * 1.5f;
            }

            GameObject spawnedItem = Instantiate(currentItemData.itemPrefab, spawnPos, Quaternion.identity);

            CobwebTrap cobwebScript = spawnedItem.GetComponent<CobwebTrap>();
            if (cobwebScript != null) cobwebScript.Initialize(currentItemData);

            HomingProjectile projectileScript = spawnedItem.GetComponent<HomingProjectile>();
            if (projectileScript != null) projectileScript.Initialize(currentItemData, playerMovement.facingDirection);

            GrapplingHook hookScript = spawnedItem.GetComponent<GrapplingHook>();
            if (hookScript != null) hookScript.Initialize(currentItemData, transform, playerMovement);

            DecoyOrb decoyScript = spawnedItem.GetComponent<DecoyOrb>();
            if (decoyScript != null) decoyScript.Initialize(currentItemData);
        }
    }
}