using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.Ships;

namespace Gameplay.Effects
{
    public class TorpedoAttack : Effect
    {

        public int target;
        public override void OnTurnEnd()
        {
            Tile impact = null;
            for (int y = Battle.main.defender.board.tiles.GetLength(1) - 1; y >= 0; y--)
            {
                Tile candidate = Battle.main.defender.board.tiles[target, y];
                if (candidate.containedShip && candidate.containedShip.health > 0)
                {
                    impact = candidate;
                    break;
                }
                else
                {
                    candidate.hit = true;
                    Battle.main.attacker.hitTiles.Add(candidate);
                }
            }

            List<Tile> damagedTiles = new List<Tile>();
            if (impact)
            {
                if (impact.containedShip.health < impact.containedShip.maxHealth)
                {
                    damagedTiles.AddRange(impact.containedShip.tiles);
                }
                else
                {
                    damagedTiles.Add(impact);
                }
            }

            foreach (Tile tile in damagedTiles)
            {
                if (!tile.hit && tile.containedShip.health > 0)
                {
                    tile.hit = true;
                    Battle.main.attacker.hitTiles.Add(tile);
                    tile.containedShip.Damage(1);
                }
            }

            //Consume a torpedo from the destroyers
            for (int i = 0; i < Battle.main.attacker.board.ships.Length; i++)
            {
                Ship ship = Battle.main.attacker.board.ships[i];
                if (ship.health > 0 && ship.type == ShipType.DESTROYER)
                {
                    Destroyer destroyer = (Destroyer)ship;
                    if (destroyer.torpedoCount > 0)
                    {
                        destroyer.torpedoCount--;
                        break;
                    }
                }
            }

            base.OnTurnEnd();
        }

        public override int GetAdditionalAllowed(bool ignoreObjectValues)
        {
            int modifier = ignoreObjectValues ? 1 : (Battle.main.attackerCapabilities.torpedoFiringArea[target] ? 1 : 0);
            return Mathf.Clamp(Battle.main.attackerCapabilities.maximumTorpedoCount - Effect.GetEffectsInQueue<TorpedoAttack>().Length, 0, Mathf.Min(base.GetAdditionalAllowed(ignoreObjectValues), MiscellaneousVariables.it.maximumTorpedoAttacksPerTurn)) * modifier;
        }

        protected override bool ConflictsWith(Effect effect)
        {
            if (!base.ConflictsWith(effect))
            {
                if (effect is TorpedoAttack && ((TorpedoAttack)effect).target == target)
                {
                    return true;
                }
                return false;
            }

            return true;
        }
    }
}