using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BattleUIAgents.Base;
using BattleUIAgents.Agents;
using BattleUIAgents.Tokens;


namespace BattleUIAgents.UI
{
    public class Attackscreen : ScreenBattleUIAgent
    {
        Agents.Grid grid;
        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            LinkAgent(FindAgent(x => { return x is Flag && x.player != player; }));
            grid = (Agents.Grid)LinkAgent(FindAgent(x => { return x is Agents.Grid && x.player == player; }));
            grid.Delinker += () => { grid = null; };

            Delinker += () => { Token.heldToken = null; };
        }

        protected override void ProcessInput()
        {
            base.ProcessInput();
            if (tap)
            {
                bool tapOutsideOfBoardArea = grid.GetTileAtPosition(currentInputPosition.world) == null;
                if (tapOutsideOfBoardArea)
                {
                    gameObject.SetActive(false);
                    FindAgent(x => { return x is Overview; }).gameObject.SetActive(true);
                }
            }
        }

        protected override Vector2 GetFrameSize()
        {
            return base.GetFrameSize() + new Vector2(player.board.tiles.GetLength(0), player.board.tiles.GetLength(1));
        }

        protected override Vector3 GetPosition()
        {
            return base.GetPosition() + player.transform.position + Vector3.left * (player.board.tiles.GetLength(0) / 4.0f);
        }

        protected override float CalculateConversionDistance()
        {
            return Camera.main.transform.position.y - MiscellaneousVariables.it.boardUIRenderHeight;
        }
    }
}
