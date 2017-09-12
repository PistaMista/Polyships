using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public struct BoardData
    {
        public static implicit operator BoardData(Board board)
        {
            return null;
        }
    }

    public Tile[,] tiles;
}
