using UnityEngine;

namespace Platforms.Data
{
    [CreateAssetMenu(fileName = "FadingData", menuName = "CTQ/Platforms/Fading Data")]
    public class FadingData : PlatformModuleData
    {
        public float supportTime = 1.25f;
        public float shakeIntensity = 0.05f;
        public float fadeTime = 0.3f;
        public float respawnTime = 2.25f;
    }
}