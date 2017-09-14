using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public struct PlayerData
    {
        public int index;
        public Board.BoardData board;
        public bool computerControlled;
        public float[,,] flag;
        public Ship.ShipData[] ships;
        public static implicit operator PlayerData(Player player)
        {
            PlayerData result = new PlayerData();
            result.index = player.index;
            result.board = player.board;
            result.computerControlled = player.computerControlled;
            result.flag = new float[player.flag.GetLength(0), player.flag.GetLength(0), 3];
            for (int x = 0; x < player.flag.GetLength(0); x++)
            {
                for (int y = 0; y < player.flag.GetLength(1); y++)
                {
                    Color color = player.flag[x, y];
                    result.flag[x, y, 0] = color.r;
                    result.flag[x, y, 1] = color.g;
                    result.flag[x, y, 2] = color.b;
                }
            }

            result.ships = new Ship.ShipData[player.ships.Length];
            for (int i = 0; i < player.ships.Length; i++)
            {
                result.ships[i] = player.ships[i];
            }
            return null;
        }
    }
    public int index;
    public Board board;
    public bool computerControlled;
    public Color[,] flag;
    public Ship[] ships;
}
