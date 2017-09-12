using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public struct PlayerData
    {
        public static implicit operator PlayerData(Player player)
        {
            return null;
        }
    }

    public Board board;
    public bool computerControlled;
    public Color[,] flag;
    public Ship[] ships;
}
