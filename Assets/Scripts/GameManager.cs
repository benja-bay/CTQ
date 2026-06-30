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
        // Si la ronda está corriendo, descontamos tiempo
        if (isRoundActive)
        {
            remainingTime -= Time.deltaTime;

            // Opcional: Aquí podrías actualizar un texto en la pantalla con el tiempo
            // Debug.Log("Remaining time: " + Mathf.CeilToInt(remainingTime));

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

    // --- LÓGICA DE PARTIDA Y MAPAS ---
    void GenerateNewMatch()
    {
        selectedMaps.Clear();
        currentRoundIndex = 0;

        List<int> availableMaps = new List<int>(mapPool);

        // Llenamos la lista con mapas aleatorios sin repetir
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

        // Al iniciar el mapa, reseteamos el reloj y activamos la ronda
        remainingTime = survivalTime;
        isRoundActive = true;
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

    // --- CONDICIONES DE VICTORIA DE RONDA ---

    // ACCIÓN: El héroe atrapa al banderín
    public void HeroCatchesBanner()
    {
        if (!isRoundActive) return; // Evita doble activación
        isRoundActive = false;

        heroPoints++;
        Debug.Log($"Point for Hero! Score: Hero {heroPoints} - Banner {bannerPoints}");

        CheckMatchWinner();
    }

    // ACCIÓN: El tiempo se terminó y el banderín sigue vivo
    void BannerSurvived()
    {
        bannerPoints++;
        Debug.Log($"Time's up! Point for Banner. Score: Hero {heroPoints} - Banner {bannerPoints}");

        CheckMatchWinner();
    }

    void CheckMatchWinner()
    {
        // 1. ¿El Héroe llegó a los 3 puntos?
        if (heroPoints >= pointsToWinMatch)
        {
            Debug.Log("Hero won the match! Swapping roles for the next match.");
            EndMatch();
        }
        // 2. ¿El Banderín llegó a los 3 puntos?
        else if (bannerPoints >= pointsToWinMatch)
        {
            Debug.Log("Banner won the match! Swapping roles for the next match.");
            EndMatch();
        }
        // 3. Nadie ganó todavía, pasamos al siguiente mapa aleatorio
        else
        {
            AdvanceToNextMap();
        }
    }

    void EndMatch()
    {
        // Reseteamos los puntos de la partida
        heroPoints = 0;
        bannerPoints = 0;

        // Invertimos los roles de los jugadores para el siguiente match completo
        player1Role = (player1Role == Role.Hero) ? Role.Banner : Role.Hero;
        player2Role = (player2Role == Role.Hero) ? Role.Banner : Role.Hero;

        // Barajamos un nuevo set de mapas aleatorios
        GenerateNewMatch();
        LoadSelectedMap();
    }

    void AdvanceToNextMap()
    {
        currentRoundIndex++;
        
        // Si por alguna razón nos quedamos sin mapas en la lista de 3 elegidos, volvemos a mezclar
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

    // Función pública para que la UI o tú puedas consultar el tiempo de forma limpia
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