﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class ArtilleryAttack : Effect
    {
        public Tile target;

        protected override void OnExpire(bool forced)
        {
            base.OnExpire(forced);
            if (!forced)
            {
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
                    target.hit = true;

                    if (target.containedShip)
                    {
                        target.containedShip.Damage(1);
                    }
                }
            }
        }

        protected override int[] GetMetadata()
        {
            return new int[2] { target.coordinates.x, target.coordinates.y };
        }

        public override void AssignReferences(EffectData data)
        {
            target = Battle.main.defender.board.tiles[data.metadata[0], data.metadata[1]];
        }

        public override int Max()
        {
            return Mathf.Clamp(Battle.main.attacker.arsenal.guns - Battle.main.effects.FindAll(x => x is ArtilleryAttack).Count, 0, int.MaxValue);
        }

        protected override bool Legal()
        {
            return target != null && !target.hit; //Target tile is required. Targeted tile has to not be hit. 
        }

        protected override bool Conflicts(Effect effect)
        {
            return (effect is ArtilleryAttack && (effect as ArtilleryAttack).target == target) && effect.targetedPlayer == targetedPlayer; //Conflicts with any artillery attacks targeted at the same tile as this one.
        }

        public override string GetDescription()
        {
            return target != null ? "Fires the fleet's main guns at this tile." : "Fires guns at a target tile. Drag onto the board to select target tile.";
        }
    }
}