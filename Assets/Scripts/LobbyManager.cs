using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class LobbyManager : MonoBehaviour
{
    [Header("UI Diagramas Jugador 1")]
    public GameObject p1KeyboardDiagram;
    public GameObject p1GamepadDiagram;

    [Header("UI Diagramas Jugador 2")]
    public GameObject p2KeyboardDiagram;
    public GameObject p2GamepadDiagram;

    [Header("Configuración del Tiempo")]
    public TMP_InputField timeInputField;
    
    [Header("Jugadores en la sala de prueba")]
    public PlayerMovement p1Movement;
    public PlayerMovement p2Movement;

    [Header("Navegación")]
    public string firstMapName = "Mapa1";
    public string mainMenuName = "MainMenu";

    void Start()
    {
        if (GameManager.instance != null)
        {
            timeInputField.text = GameManager.instance.survivalTime.ToString();
        }
        
        if (p1Movement != null && p1Movement.TryGetComponent<PlayerRole>(out var r1)) r1.SetRole(Role.Hero);
        if (p2Movement != null && p2Movement.TryGetComponent<PlayerRole>(out var r2)) r2.SetRole(Role.Banner);
        
        SetPlayer1Input("Keyboard_P1");
        SetPlayer2Input("Keyboard_P2");
    }
    
    public void SetPlayer1Input(string scheme)
    {
        if (GameManager.instance != null) GameManager.instance.p1ControlScheme = scheme;
        
        bool isKeyboard = (scheme == "Keyboard_P1");
        p1KeyboardDiagram.SetActive(isKeyboard);
        p1GamepadDiagram.SetActive(!isKeyboard);

        if (p1Movement != null)
        {
            var pInput = p1Movement.GetComponent<PlayerInput>();
            pInput.user.UnpairDevices();

            if (isKeyboard)
            {
                pInput.SwitchCurrentControlScheme("Keyboard_P1", Keyboard.current);
            }
            else if (Gamepad.all.Count > 0)
            {
                pInput.SwitchCurrentControlScheme("Gamepad", Gamepad.all[0]);
            }
        }
    }

    public void SetPlayer2Input(string scheme)
    {
        if (GameManager.instance != null) GameManager.instance.p2ControlScheme = scheme;
        
        bool isKeyboard = (scheme == "Keyboard_P2");
        p2KeyboardDiagram.SetActive(isKeyboard);
        p2GamepadDiagram.SetActive(!isKeyboard);

        if (p2Movement != null)
        {
            var pInput = p2Movement.GetComponent<PlayerInput>();
            pInput.user.UnpairDevices(); // Desvincula controles viejos

            if (isKeyboard)
            {
                pInput.SwitchCurrentControlScheme("Keyboard_P2", Keyboard.current);
            }
            else 
            {
                if (Gamepad.all.Count > 1)
                {
                    pInput.SwitchCurrentControlScheme("Gamepad", Gamepad.all[1]);
                }
                else if (Gamepad.all.Count == 1)
                {
                    pInput.SwitchCurrentControlScheme("Gamepad", Gamepad.all[0]);
                }
            }
        }
    }

    public void PlayGame()
    {
        if (float.TryParse(timeInputField.text, out float newTime))
        {
            if (GameManager.instance != null) GameManager.instance.survivalTime = newTime;
        }

        SceneManager.LoadScene(firstMapName);
    }

    public void GoBack()
    {
        SceneManager.LoadScene(mainMenuName);
    }
}