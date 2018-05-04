﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.Ships;

namespace Gameplay.Effects
{
    public class TorpedoAttack : Effect
    {
        public float boardCoverage;
        public int range
        {
            get
            {
                return Mathf.CeilToInt(Battle.main.defender.board.tiles.GetLength(1) * boardCoverage);
            }
        }
        public struct Target
        {
            public Tile torpedoDropPoint;
            public Vector2Int torpedoHeading;
            public Target(Tile droppoint = null, Vector2Int heading = default(Vector2Int))
            {
                torpedoDropPoint = droppoint;
                torpedoHeading = heading;
            }

            public static bool operator ==(Target x, Target y)
            {
                return x.torpedoDropPoint == y.torpedoDropPoint && x.torpedoHeading == y.torpedoHeading;
            }

            public static bool operator !=(Target x, Target y)
            {
                return x.torpedoDropPoint != y.torpedoDropPoint || x.torpedoHeading != y.torpedoHeading;
            }
        }

        public Target target;
        protected override int[] GetMetadata()
        {
            return new int[] { target.torpedoDropPoint.coordinates.x, target.torpedoDropPoint.coordinates.y, target.torpedoHeading.x, target.torpedoHeading.y };
        }

        public override void AssignReferences(EffectData data)
        {
            base.AssignReferences(data);
            target.torpedoDropPoint = Battle.main.defender.board.tiles[data.metadata[0], data.metadata[1]];
            target.torpedoHeading = new Vector2Int(data.metadata[2], data.metadata[3]);
        }
        public override void OnTurnEnd()
        {
            Board board = Battle.main.defender.board;
            Tile impact = null;

            Tile currentPosition = target.torpedoDropPoint;
            int traveledDistance = 0;
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
                }

                Vector2Int newCoordinates = currentPosition.coordinates + target.torpedoHeading;
                currentPosition = (newCoordinates.x >= 0 && newCoordinates.x < board.tiles.GetLength(0) && newCoordinates.y >= 0 && newCoordinates.y < board.tiles.GetLength(1)) ? board.tiles[newCoordinates.x, newCoordinates.y] : null;
                traveledDistance++;

                if (traveledDistance >= range)
                {
                    break;
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
                    tile.containedShip.Damage(1);
                }
            }

            //Consume a torpedo 
            Battle.main.attacker.arsenal.torpedoes--;
            Battle.main.attacker.arsenal.loadedTorpedoes--;
            Battle.main.attacker.arsenal.torpedoesFiredLastTurn++;

            base.OnTurnEnd();
        }

        public override int GetTheoreticalMaximumAddableAmount()
        {
            return Battle.main.attacker.arsenal.loadedTorpedoes - Effect.GetEffectsInQueue(null, typeof(TorpedoAttack), int.MaxValue).Length;
        }

        protected override bool CheckGameplayRulesForAddition()
        {
            return target.torpedoDropPoint != null && target.torpedoHeading != Vector2Int.zero; //Has to have a target.
        }

        protected override bool IsConflictingWithEffect(Effect effect)
        {
            //Conflicts with this player's torpedo cooldowns(but not reloads) and any other torpedo attacks with the same target player, line and direction.
            return (effect is TorpedoCooldown && effect.targetedPlayer == visibleTo) || (effect.targetedPlayer == targetedPlayer && ((effect is TorpedoAttack && (effect as TorpedoAttack).target == target)));
        }

        public override string GetDescription()
        {
            if (target.torpedoDropPoint == null)
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