using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Gameplay;
using BattleUIAgents.Base;
using BattleUIAgents.Agents;

namespace BattleUIAgents.UI
{
    public class Damagereport : ScreenBattleUIAgent
    {
        Agents.Grid grid;
        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            grid = LinkAgent(FindAgent(x => { return x.player == player; }, typeof(Agents.Grid)), true) as Agents.Grid;
            grid.Delinker += () => { grid = null; };

            grid.ShowInformation(false, (player == Battle.main.attacker ? Battle.main.defender : Battle.main.attacker).aiEnabled, true, true);
        }

        protected override void ProcessInput()
        {
            base.ProcessInput();
            if (tap)
            {
                gameObject.SetActive(false);
                FindAgent(x => { return true; }, typeof(TurnNotifier)).gameObject.SetActive(true);
            }
        }

        protected override Vector2 GetFrameSize()
        {
            return base.GetFrameSize() + new Vector2(player.board.tiles.GetLength(0), player.board.tiles.GetLength(1));
        }

        protected override Vector3 GetPosition()
        {
            return base.GetPosition() + player.transform.position;
        }

        protected override float CalculateConversionDistance()
        {
            return Camera.main.transform.position.y - MiscellaneousVariables.it.boardUIRenderHeight;
        }
    }
}