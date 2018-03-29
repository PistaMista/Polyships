using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BattleUIAgents.Base
{
    public class ScreenBattleUIAgent : BattleUIAgent
    {
        [Header("Camera Configuration")]
        public Waypoint cameraWaypoint;
        public Vector2 defaultFrameSize;
        public Vector3 positionOffset;
        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            if (cameraWaypoint)
            {
                cameraWaypoint.transform.position = GetPosition();
                CameraControl.GoToWaypoint(cameraWaypoint);
            }
        }

        void OnEnable()
        {
            PerformLinkageOperations();
        }

        void OnDisable()
        {
            Delinker();
        }

        public static void DelinkAllScreenAgents()
        {
            foreach (BattleUIAgent agent in FindAgents(x => { return true; }, typeof(ScreenBattleUIAgent), 20))
            {
                agent.Delinker();
            }
        }

        protected virtual Vector2 GetFrameSize()
        {
            return defaultFrameSize;
        }

        protected virtual Vector3 GetPosition()
        {
            return Vector3.up * CameraControl.CalculateCameraWaypointHeight(GetFrameSize()) + positionOffset;
        }
    }
}