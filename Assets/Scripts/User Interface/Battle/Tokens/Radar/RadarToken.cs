using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleUIAgents.Base;
using BattleUIAgents.Agents;

using Gameplay;
using Gameplay.Effects;

namespace BattleUIAgents.Tokens
{
    public class RadarToken : Token
    {
        Agents.Grid grid;

        protected override void PerformLinkageOperations()
        {
            grid = (Agents.Grid)LinkAgent(FindAgent(x => { return x.player == Battle.main.defender; }, typeof(Agents.Grid)), false);
            grid.Delinker += () => { grid = null; };

            base.PerformLinkageOperations();
        }
        Vector3 pickupPosition;
        public override void Pickup()
        {
            base.Pickup();
            pickupPosition = transform.position;
        }

        public override void ProcessExternalInputWhileHeld(Vector3 inputPosition)
        {
            base.ProcessExternalInputWhileHeld(inputPosition);
            Gameplay.Tile targetedTile = grid.GetTileAtPosition(inputPosition);

            if (effect == null)
            {
                if (targetedTile != null)
                {
                    effect = Effect.CreateEffect(typeof(RadarRecon));
                    effect.targetedPlayer = Battle.main.defender;
                    effect.visibleTo = Battle.main.attacker;
                }
                else
                {
                    hookedPosition = Utilities.GetPositionOnElevationFromPerspective(inputPosition, Camera.main.transform.position, pickupPosition.y).projectedPosition;
                }
            }
            else
            {
                hookedPosition = Utilities.GetPositionOnElevationFromPerspective(inputPosition, Camera.main.transform.position, MiscellaneousVariables.it.boardUIRenderHeight).projectedPosition;
                if (targetedTile == null)
                {
                    Destroy(effect.gameObject);
                    effect = null;
                }
            }
        }

        public override void Drop()
        {
            base.Drop();
            if (effect != null) RefreshEffectRepresentation();
        }

        protected override void RefreshEffectRepresentation()
        {
            base.RefreshEffectRepresentation();
            Board board = effect.targetedPlayer.board;
            hookedPosition = board.tiles[0, 0].transform.position - new Vector3(1.0f, 0, 1.0f);

            RadarRecon recon = effect as RadarRecon;
            Color color = Color.green;

            if (recon.result.tiles != null)
            {
                for (int x = 0; x < board.tiles.GetLength(0); x++)
                {
                    for (int y = 0; y < board.tiles.GetLength(1); y++)
                    {
                        if (!board.tiles[x, y].hit)
                        {
                            color.g = recon.result.tiles[x, y];
                            grid.SetTileGraphic(new Vector2Int(x, y), Agents.Grid.TileGraphicMaterial.TILE_RADAR, color);
                        }
                    }
                }
            }
        }
    }
}

