﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class AircraftRecon : Effect
    {
        public int target;
        public int result;

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
        public override int GetAdditionalAllowed()
        {
            return Mathf.Clamp(Battle.main.attackerCapabilities.maximumAircraftCount - Effect.GetAmountInQueue<AircraftRecon>(), 0, base.GetAdditionalAllowed());
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