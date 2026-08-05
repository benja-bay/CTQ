using System.Collections;
using Platforms.Data;
using Player;
using UnityEngine;

namespace Platforms.Modules
{
    public class FadingModule : IPlatformModule
    {
        private PlatformController controller;
        private FadingData data;
        private bool isTriggered = false;

        public void Initialize(PlatformController controller, PlatformModuleData data)
        {
            this.controller = controller;
            this.data = data as FadingData;
        }

        public void OnUpdate() { }

        public void OnPlayerEnter(PlayerMovement player)
        {
            if (!isTriggered)
            {
                isTriggered = true;
                controller.StartCoroutine(FadingRoutine());
            }
        }

        public void OnPlayerStay(PlayerMovement player) { }
        public void OnPlayerExit(PlayerMovement player) { }

        private IEnumerator FadingRoutine()
        {
            Transform platTransform = controller.transform;
            Vector3 originalPosition = platTransform.position;
            float elapsed = 0f;
        
            while (elapsed < data.supportTime)
            {
                elapsed += Time.deltaTime;
                platTransform.position = originalPosition + (Vector3)(Random.insideUnitCircle * data.shakeIntensity);
                yield return null;
            }
        
            platTransform.position = originalPosition;
        
            controller.platformCollider.enabled = false;
            elapsed = 0f;
        
            Color startColor = controller.spriteRenderer.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            while (elapsed < data.fadeTime)
            {
                elapsed += Time.deltaTime;
                controller.spriteRenderer.color = Color.Lerp(startColor, endColor, elapsed / data.fadeTime);
                yield return null;
            }
        
            yield return new WaitForSeconds(data.respawnTime);
        
            controller.spriteRenderer.color = startColor;
            controller.platformCollider.enabled = true;
            isTriggered = false;
        }
    }
}