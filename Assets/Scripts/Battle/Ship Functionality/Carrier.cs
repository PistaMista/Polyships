using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Ships
{
    public class Carrier : Ship
    {
        public int aircraftCount;
        public int aircraftCapacity;
        public int[] reconTargets;
        public int[,] reconResults = new int[0, 0];

        public override int[] GetMetadata()
        {
            List<int> result = new List<int>();
            for (int i = 0; i < reconResults.GetLength(0); i++)
            {
                result.Add(reconResults[i, 0]);
                result.Add(reconResults[i, 1]);
            }

            return result.ToArray();
        }

        public override void AssignReferences(ShipData data)
        {
            base.AssignReferences(data);
            reconResults = new int[data.metadata.Length / 2, 2];

            for (int i = 1; i < data.metadata.Length; i += 2)
            {
                reconResults[(i - 1) / 2, 0] = data.metadata[i - 1];
                reconResults[(i - 1) / 2, 1] = data.metadata[i];
            }
        }

        public override void OnTurnEnd()
        {
            base.OnTurnEnd();
            if (health > 0)
            {
                if (reconTargets != null && reconTargets.Length > 0)
                {
                    reconResults = new int[reconTargets.Length, 2];
                    for (int i = 0; i < reconTargets.Length; i++)
                    {
                        int lineIndex = reconTargets[i];
                        float linePosition = (lineIndex % (Battle.main.defender.board.tiles.GetLength(0) - 1)) + 0.5f;
                        bool lineVertical = lineIndex < linePosition;

                        int currentResult = 0;
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
                                        currentResult = (int)Mathf.Sign(relativePosition);
                                        closestTileDistance = distance;
                                    }
                                }
                            }
                        }

                        reconResults[i, 0] = lineIndex;
                        reconResults[i, 1] = currentResult;
                    }
                }
            }
            else
            {
                reconResults = new int[0, 0];
            }

            reconTargets = null;
        }

    }
}
