using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Countdown UI")]
    public Image countdownImage;        
    public Sprite[] countdownSprites;   

    [Header("Victory UI")]
    public GameObject victoryPanel;
    public Image victoryTitleImage;
    public Sprite knightWinScreen;
    public Sprite flagWinScreen;

    [Header("Textos")]
    public TextMeshProUGUI timeText;

    [Header("Sprites de Avatares Base")]
    public Sprite heroSprite;
    public Sprite bannerSprite;

    [Header("Sprites de Puntos")]
    public Sprite catchPointSprite;   
    public Sprite catchMarkerSprite;  
    public Sprite escapePointSprite;  
    public Sprite escapeMarkerSprite; 

    [Header("UI Player 1 (Izquierda)")]
    public Image p1AvatarImage;
    public Image p1ItemIconImage;
    public Image[] p1PointBases;      
    public Image[] p1PointMarkers;    

    [Header("UI Player 2 (Derecha)")]
    public Image p2AvatarImage;
    public Image p2ItemIconImage;
    public Image[] p2PointBases;      
    public Image[] p2PointMarkers;    

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
        if (victoryPanel != null) victoryPanel.SetActive(false);
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

        float time = GameManager.instance.GetRemainingTime();
        if (timeText != null) timeText.text = Mathf.CeilToInt(time).ToString();

        int heroPts = GameManager.instance.GetHeroPoints();
        int bannerPts = GameManager.instance.GetBannerPoints();

        if (p1Role != null)
        {
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

        if (p2Role != null)
        {
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

    private void UpdatePointsUI(Image[] bases, Image[] markers, Sprite baseSprite, Sprite markerSprite, int earnedPoints)
    {
        for (int i = 0; i < bases.Length; i++)
        {
            if (bases[i] != null) 
            {
                bases[i].sprite = baseSprite;
                bases[i].color = Color.white; 
            }

            if (markers[i] != null)
            {
                markers[i].sprite = markerSprite;
                markers[i].enabled = (i < earnedPoints);
            }
        }
    }

    public void StartCountdown()
    {
        if (countdownImage == null || countdownSprites == null || countdownSprites.Length == 0)
        {
            GameManager.instance.BeginRoundAfterCountdown();
            return;
        }
        
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        countdownImage.gameObject.SetActive(true);

        for (int i = 0; i < countdownSprites.Length; i++)
        {
            countdownImage.sprite = countdownSprites[i];

            // Elegir sonido: el último sprite es "GO", los demás son números
            if (AudioManager.instance != null)
            {
                if (i == countdownSprites.Length - 1) AudioManager.instance.PlaySFX(AudioManager.instance.countdownGo);
                else AudioManager.instance.PlaySFX(AudioManager.instance.countdownBeep);
            }

            float elapsed = 0f;
            float popDuration = 0.15f; 
            
            while (elapsed < popDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / popDuration;
                countdownImage.rectTransform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, t);
                yield return null; 
            }

            countdownImage.rectTransform.localScale = Vector3.one;
            yield return new WaitForSeconds(1f - popDuration);
        }

        countdownImage.gameObject.SetActive(false);
        GameManager.instance.BeginRoundAfterCountdown();
    }

    public void ShowVictoryScreen(Role winner)
    {
        if (victoryPanel == null || victoryTitleImage == null) return;

        if (winner == Role.Hero)
        {
            victoryTitleImage.sprite = knightWinScreen;
        }
        else
        {
            victoryTitleImage.sprite = flagWinScreen;
        }

        victoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnRestartButtonPressed()
    {
        Time.timeScale = 1f;
        victoryPanel.SetActive(false);
        GameManager.instance.RestartMatch();
    }

    public void OnMainMenuButtonPressed()
    {
        Time.timeScale = 1f;
        if (GameManager.instance != null)
        {
            Destroy(GameManager.instance.gameObject);
        }
        Destroy(transform.root.gameObject);
        SceneManager.LoadScene("MainMenu");
    }
}