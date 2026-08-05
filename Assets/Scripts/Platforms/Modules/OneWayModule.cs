using System.Collections;
using System.Collections.Generic;
using Platforms.Data;
using Player;
using UnityEngine;

namespace Platforms.Modules
{
    public class OneWayModule : IPlatformModule
    {
        private PlatformController controller;
        private OneWayData data;
        
        private HashSet<Collider2D> activeDrops = new HashSet<Collider2D>();

        public void Initialize(PlatformController controller, PlatformModuleData data)
        {
            this.controller = controller;
            this.data = data as OneWayData;
            SetupEffector();
        }

        private void SetupEffector()
        {
            switch (data.passType)
            {
                case PassType.Solid:
                    controller.effector.useOneWay = false;
                    break;
                case PassType.PassFromBottom:
                    controller.effector.useOneWay = true;
                    controller.effector.rotationalOffset = 0f;
                    controller.effector.surfaceArc = 180f;
                    break;
                case PassType.PassFromTop:
                    controller.effector.useOneWay = true;
                    controller.effector.rotationalOffset = 180f;
                    controller.effector.surfaceArc = 180f;
                    break;
            }
        }

        public void OnUpdate() { }
        public void OnPlayerEnter(PlayerMovement player) { }
        
        public void OnPlayerStay(PlayerMovement player)
        {
            if (!data.allowDropDown) return;

            if (player.TryGetComponent<Collider2D>(out var playerCol))
            {
                if (player.isTryingToDropDown && !activeDrops.Contains(playerCol))
                {
                    controller.StartCoroutine(DropDownRoutine(playerCol));
                }
            }
        }

        public void OnPlayerExit(PlayerMovement player) { }

        private IEnumerator DropDownRoutine(Collider2D playerCol)
        {
            activeDrops.Add(playerCol);
            
            Physics2D.IgnoreCollision(playerCol, controller.platformCollider, true);
            
            yield return new WaitForSeconds(data.dropDownTime);
            
            Physics2D.IgnoreCollision(playerCol, controller.platformCollider, false);
            activeDrops.Remove(playerCol);
        }
    }
}