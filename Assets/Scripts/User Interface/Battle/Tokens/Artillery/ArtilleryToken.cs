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
            grid = (Agents.Grid)LinkAgent(FindAgent(x => { return x is Agents.Grid && x.player == player; }));
            grid.Delinker += () => { grid = null; };
        }
        Vector3 pickupPosition;
        protected override void Pickup()
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
                    effect = Effect.CreateEffect<ArtilleryAttack>();
                }
                else
                {
                    inputPosition.y = pickupPosition.y;
                    hookedPosition = inputPosition;
                }
            }

            if (effect != null)
            {
                ArtilleryAttack attack = (ArtilleryAttack)effect;
                if (targetedTile != null)
                {
                    attack.target = targetedTile;
                    hookedPosition = targetedTile.transform.position;
                }
                else
                {
                    Destroy(attack.gameObject);
                    effect = null;
                }
            }
        }
    }
}
