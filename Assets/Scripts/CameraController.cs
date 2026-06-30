using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Jugadores a Seguir")]
    public List<Transform> targets;

    [Header("Movimiento y Suavizado")]
    public Vector3 offset = new Vector3(0f, 0f, -10f); // -10 en Z para que la cámara no se meta al 2D
    public float smoothTime = 0.3f;
    private Vector3 velocity;

    [Header("Zoom Dinámico")]
    public float minZoom = 5f;   // Lo más cerca que puede estar
    public float maxZoom = 10f;  // Lo más lejos que puede estar
    public float zoomLimiter = 20f; // Qué tan rápido llega al maxZoom (ajústalo según tu mapa)

    [Header("Límites del Mapa (Boundaries)")]
    public bool useBounds = true;
    public Vector2 minBounds; // Esquina inferior izquierda del mapa
    public Vector2 maxBounds; // Esquina superior derecha del mapa

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        // Si la lista está vacía, buscamos a los jugadores automáticamente (ideal para cambio de mapas)
        if (targets.Count == 0)
        {
            GameObject p1 = GameObject.Find("Player1");
            GameObject p2 = GameObject.Find("Player2");
            if (p1 != null) targets.Add(p1.transform);
            if (p2 != null) targets.Add(p2.transform);
        }
    }

    // Usamos LateUpdate para la cámara. Esto asegura que los jugadores ya se movieron en Update/FixedUpdate antes de que la cámara los siga (evita tirones)
    void LateUpdate()
    {
        if (targets.Count == 0) return;

        Move();
        Zoom();
    }

    void Move()
    {
        Vector3 centerPoint = GetCenterPoint();
        Vector3 newPosition = centerPoint + offset;

        // --- SISTEMA DE LÍMITES ---
        if (useBounds)
        {
            // Calculamos cuánto mide la cámara actualmente para no salirnos del borde
            float camHeight = cam.orthographicSize;
            float camWidth = cam.orthographicSize * cam.aspect;

            float minX = minBounds.x + camWidth;
            float maxX = maxBounds.x - camWidth;
            float minY = minBounds.y + camHeight;
            float maxY = maxBounds.y - camHeight;

            // Prevenir errores si el mapa es más chico que la cámara
            if (minX > maxX) minX = maxX = centerPoint.x;
            if (minY > maxY) minY = maxY = centerPoint.y;

            // Clampeamos (limitamos) la posición
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        }

        // Movemos la cámara suavemente
        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
    }

    void Zoom()
    {
        float greatestDistance = GetGreatestDistance();
        
        // Interpola entre el zoom mínimo y máximo basado en la distancia de los jugadores
        float newZoom = Mathf.Lerp(minZoom, maxZoom, greatestDistance / zoomLimiter);
        
        // Aplica el zoom de forma fluida
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoom, Time.deltaTime * 3f);
    }

    float GetGreatestDistance()
    {
        // Crea una "caja" imaginaria que envuelve a ambos jugadores
        Bounds bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }
        
        // Devuelve el lado más largo de la caja (ideal para mapas verticales u horizontales)
        return Mathf.Max(bounds.size.x, bounds.size.y);
    }

    Vector3 GetCenterPoint()
    {
        if (targets.Count == 1) return targets[0].position;

        Bounds bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }
        return bounds.center;
    }

    // Dibuja un rectángulo amarillo en el editor para que veas los límites fácilmente
    void OnDrawGizmos()
    {
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2, (minBounds.y + maxBounds.y) / 2, 0);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 1);
            Gizmos.DrawWireCube(center, size);
        }
    }
}