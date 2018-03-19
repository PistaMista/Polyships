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
            transform.position = Vector3.SmoothDamp(transform.position, hookedTo == null ? unhookedPosition : hookedPosition, ref currentVelocity, movementTime, movementMaxSpeed);

            bool aboveMaxInteractableVelocity = currentVelocity.magnitude > maximumInteractableVelocity;
            if (belowMaxInteractableVelocity == aboveMaxInteractableVelocity)
            {
                SetInteractable(!aboveMaxInteractableVelocity && hookedTo != null);
                belowMaxInteractableVelocity = !aboveMaxInteractableVelocity;
            }
        }
    }
}