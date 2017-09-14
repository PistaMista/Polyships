using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public struct BoardData
    {
        public Tile.TileData[,] tiles;
        public static implicit operator BoardData(Board board)
        {
            BoardData result = new BoardData();
            result.tiles = new Tile.TileData[board.tiles.GetLength(0), board.tiles.GetLength(1)];
            for (int x = 0; x < result.tiles.GetLength(0); x++)
            {
                for (int y = 0; y < result.tiles.GetLength(1); y++)
                {
                    result.tiles[x, y] = board.tiles[x, y];
                }
            }
            return null;
        }
    }

    public Tile[,] tiles;
}
