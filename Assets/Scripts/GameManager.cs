using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Configuración de Mapas")]
    public List<int> mapPool; 
    public int mapsPerMatch = 3;
    private List<int> selectedMaps = new List<int>();
    private int currentRoundIndex = 0;

    [Header("Configuración del Tiempo")]
    [Tooltip("Tiempo en segundos que el Banderín debe sobrevivir para ganar la ronda")]
    public float survivalTime = 30f; 
    private float remainingTime;
    private bool isRoundActive = false;

    [Header("Puntaje de la Partida")]
    public int pointsToWinMatch = 3;
    private int heroPoints = 0;
    private int bannerPoints = 0;

    [Header("Roles Actuales")]
    public Role player1Role = Role.Hero;
    public Role player2Role = Role.Banner;

    private PlayerRole player1;
    private PlayerRole player2;
    private Transform heroSpawn;
    private Transform bannerSpawn;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnMapLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (selectedMaps.Count == 0)
        {
            GenerateNewMatch();
        }
    }

    void Update()
    {
        if (isRoundActive)
        {
            remainingTime -= Time.deltaTime;
            

            if (remainingTime <= 0)
            {
                isRoundActive = false;
                BannerSurvived();
            }
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnMapLoaded;
    }
    
    void GenerateNewMatch()
    {
        selectedMaps.Clear();
        currentRoundIndex = 0;

        List<int> availableMaps = new List<int>(mapPool);
        
        for (int i = 0; i < mapsPerMatch; i++)
        {
            if (availableMaps.Count == 0) break;
            int randomIndex = Random.Range(0, availableMaps.Count);
            selectedMaps.Add(availableMaps[randomIndex]);
            availableMaps.RemoveAt(randomIndex);
        }

        Debug.Log($"New match started. Points required: {pointsToWinMatch}");
    }

    void OnMapLoaded(Scene scene, LoadSceneMode mode)
    {
        FindElementsInMap();
        SpawnAndAssignRoles();
        
        if (player1 != null && player1.TryGetComponent<PlayerMovement>(out var pm1)) pm1.canMove = false;
        if (player2 != null && player2.TryGetComponent<PlayerMovement>(out var pm2)) pm2.canMove = false;
        
        if (UIManager.instance != null)
        {
            UIManager.instance.StartCountdown();
        }
    }

    void FindElementsInMap()
    {
        player1 = GameObject.Find("Player1")?.GetComponent<PlayerRole>();
        player2 = GameObject.Find("Player2")?.GetComponent<PlayerRole>();
        heroSpawn = GameObject.Find("HeroSpawn")?.transform;
        bannerSpawn = GameObject.Find("BannerSpawn")?.transform;
    }

    void SpawnAndAssignRoles()
    {
        if (player1 == null || player2 == null || heroSpawn == null || bannerSpawn == null) return;

        player1.SetRole(player1Role);
        player2.SetRole(player2Role);

        if (player1.currentRole == Role.Hero) player1.transform.position = heroSpawn.position;
        else player1.transform.position = bannerSpawn.position;

        if (player2.currentRole == Role.Hero) player2.transform.position = heroSpawn.position;
        else player2.transform.position = bannerSpawn.position;

        if (player1.TryGetComponent<Rigidbody2D>(out var rb1)) rb1.linearVelocity = Vector2.zero;
        if (player2.TryGetComponent<Rigidbody2D>(out var rb2)) rb2.linearVelocity = Vector2.zero;
    }
    
    public void BeginRoundAfterCountdown()
    {
        if (player1 != null && player1.TryGetComponent<PlayerMovement>(out var pm1)) pm1.canMove = true;
        if (player2 != null && player2.TryGetComponent<PlayerMovement>(out var pm2)) pm2.canMove = true;
        
        remainingTime = survivalTime;
        isRoundActive = true;
    }

    // --- CONDICIONES DE VICTORIA DE RONDA ---
    public void HeroCatchesBanner()
    {
        if (!isRoundActive) return; // Evita doble activación
        isRoundActive = false;

        heroPoints++;
        Debug.Log($"Point for Hero! Score: Hero {heroPoints} - Banner {bannerPoints}");

        CheckMatchWinner();
    }
    
    void BannerSurvived()
    {
        bannerPoints++;
        Debug.Log($"Time's up! Point for Banner. Score: Hero {heroPoints} - Banner {bannerPoints}");

        CheckMatchWinner();
    }

    void CheckMatchWinner()
    {
        if (heroPoints >= pointsToWinMatch)
        {
            Debug.Log("Hero won the match! Swapping roles for the next match.");
            EndMatch();
        }
        else if (bannerPoints >= pointsToWinMatch)
        {
            Debug.Log("Banner won the match! Swapping roles for the next match.");
            EndMatch();
        }
        else
        {
            AdvanceToNextMap();
        }
    }

    void EndMatch()
    {
        heroPoints = 0;
        bannerPoints = 0;
        
        player1Role = (player1Role == Role.Hero) ? Role.Banner : Role.Hero;
        player2Role = (player2Role == Role.Hero) ? Role.Banner : Role.Hero;
        
        GenerateNewMatch();
        LoadSelectedMap();
    }

    void AdvanceToNextMap()
    {
        currentRoundIndex++;
        
        if (currentRoundIndex >= selectedMaps.Count)
        {
            GenerateNewMatch();
        }

        LoadSelectedMap();
    }

    void LoadSelectedMap()
    {
        SceneManager.LoadScene(selectedMaps[currentRoundIndex]);
    }
    
    public float GetRemainingTime()
    {
        return Mathf.Max(0, remainingTime);
    }
    
    public int GetHeroPoints()
    {
        return heroPoints;
    }

    public int GetBannerPoints()
    {
        return bannerPoints;
    }
}