using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carrier : Ship
{
    public int aircraftCount;
    public int aircraftCapacity;
    public int[] polarSearchTargets = new int[0];
    public int[,] polarSearchResults = new int[0, 0];

    public override void OnTurnStart()
    {
        base.OnTurnStart();
        if (health > 0)
        {
            if (polarSearchTargets != null)
            {
                polarSearchResults = new int[polarSearchTargets.Length, 2];
                for (int i = 0; i < polarSearchTargets.Length; i++)
                {
                    int lineIndex = polarSearchTargets[i];
                    float linePosition = (lineIndex % (Battle.main.defender.board.tiles.GetLength(0) - 1)) + 0.5f;
                    bool lineVertical = lineIndex < linePosition;

                    int currentResult = 0;
                    float closestTileDistance = Mathf.Infinity;

                    foreach (Ship ship in Battle.main.defender.board.ships)
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

                    polarSearchResults[i, 0] = lineIndex;
                    polarSearchResults[i, 1] = currentResult;
                }
            }
        }
        else
        {
            polarSearchResults = null;
        }

        polarSearchTargets = null;
    }

}
