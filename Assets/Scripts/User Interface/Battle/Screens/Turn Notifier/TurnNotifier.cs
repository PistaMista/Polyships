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
        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            player = Battle.main.attacker;

            SetInteractable(!player.aiEnabled);
            LinkAgents(FindAgents(limit: 1, predicate: x => { return x.name.Contains("Turn " + (player.aiEnabled ? "AI" : "PLAYER")) && x is Graphicfader; }));
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
    }
}
