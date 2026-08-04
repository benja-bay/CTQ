using System.Collections.Generic;
using UnityEngine;

namespace Platforms.Data
{
    [CreateAssetMenu(fileName = "NewPlatformData", menuName = "PartyGame/Platforms/Platform Data")]
    public class PlatformData : ScriptableObject
    {
        [Tooltip("Lista de módulos que definen el comportamiento de esta plataforma.")]
        public List<PlatformModuleData> modules = new List<PlatformModuleData>();
    }
}