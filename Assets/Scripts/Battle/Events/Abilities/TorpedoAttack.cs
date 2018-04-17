using System.Collections;
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
                    Battle.main.attacker.hitTiles.Add(currentPosition);
                }

                Vector2Int newCoordinates = currentPosition.coordinates + torpedoHeading;
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
                    Battle.main.attacker.hitTiles.Add(tile);
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
            return torpedoDropPoint != null && torpedoHeading != Vector2Int.zero; //Has to have a target.
        }

        protected override bool IsConflictingWithEffect(Effect effect)
        {
            //Conflicts with this player's torpedo cooldowns(but not reloads), artillery attacks and any other torpedo attacks with the same target player, line and direction.
            return (effect is TorpedoCooldown && effect.targetedPlayer == visibleTo) || (effect.targetedPlayer == targetedPlayer && (effect is ArtilleryAttack || (effect is TorpedoAttack && (effect as TorpedoAttack).torpedoDropPoint == torpedoDropPoint && (effect as TorpedoAttack).torpedoHeading == torpedoHeading)));
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