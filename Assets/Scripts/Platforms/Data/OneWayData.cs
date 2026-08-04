using UnityEngine;

namespace Platforms.Data
{
    public enum PassType 
    {
        Solid,
        PassFromBottom,
        PassFromTop
    }

    [CreateAssetMenu(fileName = "OneWayData", menuName = "CTQ/Platforms/OneWay Data")]
    public class OneWayData : PlatformModuleData
    {
        [Header("Colisión")]
        public PassType passType = PassType.PassFromBottom;
    
        [Header("Drop-down")]
        [Tooltip("Permite al jugador ignorar la colisión al presionar Abajo")]
        public bool allowDropDown = true;
        public float dropDownTime = 0.35f;
    }
}