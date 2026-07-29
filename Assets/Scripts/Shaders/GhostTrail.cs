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

    private void Awake()
    {
        playerSprite = GetComponentInChildren<SpriteRenderer>();
    }

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
    }

    private void SpawnGhost()
    {
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
        matInstance.SetColor("_GhostColor", ghostColor);

        while (elapsed < ghostLifetime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / ghostLifetime);

            matInstance.SetFloat("_Alpha", alpha);

            yield return null;
        }

        Destroy(ghostObj);
    }
}