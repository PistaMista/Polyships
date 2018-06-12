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
            flag = (Flag)LinkAgent(FindAgent(x => { return x.player == player; }, typeof(Flag)), true);
            flag.Delinker += () => { flag = null; };

            base.PerformLinkageOperations();

            SetInteractable(!player.aiEnabled);
            LinkAgent(FindAgent(x => { return x.name.Contains("Turn " + (player.aiEnabled ? "AI" : "PLAYER")); }, typeof(Graphicfader)), true);
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
                    FindAgent(x => { return true; }, typeof(FleetPlacer)).gameObject.SetActive(true);
                }
                else
                {
                    Overview overview = (Overview)FindAgent(x => { return true; }, typeof(Overview));
                    overview.enterAttackScreenOnLink = true;
                    overview.gameObject.SetActive(true);
                }
            }
        }

        void DoAITurn()
        {
            AI.PlayTurnForPlayer(player);
            gameObject.SetActive(false);
            if (Battle.main.fighting)
            {
                FindAgent(x => { return x.player != player; }, typeof(Damagereport)).gameObject.SetActive(true);
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
