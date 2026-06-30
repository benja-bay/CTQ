using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Textos")]
    public TextMeshProUGUI timeText;

    [Header("Sprites de Avatares Base")]
    public Sprite heroSprite;
    public Sprite bannerSprite;

    [Header("Sprites de Puntos")]
    public Sprite catchPointSprite;   // La banderita roja
    public Sprite catchMarkerSprite;  // La X roja (Tachado)
    public Sprite escapePointSprite;  // El tipito corriendo
    public Sprite escapeMarkerSprite; // El tilde verde (Check)

    [Header("UI Player 1 (Izquierda)")]
    public Image p1AvatarImage;
    public Image p1ItemIconImage;
    public Image[] p1PointBases;      // Los 3 cuadraditos de fondo del P1
    public Image[] p1PointMarkers;    // Las 3 cruces o tildes del P1

    [Header("UI Player 2 (Derecha)")]
    public Image p2AvatarImage;
    public Image p2ItemIconImage;
    public Image[] p2PointBases;      // Los 3 cuadraditos de fondo del P2
    public Image[] p2PointMarkers;    // Las 3 cruces o tildes del P2

    // Variables internas
    private PlayerInventory p1Inventory;
    private PlayerRole p1Role;
    private PlayerInventory p2Inventory;
    private PlayerRole p2Role;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.root.gameObject); 
            SceneManager.sceneLoaded += OnMapLoaded;
        }
        else
        {
            Destroy(transform.root.gameObject);
            return;
        }
    }

    void Start()
    {
        FindPlayersInScene();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnMapLoaded;
    }

    void OnMapLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayersInScene();
    }

    void FindPlayersInScene()
    {
        GameObject p1 = GameObject.Find("Player1");
        if (p1 != null)
        {
            p1Inventory = p1.GetComponent<PlayerInventory>();
            p1Role = p1.GetComponent<PlayerRole>();
        }

        GameObject p2 = GameObject.Find("Player2");
        if (p2 != null)
        {
            p2Inventory = p2.GetComponent<PlayerInventory>();
            p2Role = p2.GetComponent<PlayerRole>();
        }
    }

    void Update()
    {
        if (GameManager.instance == null) return;

        // ==========================================
        // RELOJ
        // ==========================================
        float time = GameManager.instance.GetRemainingTime();
        if (timeText != null) timeText.text = Mathf.CeilToInt(time).ToString();

        int heroPts = GameManager.instance.GetHeroPoints();
        int bannerPts = GameManager.instance.GetBannerPoints();

        // ==========================================
        // ACTUALIZACIÓN JUGADOR 1 (IZQUIERDA)
        // ==========================================
        if (p1Role != null)
        {
            // Avatares
            if (p1AvatarImage != null)
            {
                if (p1Role.currentRole == Role.Hero)
                {
                    p1AvatarImage.sprite = heroSprite;
                    p1AvatarImage.rectTransform.localScale = new Vector3(1f, 1f, 1f);
                }
                else
                {
                    p1AvatarImage.sprite = bannerSprite;
                    p1AvatarImage.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
                }
            }

            // Sistema de Puntos P1
            if (p1Role.currentRole == Role.Hero)
            {
                UpdatePointsUI(p1PointBases, p1PointMarkers, catchPointSprite, catchMarkerSprite, heroPts);
            }
            else
            {
                UpdatePointsUI(p1PointBases, p1PointMarkers, escapePointSprite, escapeMarkerSprite, bannerPts);
            }
        }

        if (p1Inventory != null && p1ItemIconImage != null)
        {
            p1ItemIconImage.sprite = p1Inventory.hasItem ? p1Inventory.currentItemData.itemIcon : null;
            p1ItemIconImage.enabled = p1Inventory.hasItem;
        }

        // ==========================================
        // ACTUALIZACIÓN JUGADOR 2 (DERECHA)
        // ==========================================
        if (p2Role != null)
        {
            // Avatares
            if (p2AvatarImage != null)
            {
                if (p2Role.currentRole == Role.Banner)
                {
                    p2AvatarImage.sprite = bannerSprite;
                    p2AvatarImage.rectTransform.localScale = new Vector3(1f, 1f, 1f);
                }
                else
                {
                    p2AvatarImage.sprite = heroSprite;
                    p2AvatarImage.rectTransform.localScale = new Vector3(-1f, 1f, 1f);
                }
            }

            // Sistema de Puntos P2
            if (p2Role.currentRole == Role.Hero)
            {
                UpdatePointsUI(p2PointBases, p2PointMarkers, catchPointSprite, catchMarkerSprite, heroPts);
            }
            else
            {
                UpdatePointsUI(p2PointBases, p2PointMarkers, escapePointSprite, escapeMarkerSprite, bannerPts);
            }
        }

        if (p2Inventory != null && p2ItemIconImage != null)
        {
            p2ItemIconImage.sprite = p2Inventory.hasItem ? p2Inventory.currentItemData.itemIcon : null;
            p2ItemIconImage.enabled = p2Inventory.hasItem;
        }
    }

    // Función auxiliar para actualizar las imágenes de los puntos fácilmente
    private void UpdatePointsUI(Image[] bases, Image[] markers, Sprite baseSprite, Sprite markerSprite, int earnedPoints)
    {
        for (int i = 0; i < bases.Length; i++)
        {
            if (bases[i] != null) 
            {
                // Mostramos el ícono base en el Horizontal Layout de atrás
                bases[i].sprite = baseSprite;
                bases[i].color = Color.white; 
            }

            if (markers[i] != null)
            {
                // Asignamos la marca en el Horizontal Layout de adelante
                markers[i].sprite = markerSprite;
                
                // Prendemos la imagen de tachado solo si ya ganaron este punto
                markers[i].enabled = (i < earnedPoints);
            }
        }
    }
}