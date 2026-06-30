using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    [Header("Referencias Visuales")]
    public Transform head;
    public SpriteRenderer ropeRenderer;

    [Header("Ajustes de Arrastre")]
    [Tooltip("Distancia que arrastra al Banderín. Si es 0, lo atrae todo el camino.")]
    public float bannerDragDistance = 0f;

    private float speed;
    private float maxDistance;
    private float pullSpeed = 20f; 
    private float currentDragDistance = 0f;

    private Transform playerTransform;
    private PlayerMovement playerMovement;
    private Rigidbody2D playerRb;
    private float originalGravity;
    
    private PlayerMovement bannerMovement;

    private float facingDirection;
    private enum HookState { Extending, PullingPlayer, PullingBanner, Returning }
    private HookState currentState = HookState.Extending;

    public void Initialize(ItemData data, Transform player, PlayerMovement pm)
    {
        speed = data.mainPower; 
        maxDistance = data.secondaryPower; 
        
        playerTransform = player;
        playerMovement = pm;
        facingDirection = pm.facingDirection;

        playerMovement.canMove = false;
        if (player.TryGetComponent<Rigidbody2D>(out playerRb))
        {
            originalGravity = playerRb.gravityScale;
            playerRb.gravityScale = 0f;
            playerRb.linearVelocity = Vector2.zero;
        }

        if (facingDirection < 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 180f);
        }
        
        // Desvinculamos para evitar el patinaje
        head.parent = null; 
        ropeRenderer.transform.parent = null;
    }

    void Update()
    {
        Vector3 pPos = playerTransform.position;
        Vector3 hPos = head.position;

        float distance = Vector2.Distance(pPos, hPos);
        ropeRenderer.size = new Vector2(distance, ropeRenderer.size.y);
        ropeRenderer.transform.position = (pPos + hPos) / 2f;
        
        Vector3 direction = hPos - pPos;
        if (direction != Vector3.zero)
        {
            ropeRenderer.transform.right = direction.normalized;
        }
    }

    void FixedUpdate()
    {
        switch (currentState)
        {
            case HookState.Extending:
                head.Translate(Vector3.right * speed * Time.fixedDeltaTime, Space.Self);
                
                if (Vector2.Distance(transform.position, head.position) >= maxDistance)
                    currentState = HookState.Returning;
                break;

            case HookState.Returning:
                head.position = Vector2.MoveTowards(head.position, playerTransform.position, speed * Time.fixedDeltaTime);
                if (Vector2.Distance(playerTransform.position, head.position) < 0.2f)
                    FinishHook();
                break;

            case HookState.PullingPlayer:
                Vector2 pullDir = (head.position - playerTransform.position).normalized;
                
                playerRb.MovePosition(playerRb.position + pullDir * pullSpeed * Time.fixedDeltaTime);

                ContactFilter2D filter = new ContactFilter2D { layerMask = LayerMask.GetMask("Ground"), useLayerMask = true };
                RaycastHit2D[] hitResults = new RaycastHit2D[1];
                
                int wallHits = playerRb.Cast(pullDir, filter, hitResults, 0.1f);

                if (wallHits > 0 || Vector2.Distance(playerTransform.position, head.position) < 0.8f)
                {
                    FinishHook();
                }
                break;

            case HookState.PullingBanner:
                Vector3 previousHeadPos = head.position;
                head.position = Vector2.MoveTowards(head.position, playerTransform.position, pullSpeed * Time.fixedDeltaTime);
                
                // Sumamos cuánto arrastramos la cabeza en este exacto frame
                currentDragDistance += Vector2.Distance(previousHeadPos, head.position);
                
                if (bannerMovement != null)
                {
                    bannerMovement.transform.position = head.position;
                }

                //  Llegó hasta el Héroe
                if (Vector2.Distance(playerTransform.position, head.position) < 0.5f)
                {
                    FinishHook();
                }
                //  Llegó al límite de arrastre
                else if (bannerDragDistance > 0f && currentDragDistance >= bannerDragDistance)
                {
                    // Soltamos al Banderín en el aire
                    if (bannerMovement != null)
                    {
                        bannerMovement.canMove = true; 
                        bannerMovement = null;
                    }
                    // La soga y la garra vuelven vacías hacia el Héroe
                    currentState = HookState.Returning; 
                }
                break;
        }
    }

    public void HandleCollision(Collider2D other)
    {
        if (currentState != HookState.Extending) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            currentState = HookState.PullingPlayer;
        }
        else if (other.CompareTag("Banner"))
        {
            currentState = HookState.PullingBanner;
            bannerMovement = other.GetComponent<PlayerMovement>();
            currentDragDistance = 0f;
            
            if (bannerMovement != null)
            {
                bannerMovement.canMove = false; 
                if (bannerMovement.TryGetComponent<Rigidbody2D>(out var bannerRb))
                    bannerRb.linearVelocity = Vector2.zero;
            }
        }
    }

    private void FinishHook()
    {
        if (playerRb != null) playerRb.gravityScale = originalGravity;
        playerMovement.canMove = true;
        
        if (bannerMovement != null) bannerMovement.canMove = true;

        if (head != null) Destroy(head.gameObject);
        if (ropeRenderer != null) Destroy(ropeRenderer.gameObject);
        Destroy(gameObject);
    }
}