using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BattleUIAgents.Base;
using BattleUIAgents.Agents;

using Gameplay;
using Gameplay.Effects;

namespace BattleUIAgents.Tokens
{
    public class ArtilleryToken : Token
    {
        Agents.Grid grid;

        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            grid = (Agents.Grid)LinkAgent(FindAgent(x => { return x.player == Battle.main.defender; }, typeof(Agents.Grid)), false);
            grid.Delinker += () => { grid = null; };
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
                    effect = Effect.CreateEffect(typeof(ArtilleryAttack));
                }
                else
                {
                    hookedPosition = Utilities.GetPositionOnElevationFromPerspective(inputPosition, Camera.main.transform.position, pickupPosition.y).projectedPosition;
                }
            }

            if (effect != null)
            {
                ArtilleryAttack attack = effect as ArtilleryAttack;
                if (targetedTile != null)
                {
                    if (targetedTile != attack.target)
                    {
                        attack.target = targetedTile;
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
            hookedPosition = (effect as ArtilleryAttack).target.transform.position + Vector3.up * (MiscellaneousVariables.it.boardUIRenderHeight + height);
        }
    }
}
