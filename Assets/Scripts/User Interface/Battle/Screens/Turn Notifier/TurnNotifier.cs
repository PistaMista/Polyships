using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleUIAgents.Base;
using BattleUIAgents.Agents;

using Gameplay;

namespace BattleUIAgents.UI
{
    public class TurnNotifier : ScreenBattleUIAgent
    {
        [Header("Turn Notification Configuration")]
        public float AITurnWaitTime;
        Flag flag;
        protected override void PerformLinkageOperations()
        {
            player = Battle.main.attacker;
            flag = (Flag)LinkAgent(FindAgent(x => { return x is Flag && x.player == player; }));
            flag.Delinker += () => { flag = null; };

            base.PerformLinkageOperations();

            SetInteractable(!player.aiEnabled);
            LinkAgent(FindAgent(x => { return x.name.Contains("Turn " + (player.aiEnabled ? "AI" : "PLAYER")) && x is Graphicfader; }));
            if (player.aiEnabled) Invoke("DoAITurn", AITurnWaitTime);
        }

        protected override void ProcessInput()
        {
            base.ProcessInput();
            if (tap)
            {
                gameObject.SetActive(false);
                if (player.board.ships == null)
                {
                    FindAgent(x => { return x is FleetPlacer; }).gameObject.SetActive(true);
                }
                else
                {
                    FindAgent(x => { return x is Overview; }).gameObject.SetActive(true);
                }
            }
        }

        void DoAITurn()
        {
            player.aiModule.DoTurn();
            gameObject.SetActive(false);
            if (Battle.main.log.Count > 1)
            {

            }
            else
            {
                gameObject.SetActive(true);
            }
        }
        protected override Vector2 GetFrameSize()
        {
            return base.GetFrameSize() + new Vector2(player.flag.GetLength(0), player.flag.GetLength(1)) * flag.voxelScale;
        }

        protected override Vector3 GetPosition()
        {
            return base.GetPosition() + player.transform.position + Vector3.up * MiscellaneousVariables.it.flagRenderHeight;
        }
    }
}
