using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Configuración de Parallax")]
    [Tooltip("1 = Se mueve igual que la cámara (Cielo lejano). 0 = Se queda quieto en el mundo (Suelo). Valores entre 0 y 1 para capas intermedias.")]
    public Vector2 parallaxMultiplier;

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
    }
    
    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        
        transform.position += new Vector3(deltaMovement.x * parallaxMultiplier.x, deltaMovement.y * parallaxMultiplier.y, 0f);
        
        lastCameraPosition = cameraTransform.position;
    }
}