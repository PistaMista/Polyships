using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Board : MonoBehaviour
{
    [Serializable]
    public struct BoardData
    {
        public bool ownedByAttacker;
        public Tile.TileData[,] tiles;
        public Ship.ShipData[] ships;
        public int intactShipCount;

        public static implicit operator BoardData(Board board)
        {
            BoardData result = new BoardData();
            result.ownedByAttacker = board.owner == Battle.main.attacker;
            result.tiles = new Tile.TileData[board.tiles.GetLength(0), board.tiles.GetLength(1)];
            for (int x = 0; x < result.tiles.GetLength(0); x++)
            {
                for (int y = 0; y < result.tiles.GetLength(1); y++)
                {
                    result.tiles[x, y] = board.tiles[x, y];
                }
            }

            if (board.ships != null)
            {
                result.ships = new Ship.ShipData[board.ships.Length];
                for (int i = 0; i < board.ships.Length; i++)
                {
                    result.ships[i] = board.ships[i];
                }
            }

            result.intactShipCount = board.intactShipCount;
            return result;
        }
    }
    public Player owner;
    public Tile[,] tiles;
    public Ship[] ships;
    public int intactShipCount;

    public void Initialize(BoardData data)
    {
        //owner - REF
        tiles = new Tile[data.tiles.GetLength(0), data.tiles.GetLength(1)];

        Vector3 startingPosition = new Vector3(-tiles.GetLength(0) / 2.0f + 0.5f, 0, -tiles.GetLength(1) / 2.0f + 0.5f);
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                tiles[x, y] = new GameObject("Tile X:" + x + " Y:" + y).AddComponent<Tile>();
                tiles[x, y].transform.SetParent(transform);
                tiles[x, y].transform.localPosition = startingPosition + new Vector3(x, 0, y);
                tiles[x, y].Initialize(data.tiles[x, y]);
            }
        }

        if (data.ships != null)
        {
            ships = new Ship[data.ships.Length];
            for (int i = 0; i < data.ships.Length; i++)
            {
                Ship ship = Instantiate(MiscellaneousVariables.it.shipPrefabs[(int)data.ships[i].type]).GetComponent<Ship>();
                ship.transform.SetParent(transform);
                ship.Initialize(data.ships[i]);
                ships[i] = ship;
            }
        }

        intactShipCount = data.intactShipCount;
    }

    public void AssignReferences(BoardData data)
    {
        owner = data.ownedByAttacker ? Battle.main.attacker : Battle.main.defender;
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                tiles[x, y].AssignReferences(data.tiles[x, y]);
            }
        }

        if (data.ships != null)
        {
            for (int i = 0; i < ships.Length; i++)
            {
                ships[i].AssignReferences(data.ships[i]);
            }
        }
    }

    //PLACEMENT FUNCTIONS
    public struct PlacementInfo
    {
        public List<Tile> selectedTiles; //List of tiles selected to house the currently selected ship
        public List<Tile> selectableTiles; //List of selectable tiles
        public List<Tile> obstructedTiles; //List of tiles where nothing can be placed
        public Dictionary<Tile, int> validTiles; //List of tiles where the current ship can be placed
        public List<Tile> invalidTiles; //List of tiles where the current ship cannot be placed

        public List<Tile> occupiedTiles
        {
            get
            {
                List<Tile> result = new List<Tile>();
                placedShips.ForEach(x => { if (x.tiles != null) result.AddRange(x.tiles); });
                return result;
            }
        }

        public Ship selectedShip;
        public List<Ship> notplacedShips;
        public List<Ship> placedShips;
        public List<Ship> allShips;
    }

    public PlacementInfo placementInfo;

    public void SpawnShips()
    {
        placementInfo.allShips = new List<Ship>();
        placementInfo.placedShips = new List<Ship>();
        placementInfo.notplacedShips = new List<Ship>();
        for (int i = 0; i < MiscellaneousVariables.it.defaultShipLoadout.Length; i++)
        {
            Ship ship = Instantiate(MiscellaneousVariables.it.defaultShipLoadout[i]).GetComponent<Ship>();
            placementInfo.notplacedShips.Add(ship);
            placementInfo.allShips.Add(ship);
            ship.placementInfo.lastLocation = null;
            ship.index = i;

            ship.parentBoard = this;
            ship.transform.SetParent(transform);
        }

        ships = placementInfo.allShips.ToArray();
    }

    public void ReevaluateTiles()
    {
        Vector2 boardSize = new Vector2(tiles.GetLength(0), tiles.GetLength(1));

        placementInfo.obstructedTiles = new List<Tile>();

        //Determine the tiles in which a 1-tile sized ship cannot be placed
        foreach (Tile tile in placementInfo.occupiedTiles)
        {
            for (int x = (tile.coordinates.x == 0 ? 0 : -1); x <= ((tile.coordinates.x == boardSize.x - 1) ? 0 : 1); x++)
            {
                for (int y = (tile.coordinates.y == 0 ? 0 : -1); y <= ((tile.coordinates.y == boardSize.y - 1) ? 0 : 1); y++)
                {
                    Tile obstructedTile = tiles[x + (int)tile.coordinates.x, y + (int)tile.coordinates.y];
                    if (!placementInfo.obstructedTiles.Contains(obstructedTile))
                    {
                        placementInfo.obstructedTiles.Add(obstructedTile);
                    }
                }
            }
        }



        placementInfo.invalidTiles = new List<Tile>();
        placementInfo.validTiles = new Dictionary<Tile, int>();

        for (int x = 0; x < boardSize.x; x++)
        {
            for (int y = 0; y < boardSize.y; y++)
            {
                placementInfo.invalidTiles.Add(tiles[x, y]);
            }
        }

        //Determine where the current ship can or cannot be placed
        for (int axis = 0; axis < 2; axis++) //The axis we are sweeping across
        {
            for (int line = 0; line < (axis == 0 ? boardSize.y : boardSize.x); line++)
            {
                List<Tile> inlineValidTiles = new List<Tile>();
                List<Tile> inlineNeighbouringValidTiles = new List<Tile>();
                for (int depth = 0; depth < (axis == 0 ? boardSize.x : boardSize.y); depth++)
                {
                    Tile examined = tiles[axis == 0 ? depth : line, axis == 0 ? line : depth];
                    if (!placementInfo.obstructedTiles.Contains(examined))
                    {
                        inlineNeighbouringValidTiles.Add(examined);
                    }
                    else
                    {
                        if (inlineNeighbouringValidTiles.Count >= placementInfo.selectedShip.maxHealth)
                        {
                            inlineValidTiles.AddRange(inlineNeighbouringValidTiles);
                        }
                        inlineNeighbouringValidTiles = new List<Tile>();
                    }
                }

                if (placementInfo.selectedShip && inlineNeighbouringValidTiles.Count >= placementInfo.selectedShip.maxHealth)
                {
                    inlineValidTiles.AddRange(inlineNeighbouringValidTiles);
                }

                foreach (Tile tile in inlineValidTiles)
                {
                    if (!placementInfo.validTiles.ContainsKey(tile))
                    {
                        placementInfo.validTiles.Add(tile, axis);
                        placementInfo.invalidTiles.Remove(tile);
                    }
                    else
                    {
                        placementInfo.validTiles[tile] = -1;
                    }
                }
            }
        }

        placementInfo.selectedTiles = new List<Tile>();
        placementInfo.selectableTiles = new List<Tile>(placementInfo.validTiles.Keys);
    }

    public bool SelectTileForPlacement(Tile tile)
    {
        bool selectTile = placementInfo.selectedShip != null && IsTileValidForSelection(tile);

        if (selectTile)
        {
            placementInfo.selectableTiles = new List<Tile>();

            if (placementInfo.selectedTiles.Count > 1)
            {
                if (Vector2Int.Distance(placementInfo.selectedTiles[0].coordinates, tile.coordinates) == 1)
                {
                    placementInfo.selectedTiles.Insert(0, tile);
                }
                else
                {
                    placementInfo.selectedTiles.Add(tile);
                }
            }
            else
            {
                placementInfo.selectedTiles.Add(tile);

            }

            if (placementInfo.selectedTiles.Count == placementInfo.selectedShip.maxHealth)
            {
                placementInfo.selectedShip.Place(placementInfo.selectedTiles.ToArray());
                placementInfo.selectedTiles = new List<Tile>();
            }
            else
            {
                for (int tileEnd = 0; tileEnd < (placementInfo.selectedTiles.Count > 1 ? 2 : 1); tileEnd++)
                {
                    Tile end = tileEnd == 0 ? placementInfo.selectedTiles[0] : placementInfo.selectedTiles[placementInfo.selectedTiles.Count - 1];
                    for (int i = 1; i <= 4; i++)
                    {
                        Vector2Int pos = end.coordinates + new Vector2Int(i == 2 ? 1 : i == 4 ? -1 : 0, i == 1 ? 1 : i == 3 ? -1 : 0);
                        if (pos.x >= 0 && pos.x < tiles.GetLength(0) && pos.y >= 0 && pos.y < tiles.GetLength(1))
                        {
                            Tile consideredTile = tiles[pos.x, pos.y];
                            if (IsTileValidForSelection(consideredTile))
                            {
                                placementInfo.selectableTiles.Add(consideredTile);
                            }
                        }
                    }
                }
            }
        }

        return selectTile;
    }



    bool IsTileValidForSelection(Tile tile)
    {
        if (!placementInfo.selectedTiles.Contains(tile) && placementInfo.validTiles.ContainsKey(tile))
        {
            if (placementInfo.selectedTiles.Count == 0)
            {
                return true;
            }
            else
            {
                int directional = placementInfo.validTiles[tile];
                if (directional > -1 && ((placementInfo.selectedTiles[0].coordinates - tile.coordinates).x != 0 ? 0 : 1) != directional)
                {
                    return false;
                }

                bool connects = false;
                bool outOfLine = false;

                foreach (Tile existingTile in placementInfo.selectedTiles)
                {
                    float distance = Vector2.Distance(existingTile.coordinates, tile.coordinates);
                    if ((int)distance != distance)
                    {
                        outOfLine = true;
                        break;
                    }

                    if (distance == 1)
                    {
                        connects = true;
                    }
                }

                return connects && !outOfLine;
            }
        }

        return false;
    }
}
