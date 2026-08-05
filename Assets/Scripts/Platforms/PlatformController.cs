using System.Collections.Generic;
using Platforms.Data;
using Platforms.Modules;
using Player;
using UnityEngine;

namespace Platforms
{
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer), typeof(PlatformEffector2D))]
    public class PlatformController : MonoBehaviour
    {
        [Header("Config")]
        public PlatformData platformData;
    
        public Collider2D platformCollider { get; private set; }
        public SpriteRenderer spriteRenderer { get; private set; }
        public PlatformEffector2D effector { get; private set; }
    
        private List<IPlatformModule> activeModules = new List<IPlatformModule>();

        [Header("Auto-Colisión")]
        [Tooltip("Píxeles de profundidad en la parte SUPERIOR")]
        public float topDepthPixelsToIgnore = 4f; 
        public float pixelsPerUnit = 16f;

        void Awake()
        {
            platformCollider = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            effector = GetComponent<PlatformEffector2D>();

            InitializeModules();
        }

        private void InitializeModules()
        {
            if (platformData == null || platformData.modules == null) return;
        
            foreach (PlatformModuleData data in platformData.modules)
            {
                if (data == null) continue;

                IPlatformModule newModule = PlatformFactory.CreateModule(data);
                if (newModule != null)
                {
                    newModule.Initialize(this, data);
                    activeModules.Add(newModule);
                }
            }
        }

        void Update()
        {
            foreach (var module in activeModules)
            {
                module.OnUpdate();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent<PlayerMovement>(out var player))
            {
                foreach (var module in activeModules) module.OnPlayerEnter(player);
            }
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent<PlayerMovement>(out var player))
            {
                foreach (var module in activeModules) module.OnPlayerStay(player);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.TryGetComponent<PlayerMovement>(out var player))
            {
                foreach (var module in activeModules) module.OnPlayerExit(player);
            }
        }
    
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.TryGetComponent<PlayerMovement>(out var player))
            {
                foreach (var module in activeModules) module.OnPlayerEnter(player);
            }
        }

        private void OnTriggerStay2D(Collider2D collider)
        {
            if (collider.TryGetComponent<PlayerMovement>(out var player))
            {
                foreach (var module in activeModules) module.OnPlayerStay(player);
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.TryGetComponent<PlayerMovement>(out var player))
            {
                foreach (var module in activeModules) module.OnPlayerExit(player);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (platformCollider == null) platformCollider = GetComponent<Collider2D>();

            if (spriteRenderer != null && platformCollider != null && platformCollider is BoxCollider2D boxCol)
            {
                boxCol.autoTiling = false; 
                float depthInUnits = topDepthPixelsToIgnore / pixelsPerUnit;
                Vector2 newSize = new Vector2(spriteRenderer.size.x, spriteRenderer.size.y - depthInUnits);
                boxCol.size = newSize;
                boxCol.offset = new Vector2(0f, -depthInUnits / 2f);
            }
        }
#endif
    }
}