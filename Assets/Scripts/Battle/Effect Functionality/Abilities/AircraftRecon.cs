using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class AircraftRecon : Effect
    {
        public int target;
        public int result;

        protected override int[] GetMetadata()
        {
            return new int[] { target, result };
        }

        public override void Initialize(EffectData data)
        {
            target = data.metadata[0];
            result = data.metadata[1];
            base.Initialize(data);
        }

        public override void OnTurnEnd()
        {
            float linePosition = (target % (Battle.main.defender.board.tiles.GetLength(0) - 1)) + 0.5f;
            bool lineVertical = target < linePosition;

            float closestTileDistance = Mathf.Infinity;
            foreach (Ship ship in targetedPlayer.board.ships)
            {
                if (ship.health > 0)
                {
                    foreach (Tile tile in ship.tiles)
                    {
                        float relativePosition = (lineVertical ? tile.coordinates.x : tile.coordinates.y) - linePosition;
                        float distance = Mathf.Abs(relativePosition);

                        if (distance < closestTileDistance)
                        {
                            result = (int)Mathf.Sign(relativePosition);
                            closestTileDistance = distance;
                        }
                    }
                }
            }

            editable = false;
            base.OnTurnEnd();
        }
        public override int Max()
        {
            return Battle.main.attacker.arsenal.aircraft - Battle.main.effects.FindAll(x => x is AircraftRecon && x.visibleTo == Battle.main.attacker).Count;
        }

        protected override bool Legal()
        {
            return target >= 0 && target < (Battle.main.defender.board.tiles.GetLength(0) * 2 - 2); //Has to have a targeted line.
        }

        protected override bool Conflicts(Effect effect)
        {
            return effect is AircraftRecon && (effect as AircraftRecon).target == target && effect.targetedPlayer == targetedPlayer; //Conflicts with aircraft recon targeted at the same line and at the same player.
        }

        public override string GetDescription()
        {
            if (target != -1)
            {
                string desc = "Shows an arrow pointing to the ship, that is closest to this line.";
                if (!editable)
                {
                    desc += " Lasts for " + FormattedDuration + ".";
                }

                return desc;
            }

            return "Launches aircraft to point you in the direction of enemy ships. Pickup and drag into highlighted areas to select target line.";
        }
    }
}