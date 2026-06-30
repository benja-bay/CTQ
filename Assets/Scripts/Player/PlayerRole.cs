using UnityEngine;

public enum Role { Hero, Banner }

public class PlayerRole : MonoBehaviour
{
    public Role currentRole;
    
    [Header("Visuales (Hijos)")]
    public GameObject heroVisuals;
    public GameObject flagVisuals;
    
    private PlayerMovement movement;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    // Configura el personaje según el rol que le asigne el GameManager
    public void SetRole(Role newRole)
    {
        currentRole = newRole;

        if (currentRole == Role.Hero)
        {
            gameObject.tag = "Hero";
            movement.moveSpeed = 8f;
            
            // Prendemos al Héroe y apagamos al Banderín
            if (heroVisuals != null) heroVisuals.SetActive(true);
            if (flagVisuals != null) flagVisuals.SetActive(false);
        }
        else
        {
            gameObject.tag = "Banner";
            movement.moveSpeed = 9f; // El banderín es un poco más rápido por defecto
            
            // Prendemos al Banderín y apagamos al Héroe
            if (heroVisuals != null) heroVisuals.SetActive(false);
            if (flagVisuals != null) flagVisuals.SetActive(true);
        }
    }

    // Detectar colisión entre jugadores
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentRole == Role.Hero && collision.gameObject.CompareTag("Banner"))
        {
            Debug.Log("Hero caught the Banner!");
            GameManager.instance.HeroCatchesBanner();
        }
    }
}