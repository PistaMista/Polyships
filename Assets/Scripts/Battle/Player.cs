using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    [Serializable]
    public struct PlayerData
    {
        public int index;
        public Board.BoardData board;
        public bool computerControlled;
        public float[,,] flag;
        public Ship.ShipData[] ships;
        public int intactShipCount;
        public int[,] hitTiles;
        public static implicit operator PlayerData(Player player)
        {
            PlayerData result = new PlayerData();
            result.index = player.index;
            result.board = player.board;
            result.computerControlled = player.computerControlled;
            result.flag = new float[player.flag.GetLength(0), player.flag.GetLength(1), 3];
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

            if (player.ships != null)
            {
                result.ships = new Ship.ShipData[player.ships.Length];
                for (int i = 0; i < player.ships.Length; i++)
                {
                    result.ships[i] = player.ships[i];
                }
            }


            result.intactShipCount = player.intactShipCount;
            result.hitTiles = new int[player.hitTiles.Count, 2];
            for (int i = 0; i < player.hitTiles.Count; i++)
            {
                Tile tile = player.hitTiles[i];
                result.hitTiles[i, 0] = (int)tile.coordinates.x;
                result.hitTiles[i, 1] = (int)tile.coordinates.y;
            }


            return result;
        }
    }
    public int index;
    public Board board;
    public bool computerControlled;
    public Color[,] flag;
    public Ship[] ships;
    public int intactShipCount;
    public List<Tile> hitTiles;




    public Waypoint boardCameraPoint;
    public Waypoint flagCameraPoint;
    public void Initialize(PlayerData data)
    {
        index = data.index;

        board = new GameObject("Board").AddComponent<Board>();
        board.transform.SetParent(transform);
        board.Initialize(data.board);

        computerControlled = data.computerControlled;

        flag = new Color[data.flag.GetLength(0), data.flag.GetLength(1)];
        for (int x = 0; x < flag.GetLength(0); x++)
        {
            for (int y = 0; y < flag.GetLength(1); y++)
            {
                flag[x, y] = new Color(data.flag[x, y, 0], data.flag[x, y, 1], data.flag[x, y, 2]);
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

        //hitTiles - REF

        boardCameraPoint = new GameObject("Camera Point").AddComponent<Waypoint>();
        boardCameraPoint.transform.SetParent(transform);
        boardCameraPoint.transform.localPosition = Vector3.up * (CameraControl.CalculateCameraWaypointHeight(new Vector2(board.tiles.GetLength(0) + 3, board.tiles.GetLength(1) + 3)) * MiscellaneousVariables.it.boardCameraHeightModifier + MiscellaneousVariables.it.boardUIRenderHeight);
        boardCameraPoint.transform.LookAt(transform);

        flagCameraPoint = new GameObject("Flag Camera Point").AddComponent<Waypoint>();
        flagCameraPoint.transform.SetParent(transform);
        flagCameraPoint.transform.localPosition = Vector3.up * (CameraControl.CalculateCameraWaypointHeight(MiscellaneousVariables.it.flagVoxelScale * new Vector2(flag.GetLength(0) + 3, flag.GetLength(1) + 3)) + MiscellaneousVariables.it.flagRenderHeight);
        flagCameraPoint.transform.LookAt(transform);
    }



    public void AssignReferences(PlayerData data)
    {
        board.AssignReferences(data.board);
        if (data.ships != null)
        {
            for (int i = 0; i < ships.Length; i++)
            {
                ships[i].AssignReferences(data.ships[i]);
            }
        }

        Board targetBoard = board == Battle.main.attacker.board ? Battle.main.defender.board : Battle.main.attacker.board;
        hitTiles = new List<Tile>();
        if (data.hitTiles != null)
        {
            for (int i = 0; i < data.hitTiles.GetLength(0); i++)
            {
                hitTiles.Add(targetBoard.tiles[data.hitTiles[i, 0], data.hitTiles[i, 1]]);
            }
        }
    }

    public void OnTurnStart()
    {
        if (ships != null)
        {
            for (int i = 0; i < ships.Length; i++)
            {
                ships[i].OnTurnStart();
            }
        }
    }

    public void OnTurnEnd()
    {
        if (ships != null)
        {
            for (int i = 0; i < ships.Length; i++)
            {
                ships[i].OnTurnEnd();
            }
        }
    }
}
