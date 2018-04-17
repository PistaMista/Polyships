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

            if (indicator != null && indicator.id == 50) indicator.Delinker();
            indicator = RequestLineMarker(1, false, new Vector3[] { Vector3.zero, Vector3.forward * Battle.main.defender.board.tiles.GetLength(0) * 2.05f }, new int[][] { new int[] { 1 }, new int[0] }, 0, linesMaterial);
            indicator.Delinker += () => { indicator = null; };
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

                Highlighterline areaMarker = RequestLineMarker(2 + i, false, finalNodes, areaMarkerConnections, 0, linesMaterial);
                usageHighlighters.Add(areaMarker);
            }
        }

        public override void Drop()
        {
            base.Drop();
            if (effect == null) indicator.Delinker();
        }

        public override void ProcessExternalInputWhileHeld(Vector3 inputPosition)
        {
            base.ProcessExternalInputWhileHeld(inputPosition);
            int targetLine = -1;
            Vector3Int flatTileCoordinate = grid.GetFlatTileCoordinateAtPosition(inputPosition);
            if (Mathf.Sign(flatTileCoordinate.x) != Mathf.Sign(flatTileCoordinate.z) && flatTileCoordinate.x < Battle.main.defender.board.tiles.GetLength(0) - 1 && flatTileCoordinate.z < Battle.main.defender.board.tiles.GetLength(1) - 1)
            {
                if (flatTileCoordinate.x >= 0)
                {
                    targetLine = flatTileCoordinate.x;
                }
                else
                {
                    targetLine = flatTileCoordinate.z + (Battle.main.defender.board.tiles.GetLength(0) - 1);
                }
            }
            else
            {
                targetLine = -1;
            }

            if (effect == null)
            {
                if (targetLine >= 0)
                {
                    effect = Effect.CreateEffect(typeof(AircraftRecon));
                    effect.visibleTo = Battle.main.attacker;
                    effect.targetedPlayer = Battle.main.defender;
                }
                else
                {
                    hookedPosition = Utilities.GetPositionOnElevationFromPerspective(inputPosition, Camera.main.transform.position, pickupPosition.y).projectedPosition;
                }
            }

            if (effect != null)
            {
                AircraftRecon recon = effect as AircraftRecon;
                if (targetLine >= 0)
                {
                    if (targetLine != recon.target)
                    {
                        recon.target = targetLine;
                        RefreshEffectRepresentation();
                    }
                }
                else
                {
                    Destroy(recon.gameObject);
                    effect = null;
                }
            }
        }

        protected override void RefreshEffectRepresentation()
        {
            base.RefreshEffectRepresentation();
            AircraftRecon recon = effect as AircraftRecon;
            int actualPosition = recon.target % (Battle.main.defender.board.tiles.GetLength(0) - 1);
            bool lineVertical = recon.target == actualPosition;

            if (indicator == null)
            {
                indicator = RequestLineMarker(50, true, new Vector3[] { Vector3.zero, Vector3.forward * 0.925f * Battle.main.defender.board.tiles.GetLength(0), Vector3.forward * 1.025f * Battle.main.defender.board.tiles.GetLength(0) + Vector3.right * (lineVertical ? recon.result : -recon.result) * 2, Vector3.forward * 1.125f * Battle.main.defender.board.tiles.GetLength(0), Vector3.forward * 2.05f * Battle.main.defender.board.tiles.GetLength(0) }, new int[][] { new int[] { 1 }, new int[] { 2 }, new int[] { 3 }, new int[] { 4 }, new int[0] }, 0, linesMaterial);
                indicator.Delinker += () => { indicator = null; };
                indicator.transform.SetParent(transform, false);
            }

            indicator.transform.rotation = Quaternion.Euler(0, lineVertical ? 0 : 90, 0);


            Vector3 startingPosition = Battle.main.defender.board.tiles[0, 0].transform.position - new Vector3(1, 0, 1) * 1f + Vector3.up * (MiscellaneousVariables.it.boardUIRenderHeight + 0.005f);
            hookedPosition = startingPosition + new Vector3(lineVertical ? 1 : 0, 0, lineVertical ? 0 : 1) * (actualPosition + 1.5f);
        }
    }
}