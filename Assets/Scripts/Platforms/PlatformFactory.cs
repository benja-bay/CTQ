using Platforms.Data;
using Platforms.Modules;
using UnityEngine;

namespace Platforms
{
    public static class PlatformFactory
    {
        public static IPlatformModule CreateModule(PlatformModuleData data)
        {
            if (data is OneWayData) return new OneWayModule();
            if (data is FadingData) return new FadingModule();
        
            Debug.LogWarning($"[PlatformFactory] Módulo no reconocido: {data.GetType()}");
            return null;
        }
    }
}