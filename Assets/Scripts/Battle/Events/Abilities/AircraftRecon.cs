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
            foreach (Ship ship in Battle.main.defender.board.ships)
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

            base.OnTurnEnd();
        }
        public override int GetAdditionalAllowed(bool ignoreObjectValues)
        {
            int max = Battle.main.attackerCapabilities.maximumAircraftCount;
            int existing = Effect.GetEffectsInQueue(null, typeof(AircraftRecon), int.MaxValue).Length;
            int baseAllowed = base.GetAdditionalAllowed(ignoreObjectValues);
            return Mathf.Clamp(max - existing, 0, baseAllowed);
        }

        protected override bool ConflictsWith(Effect effect)
        {
            if (!base.ConflictsWith(effect))
            {
                if (effect is AircraftRecon && ((AircraftRecon)effect).target == target)
                {
                    return true;
                }
                return false;
            }

            return true;
        }
    }
}