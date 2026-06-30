using UnityEngine;

// Definimos todos los tipos de ítems posibles
public enum ItemType { Dash, StunProjectile, Hook, Cobweb, SpeedPotion, Decoy }

[CreateAssetMenu(fileName = "NewItemData", menuName = "PartyGame/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Datos Visuales e Identidad")]
    public ItemType itemType;
    public string itemName;
    public Sprite itemIcon;

    [Header("Configuración de Valores (Balance)")]
    [Tooltip("Uso: Duración del Stun, de la poción de velocidad, o de la ralentización de la telaraña.")]
    public float effectDuration;
    
    [Tooltip("Uso: Fuerza del Dash, Multiplicador de velocidad de la poción, o Velocidad del proyectil.")]
    public float mainPower;
    
    [Tooltip("Uso: Fuerza de Knockback del señuelo, o distancia máxima del gancho.")]
    public float secondaryPower;

    [Header("Prefabs Físicos (Dejar vacío si no aplica)")]
    [Tooltip("El objeto que se va a instanciar en el mapa (El misil, la trampa, el orbe falso, etc.).")]
    public GameObject itemPrefab;
}