using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;

    [Header("Configuración")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    void Start()
    {
        // Nos aseguramos de que el panel esté apagado y el tiempo corriendo al iniciar
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        // Al presionar Escape, alternamos la pausa (se abre o se cierra)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame(); // Si ya estaba pausado, lo cierra y reanuda el tiempo
            }
            else
            {
                PauseGame(); // Si no estaba pausado, lo abre y congela el tiempo
            }
        }
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f; // Congela el tiempo (físicas, animaciones, timers)
        isPaused = true;
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f; // Descongela el tiempo
        isPaused = false;
    }

    public void LoadMainMenu()
    {
        // 1. IMPORTANTÍSIMO: Devolver el tiempo a la normalidad antes de salir
        // Si no hacemos esto, el menú principal también estará congelado.
        Time.timeScale = 1f; 

        // 2. Destruimos los objetos inmortales para reiniciar la arquitectura
        if (GameManager.instance != null) 
        {
            Destroy(GameManager.instance.gameObject);
        }
        
        // 3. Cargamos el menú principal
        SceneManager.LoadScene(mainMenuSceneName);
        
        // 4. Finalmente, nos destruimos a nosotros mismos (el Canvas) 
        // para que no aparezca flotando encima del Menú Principal
        Destroy(transform.root.gameObject); 
    }
}