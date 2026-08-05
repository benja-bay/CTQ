using System.Collections;
using UnityEngine;

public class GhostTrail : MonoBehaviour
{
    [Header("Configuración del Ghost")]
    [SerializeField] private float ghostLifetime = 0.5f;
    [SerializeField] private float spawnInterval = 0.05f;
    [SerializeField] private Material ghostMaterial;
    [SerializeField] private Color ghostColor = new Color(0f, 0.7f, 1f, 0.8f);

    private SpriteRenderer playerSprite;
    private bool isTrailActive = false;
    private float spawnTimer;

    private void Update()
    {
        if (isTrailActive)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                SpawnGhost();
                spawnTimer = 0f;
            }
        }
    }

    public void SetTrailActive(bool active)
    {
        isTrailActive = active;
        
        if (active)
        {
            // Actualizamos la referencia del sprite justo antes de empezar a dejar el rastro
            UpdateActiveSprite();
            spawnTimer = spawnInterval; // Fuerza a crear el primer fantasma al instante
        }
    }

    // Busca todos los SpriteRenderers hijos y se queda con el que esté activo en este momento
    private void UpdateActiveSprite()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(false);
        foreach (var sr in renderers)
        {
            if (sr.gameObject.activeInHierarchy)
            {
                playerSprite = sr;
                return;
            }
        }
    }

    private void SpawnGhost()
    {
        // Control de seguridad por si no encontró un sprite activo
        if (playerSprite == null) return;

        GameObject ghostObj = new GameObject("GhostFrame");
        ghostObj.transform.position = playerSprite.transform.position;
        ghostObj.transform.rotation = playerSprite.transform.rotation;
        ghostObj.transform.localScale = playerSprite.transform.localScale;

        SpriteRenderer ghostSr = ghostObj.AddComponent<SpriteRenderer>();
        ghostSr.sprite = playerSprite.sprite;
        ghostSr.flipX = playerSprite.flipX;
        ghostSr.flipY = playerSprite.flipY;
        ghostSr.sortingLayerID = playerSprite.sortingLayerID;
        ghostSr.sortingOrder = playerSprite.sortingOrder - 1;

        if (ghostMaterial != null)
        {
            ghostSr.material = ghostMaterial;
        }

        StartCoroutine(FadeAndDestroyGhost(ghostObj, ghostSr));
    }

    private IEnumerator FadeAndDestroyGhost(GameObject ghostObj, SpriteRenderer ghostSr)
    {
        float elapsed = 0f;
        Material matInstance = ghostSr.material;
        
        if (matInstance != null && matInstance.HasProperty("_GhostColor"))
        {
            matInstance.SetColor("_GhostColor", ghostColor);
        }

        while (elapsed < ghostLifetime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / ghostLifetime);
            
            if (matInstance != null && matInstance.HasProperty("_Alpha"))
            {
                matInstance.SetFloat("_Alpha", alpha);
            }
            else
            {
                // Fallback por si el material no tiene la propiedad _Alpha
                Color c = ghostSr.color;
                c.a = alpha;
                ghostSr.color = c;
            }
            
            yield return null;
        }

        Destroy(ghostObj);
    }
}