using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIModule : ScriptableObject
{
    [Serializable]
    public struct AIModuleData
    {
        public static implicit operator AIModuleData(AIModule module)
        {
            AIModuleData result;
            return result;
        }
    }
    public Player owner;

    public virtual void Initialize(AIModuleData data)
    {

    }

    public virtual void AssignReferences(AIModuleData data)
    {

    }

    public void DoTurn()
    {
        if (owner.board.ships == null)
        {
            owner.board.SpawnShips();
            PlaceShips();
            for (int i = 0; i < owner.board.ships.Length; i++)
            {
                owner.board.ships[i].gameObject.SetActive(false);
            }
            Battle.main.NextTurn();
        }
        else
        {
            Attack();
        }
    }

    void Attack()
    {
        //TEST
        Battle.main.ExecuteArtilleryAttack(new Tile[] { Battle.main.defender.board.tiles[0, 0] });
        //TEST
    }


    public void PlaceShips()
    {
        //Remove any placed owner.board.ships from the board
        for (int i = 0; i < owner.board.ships.Length; i++)
        {
            Ship ship = owner.board.ships[i];
            ship.Pickup();
            ship.Place(null);
        }

        //Each ship gets a heatmap of best placement spots
        float[][,] shipLocationHeatmaps = new float[owner.board.ships.Length][,];
        for (int i = 0; i < owner.board.ships.Length; i++)
        {
            shipLocationHeatmaps[i] = new float[owner.board.tiles.GetLength(0), owner.board.tiles.GetLength(1)];
        }

        //Determine heatmaps by individual tactical choices
        //1.Tactic - Dispersion
        float dispersionValue = UnityEngine.Random.Range(0.000f, 1.000f);
        for (int i = 0; i < owner.board.ships.Length; i++)
        {
            shipLocationHeatmaps[i] = ModifyHeatmap(shipLocationHeatmaps[i], new Tile[] { owner.board.tiles[UnityEngine.Random.Range(0, owner.board.tiles.GetLength(0)), UnityEngine.Random.Range(0, owner.board.tiles.GetLength(1))] }, 8.0f * dispersionValue, 0.15f);
        }

        //2.Tactic - Destroyer location
        float agressivityValue = 1.0f - (float)Math.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 2);
        float discretionValue = 1.0f + (float)Math.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 5);

        Tile[] topBar = new Tile[owner.board.tiles.GetLength(0)];
        for (int x = 0; x < topBar.Length; x++)
        {
            topBar[x] = owner.board.tiles[x, owner.board.tiles.GetLength(1) - 1 - UnityEngine.Random.Range(0, 4)];
        }

        for (int i = 0; i < owner.board.ships.Length; i++)
        {
            if (owner.board.ships[i].type == ShipType.DESTROYER)
            {
                shipLocationHeatmaps[i] = ModifyHeatmap(shipLocationHeatmaps[i], topBar, agressivityValue * 12.0f, 0.1f);
            }
            else
            {
                shipLocationHeatmaps[i] = ModifyHeatmap(shipLocationHeatmaps[i], topBar, -discretionValue * 9.0f, 0.7f);
            }
        }

        //3.Tactic - Camouflage
        float concealmentAccuracyValue = 1.0f - (float)Math.Pow(UnityEngine.Random.Range(0.000f, 1.000f), 4);
        List<int> cruiserIDs = new List<int>();
        for (int i = 0; i < owner.board.ships.Length; i++)
        {
            if (owner.board.ships[i].type == ShipType.CRUISER)
            {
                cruiserIDs.Add(i);
            }
        }

        int[] shipsToConcealIDs = new int[cruiserIDs.Count];
        for (int s = 0; s < shipsToConcealIDs.Length; s++)
        {
            int[] ranges = new int[owner.board.ships.Length];
            for (int i = 0; i < owner.board.ships.Length; i++)
            {
                int lastRange = i > 0 ? ranges[i - 1] : 0;
                ranges[i] = lastRange + owner.board.ships[i].concealmentAIValue; ;
            }

            int chosen = UnityEngine.Random.Range(0, ranges[ranges.Length - 1] + 1);
            for (int i = 0; i < owner.board.ships.Length; i++)
            {
                if (chosen <= ranges[i])
                {
                    shipsToConcealIDs[s] = i;
                    break;
                }
            }
        }

        for (int i = 0; i < shipsToConcealIDs.Length; i++)
        {
            int shipID = shipsToConcealIDs[i];
            int cruiserID = cruiserIDs[i];
            shipLocationHeatmaps[cruiserID] = SumHeatmaps(shipLocationHeatmaps[cruiserID], MagnifyHeatmap(shipLocationHeatmaps[shipID], 3.0f));
        }


        //Sort the ships so they get placed in the right order
        List<int> sortedShipIDs = new List<int>();

        sortedShipIDs.AddRange(shipsToConcealIDs);
        sortedShipIDs.AddRange(cruiserIDs);

        for (int i = 0; i < owner.board.ships.Length; i++)
        {
            if (!sortedShipIDs.Contains(i))
            {
                sortedShipIDs.Add(i);
            }
        }



        //Place ships in whatever the best available spot left is
        foreach (int shipID in sortedShipIDs)
        {
            Ship ship = owner.board.ships[shipID];
            ship.Pickup();

            float[,] heatmap = shipLocationHeatmaps[shipID];

            for (int x = 0; x < ship.maxHealth; x++)
            {
                Tile bestChoice = owner.board.placementInfo.selectableTiles[0];

                foreach (Tile tile in owner.board.placementInfo.selectableTiles)
                {
                    if ((heatmap[tile.coordinates.x, tile.coordinates.y] > heatmap[bestChoice.coordinates.x, bestChoice.coordinates.y]) || (heatmap[tile.coordinates.x, tile.coordinates.y] == heatmap[bestChoice.coordinates.x, bestChoice.coordinates.y] && UnityEngine.Random.Range(0, 2) == 0))
                    {
                        bestChoice = tile;
                    }
                }

                owner.board.SelectTileForPlacement(bestChoice);
            }

            if (ship.type == ShipType.CRUISER)
            {
                ((Cruiser)ship).ConcealAlreadyPlacedShipsInConcealmentArea();
            }

            if (owner.board.placementInfo.selectableTiles.Count == 0)
            {
                PlaceShips();
                break;
            }
        }
    }

    float[,] ModifyHeatmap(float[,] currentHeatmap, Tile[] heatSources, float magnitude, float wholeDropoff)
    {
        float cycleModifier = 1.0f - wholeDropoff;

        foreach (Tile tile in heatSources)
        {
            for (int x = 0; x < currentHeatmap.GetLength(0); x++)
            {
                for (int y = 0; y < currentHeatmap.GetLength(1); y++)
                {
                    Vector2Int relative = new Vector2Int(x, y) - tile.coordinates;
                    int distance = Mathf.Abs(relative.x) + Mathf.Abs(relative.y);

                    currentHeatmap[x, y] += magnitude * Mathf.Pow(1.0f - wholeDropoff, distance);
                }
            }
        }

        return currentHeatmap;
    }

    float[,] SumHeatmaps(float[,] heatmap1, float[,] heatmap2)
    {
        for (int x = 0; x < heatmap1.GetLength(0); x++)
        {
            for (int y = 0; y < heatmap1.GetLength(1); y++)
            {
                heatmap1[x, y] += heatmap2[x, y];
            }
        }

        return heatmap1;
    }

    float[,] MagnifyHeatmap(float[,] heatmap, float magnitude)
    {
        for (int x = 0; x < heatmap.GetLength(0); x++)
        {
            for (int y = 0; y < heatmap.GetLength(1); y++)
            {
                heatmap[x, y] *= magnitude;
            }
        }

        return heatmap;
    }

}
