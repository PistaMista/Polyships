﻿using System.Collections;
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
            Battle.main.attacker.hitTiles.Add(target);
            target.hit = true;

            if (target.containedShip)
            {
                target.containedShip.Damage(1);
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

        public override int GetAdditionalAllowed()
        {
            return Mathf.Clamp(Battle.main.attackerCapabilities.maximumArtilleryCount - Effect.GetEffectsInQueue<ArtilleryAttack>().Length, 0, base.GetAdditionalAllowed()) * (Battle.main.attacker.hitTiles.Contains(target) ? 0 : 1);
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