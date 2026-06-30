using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cambiar de escena

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneles del Menú")]
    public GameObject mainPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("Configuración de Escena")]
    [Tooltip("Escribe el nombre EXACTO de la escena donde empieza tu juego")]
    public string startingSceneName = "Mapa1"; // Cambia esto por el nombre de tu mapa base o GameManager scene

    void Start()
    {
        // Al iniciar, nos aseguramos de que solo el panel principal esté visible
        OpenMainPanel();
    }

    // ==========================================
    // FUNCIONES DE LOS BOTONES
    // ==========================================

    public void StartGame()
    {
        // Carga la escena del juego
        SceneManager.LoadScene(startingSceneName);
    }

    public void OpenOptions()
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(true);
        creditsPanel.SetActive(false);
    }

    public void OpenCredits()
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void OpenMainPanel()
    {
        mainPanel.SetActive(true);
        optionsPanel.SetActive(false);
        creditsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        // Esto solo se verá en la consola del editor de Unity
        Debug.Log("Cerrando el juego...");
        
        // Esto cerrará el juego real cuando lo exportes (.exe)
        Application.Quit();
    }
}