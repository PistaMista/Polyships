using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleUIAgents.Base;
using BattleUIAgents.Agents;

using Gameplay;

namespace BattleUIAgents.UI
{
    public class TurnNotifier : BattleUIAgent
    {
        public float AITurnWaitTime;
        protected override void GatherRequiredAgents()
        {
            base.GatherRequiredAgents();
            player = Battle.main.attacker;

            SetInteractable(!player.aiEnabled);
            HookToThis<Graphicfader>("Turn " + (player.aiEnabled ? "AI" : "PLAYER"), null, 1, false);
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
                    FindAgents<FleetPlacer>("", null, 1)[0].gameObject.SetActive(true);
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
