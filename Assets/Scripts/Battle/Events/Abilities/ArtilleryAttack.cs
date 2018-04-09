using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class ArtilleryAttack : Effect
    {
        public Tile target;
        public override void OnTurnEnd()
        {
            Battle.main.log[0].artilleryImpacts.Add(target);

            if (target.containedShip && target.containedShip.concealedBy)
            {
                for (int x = (target.coordinates.x == 0 ? 0 : -1); x <= ((target.coordinates.x == target.parentBoard.tiles.GetLength(0) - 1) ? 0 : 1); x++)
                {
                    for (int y = (target.coordinates.y == 0 ? 0 : -1); y <= ((target.coordinates.y == target.parentBoard.tiles.GetLength(1) - 1) ? 0 : 1); y++)
                    {
                        if (!(y == 0 && x == 0))
                        {
                            Tile candidate = target.parentBoard.tiles[x + (int)target.coordinates.x, y + (int)target.coordinates.y];
                            if (!candidate.hit && candidate.containedShip == null)
                            {
                                target = candidate;
                                break;
                            }
                        }
                    }
                }
            }

            if (!target.hit)
            {
                Battle.main.attacker.hitTiles.Add(target);
                target.hit = true;

                if (target.containedShip)
                {
                    target.containedShip.Damage(1);
                }
            }

            base.OnTurnEnd();
        }

        protected override int[] GetMetadata()
        {
            return new int[2] { target.coordinates.x, target.coordinates.y };
        }

        public override void AssignReferences(EffectData data)
        {
            target = Battle.main.defender.board.tiles[data.metadata[0], data.metadata[1]];
        }

        public override int GetAdditionalAllowed(bool ignoreObjectValues)
        {
            int modifier = ignoreObjectValues ? 1 : (target.hit ? 0 : 1);
            return Mathf.Clamp(Battle.main.attackerCapabilities.maximumArtilleryCount - Effect.GetEffectsInQueue<ArtilleryAttack>().Length, 0, base.GetAdditionalAllowed(ignoreObjectValues)) * modifier;
        }

        protected override bool ConflictsWith(Effect effect)
        {
            if (!base.ConflictsWith(effect))
            {
                if (effect is ArtilleryAttack && ((ArtilleryAttack)effect).target == target)
                {
                    return true;
                }
                return false;
            }

            return true;
        }
    }
}