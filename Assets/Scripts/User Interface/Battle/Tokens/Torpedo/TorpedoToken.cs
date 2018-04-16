using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BattleUIAgents.Base;
using BattleUIAgents.Agents;
using Gameplay;
using Gameplay.Effects;

namespace BattleUIAgents.Tokens
{
    public class TorpedoToken : Token
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

            for (int i = 0; i < 4; i++)
            {
                for (int x = 0; x < areaMarkerNodes.Length; x++)
                {
                    Vector3 initialPosition = areaMarkerNodes[x];
                    areaMarkerNodes[x] = new Vector3(-initialPosition.z, 0, initialPosition.x);
                }

                Vector3[] finalNodes = new Vector3[5];
                System.Array.Copy(areaMarkerNodes, finalNodes, finalNodes.Length);

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
            Gameplay.Tile torpedoDrop = null;
            Vector2Int torpedoHeading = Vector2Int.zero;

            Board board = Battle.main.defender.board;
            Vector3Int flatTileCoordinate = grid.GetFlatTileCoordinateAtPosition(inputPosition);

            if (!grid.GetTileAtPosition(inputPosition))
            {
                if (flatTileCoordinate.x >= 0 && flatTileCoordinate.x < board.tiles.GetLength(0))
                {
                    torpedoDrop = board.tiles[flatTileCoordinate.x, flatTileCoordinate.z > 0 ? board.tiles.GetLength(1) - 1 : 0];
                    torpedoHeading = Vector2Int.up * (flatTileCoordinate.z > 0 ? -1 : 1);
                }
                else if (flatTileCoordinate.z >= 0 && flatTileCoordinate.z < board.tiles.GetLength(1))
                {
                    torpedoDrop = board.tiles[flatTileCoordinate.x > 0 ? board.tiles.GetLength(0) - 1 : 0, flatTileCoordinate.z];
                    torpedoHeading = Vector2Int.right * (flatTileCoordinate.x > 0 ? -1 : 1);
                }
            }

            if (effect == null)
            {
                if (torpedoDrop != null && torpedoHeading != Vector2Int.zero)
                {
                    effect = Effect.CreateEffect(typeof(TorpedoAttack));
                    effect.visibleTo = Battle.main.attacker;
                    effect.affectedPlayer = Battle.main.defender;
                }
                else
                {
                    hookedPosition = Utilities.GetPositionOnElevationFromPerspective(inputPosition, Camera.main.transform.position, pickupPosition.y).projectedPosition;
                }
            }

            if (effect != null)
            {
                TorpedoAttack attack = effect as TorpedoAttack;
                if (torpedoDrop != null && torpedoHeading != Vector2Int.zero)
                {
                    if (torpedoDrop != attack.torpedoDropPoint || torpedoHeading != attack.torpedoHeading)
                    {
                        attack.torpedoDropPoint = torpedoDrop;
                        attack.torpedoHeading = torpedoHeading;
                        RefreshEffectRepresentation();
                    }
                }
                else
                {
                    Destroy(attack.gameObject);
                    effect = null;
                }
            }
        }


        protected override void RefreshEffectRepresentation()
        {
            base.RefreshEffectRepresentation();
            TorpedoAttack attack = effect as TorpedoAttack;
            if (indicator == null)
            {
                indicator = RequestLineMarker(1, false, new Vector3[] { Vector3.zero, Vector3.forward * Battle.main.defender.board.tiles.GetLength(0) * 2.05f }, new int[][] { new int[] { 1 }, new int[0] }, 0, linesMaterial);
                indicator.Delinker += () => { indicator = null; };
                indicator.transform.SetParent(transform, false);
            }

            bool horizontal = attack.torpedoHeading.x != 0;
            bool directionPositive = attack.torpedoHeading.x + attack.torpedoHeading.y > 0;

            indicator.transform.rotation = Quaternion.Euler(0, horizontal ? (directionPositive ? 90 : 270) : (directionPositive ? 0 : 180), 0);
            hookedPosition = attack.torpedoDropPoint.transform.position + new Vector3(-attack.torpedoHeading.x, MiscellaneousVariables.it.boardUIRenderHeight + height, -attack.torpedoHeading.y);
        }
    }
}
