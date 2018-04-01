using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleUIAgents.Base
{
    public class WorldBattleUIAgent : BattleUIAgent
    {
        public Vector3 unhookedPosition;
        public Vector3 hookedPosition;
        public float movementTime;
        public float movementMaxSpeed;
        public float maximumInteractableVelocity;
        public Vector3 currentVelocity;
        bool belowMaxInteractableVelocity;



        protected override void Update()
        {
            base.Update();
            Vector3 targetPosition = linked ? hookedPosition : unhookedPosition;
            if (Vector3.Distance(transform.localPosition, targetPosition) > 0.005f)
            {
                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref currentVelocity, movementTime, movementMaxSpeed);
            }
            else
            {
                transform.localPosition = targetPosition;
            }

            bool aboveMaxInteractableVelocity = currentVelocity.magnitude > maximumInteractableVelocity;
            if (belowMaxInteractableVelocity == aboveMaxInteractableVelocity)
            {
                SetInteractable(!aboveMaxInteractableVelocity && linked);
                belowMaxInteractableVelocity = !aboveMaxInteractableVelocity;
            }
        }

        protected virtual void OnHookedPositionSet(Vector3 position)
        {

        }

        protected virtual void OnUnhookedPositionSet(Vector3 position)
        {

        }
    }
}