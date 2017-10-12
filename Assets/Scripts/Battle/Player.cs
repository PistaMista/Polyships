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


            return result;
        }
    }
    public int index;
    public Board board;
    public bool computerControlled;
    public Color[,] flag;
    public Ship[] ships;




    public Waypoint boardCameraPoint;
    public Waypoint flagCameraPoint;
    public PlayerSecondaryBUI secondaryBUI;
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
            for (int i = 0; i < data.ships.Length; i++)
            {
                Ship ship = Instantiate(MiscellaneousVariables.it.shipPrefabs[(int)data.ships[i].type]).GetComponent<Ship>();
                ship.transform.SetParent(transform);
                ship.Initialize(data.ships[i]);
            }
        }

        float fov = Camera.main.fieldOfView;
        float aspect = Camera.main.aspect;

        // float halfScreenWidthNormal = Mathf.Tan(fov / 2.0f * Mathf.Deg2Rad);
        // float modifiedFov = Mathf.Atan(halfScreenWidthNormal) * Mathf.Rad2Deg;

        boardCameraPoint = new GameObject("Camera Point").AddComponent<Waypoint>();
        boardCameraPoint.transform.SetParent(transform);
        float height = (1 / Mathf.Tan(Mathf.Deg2Rad * fov / 2.0f)) * (board.tiles.GetLength(0) / 2.0f) * MiscellaneousVariables.it.boardCameraHeightModifier + MiscellaneousVariables.it.boardUIRenderHeight;
        boardCameraPoint.transform.localPosition = Vector3.up * height;
        boardCameraPoint.transform.LookAt(transform);

        flagCameraPoint = new GameObject("Flag Camera Point").AddComponent<Waypoint>();
        flagCameraPoint.transform.SetParent(transform);
        height = Mathf.Tan(Mathf.Rad2Deg * (90 - fov / 2.0f)) * (flag.GetLength(0) / 2.0f);
        flagCameraPoint.transform.localPosition = Vector3.up * (height + MiscellaneousVariables.it.boardUIRenderHeight);
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
    }
}
