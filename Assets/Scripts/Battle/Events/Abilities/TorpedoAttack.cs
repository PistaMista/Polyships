using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.Ships;

namespace Gameplay.Effects
{
    public class TorpedoAttack : Effect
    {
        public Tile torpedoDropPoint;
        public Vector2Int torpedoHeading;
        protected override int[] GetMetadata()
        {
            return new int[] { torpedoDropPoint.coordinates.x, torpedoDropPoint.coordinates.y, torpedoHeading.x, torpedoHeading.y };
        }

        public override void AssignReferences(EffectData data)
        {
            base.AssignReferences(data);
            torpedoDropPoint = Battle.main.defender.board.tiles[data.metadata[0], data.metadata[1]];
            torpedoHeading = new Vector2Int(data.metadata[2], data.metadata[3]);
        }
        public override void OnTurnEnd()
        {
            Board board = Battle.main.defender.board;
            Tile impact = null;
            Tile currentPosition = torpedoDropPoint;
            while (currentPosition != null)
            {
                if (currentPosition.containedShip)
                {
                    impact = currentPosition;
                    break;
                }
                else
                {
                    currentPosition.hit = true;
                    Battle.main.attacker.hitTiles.Add(currentPosition);
                }

                Vector2Int newCoordinates = currentPosition.coordinates + torpedoHeading;
                currentPosition = (newCoordinates.x >= 0 && newCoordinates.x < board.tiles.GetLength(0) && newCoordinates.y >= 0 && newCoordinates.y < board.tiles.GetLength(1)) ? board.tiles[newCoordinates.x, newCoordinates.y] : null;
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
            int max = Battle.main.attackerCapabilities.maximumTorpedoCount;
            int existing = Effect.GetEffectsInQueue(null, typeof(TorpedoAttack), int.MaxValue).Length;
            int baseAllowed = base.GetAdditionalAllowed(ignoreObjectValues);
            return Mathf.Clamp(max - existing, 0, Mathf.Min(baseAllowed, MiscellaneousVariables.it.maximumTorpedoAttacksPerTurn));
        }

        protected override bool ConflictsWith(Effect effect)
        {
            if (!base.ConflictsWith(effect))
            {
                if (effect is TorpedoAttack)
                {
                    TorpedoAttack attack = effect as TorpedoAttack;
                    return attack.torpedoDropPoint == torpedoDropPoint && attack.torpedoHeading == torpedoHeading;
                }
                return false;
            }

            return true;
        }

        public override string GetDescription()
        {
            if (torpedoDropPoint == null)
            {
                return "Fires torpedoes down a line of tiles. Pickup and drag into highlighted areas to select target line and direction.";
            }
            else
            {
                return "Fires a torpedo down this line of tiles, hitting any ship on the way.";
            }
        }
    }
}