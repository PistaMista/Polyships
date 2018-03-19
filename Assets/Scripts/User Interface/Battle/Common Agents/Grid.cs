using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleUIAgents.Base;

namespace BattleUIAgents.Agents
{
    public class Grid : BattleUIAgent
    {
        protected override void GatherRequiredAgents()
        {
            base.GatherRequiredAgents();
            Board managedBoard = player.board;
            Gridline[] gridLines = (Gridline[])HookToThis<Gridline>("", player, true, managedBoard.tiles.GetLength(0) + managedBoard.tiles.GetLength(1) - 2);

            float lineWidth = 1.00f - MiscellaneousVariables.it.boardTileSideLength;
            for (int i = 0; i < gridLines.Length; i++)
            {
                Gridline positionedLine = gridLines[i];
                int verticalIndex = i - (managedBoard.tiles.GetLength(0) - 1);

                if (verticalIndex < 0)
                {
                    positionedLine.transform.localScale = new Vector3(lineWidth, 1, managedBoard.tiles.GetLength(1));
                    positionedLine.hookedPosition = new Vector3(-managedBoard.tiles.GetLength(0) / 2.0f + i, 0, 0);
                }
                else
                {
                    positionedLine.transform.localScale = new Vector3(managedBoard.tiles.GetLength(0), 1, lineWidth);
                    positionedLine.hookedPosition = new Vector3(0, 0, -managedBoard.tiles.GetLength(1) / 2.0f + verticalIndex);
                }

                positionedLine.hookedPosition += managedBoard.transform.position + Vector3.up * MiscellaneousVariables.it.boardUIRenderHeight;
                positionedLine.unhookedPosition = positionedLine.hookedPosition - Vector3.up * positionedLine.hookedPosition.y * 1.25f;
                positionedLine.transform.position = positionedLine.unhookedPosition;
            }
        }

        protected override void ProcessInput()
        {
            base.ProcessInput();
        }
    }
}
