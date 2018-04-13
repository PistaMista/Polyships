using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BattleUIAgents.Base;
using BattleUIAgents.Agents;
using Gameplay;
using Gameplay.Effects;

namespace BattleUIAgents.Tokens
{
    public class AircraftToken : Token
    {
        public Material linesMaterial;
        Agents.Grid grid;
        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            grid = (Agents.Grid)LinkAgent(FindAgent(x => { return x.player == Battle.main.defender; }, typeof(Agents.Grid)), false);
            grid.Delinker += () => { grid = null; };
        }
        Highlighterline indicator;
        Vector3 pickupPosition;
        public override void Pickup()
        {
            base.Pickup();
            pickupPosition = transform.position;

            indicator = RequestLineMarker(458, false, new Vector3[] { Vector3.zero, Vector3.forward * Battle.main.defender.board.tiles.GetLength(0) }, new int[][] { new int[] { 1 }, new int[0] }, 0, linesMaterial);
            indicator.transform.SetParent(transform, false);

            int[][] areaMarkerConnections = new int[][] { new int[] { 1 }, new int[] { 2 }, new int[] { 3 }, new int[] { 4 }, new int[0] };
            Vector3[] areaMarkerNodes = new Vector3[5];
            areaMarkerNodes[0] = -Battle.main.defender.board.tiles.GetLength(0) / 2.0f * new Vector3(1, 0, 1);
            areaMarkerNodes[1] = areaMarkerNodes[0];
            areaMarkerNodes[1].x *= -1;
            areaMarkerNodes[2] = areaMarkerNodes[1] + Vector3.back;
            areaMarkerNodes[3] = areaMarkerNodes[0] + Vector3.back;
            areaMarkerNodes[4] = areaMarkerNodes[0];

            for (int i = 0; i < 2; i++)
            {
                Vector3[] finalNodes = new Vector3[5];
                System.Array.Copy(areaMarkerNodes, finalNodes, finalNodes.Length);

                if (i == 1) for (int x = 0; x < areaMarkerNodes.Length; x++)
                    {
                        Vector3 initialPosition = finalNodes[x];
                        finalNodes[x] = new Vector3(initialPosition.z, 0, initialPosition.x);
                    }

                for (int x = 0; x < finalNodes.Length; x++)
                {
                    finalNodes[x] += Battle.main.defender.transform.position + Vector3.up * MiscellaneousVariables.it.boardUIRenderHeight;
                }

                Highlighterline areaMarker = RequestLineMarker(459 + i, false, finalNodes, areaMarkerConnections, 0, linesMaterial);
                usageHighlighters.Add(areaMarker);
            }
        }

        public override void Drop()
        {
            base.Drop();


            indicator.Delinker();
        }

        public override void ProcessExternalInputWhileHeld(Vector3 inputPosition)
        {
            base.ProcessExternalInputWhileHeld(inputPosition);
            Gameplay.Tile targetedTile = grid.GetTileAtPosition(inputPosition);
            hookedPosition = Utilities.GetPositionOnElevationFromPerspective(inputPosition, Camera.main.transform.position, pickupPosition.y).projectedPosition;


        }

        protected override void RefreshEffectRepresentation()
        {
            base.RefreshEffectRepresentation();
            if (indicator == null)
            {

            }
        }
    }
}