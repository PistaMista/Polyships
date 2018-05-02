using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using BattleUIAgents.Base;

using Gameplay.Effects;

namespace Gameplay
{
    public class Player : BattleBehaviour
    {
        [Serializable]
        public struct PlayerData
        {
            public int index;
            public Board.BoardData board;
            public bool aiEnabled;
            public AI.AIModuleData aIModuleData;
            public float[,,] flag;
            public int[,] hitTiles;
            public static implicit operator PlayerData(Player player)
            {
                PlayerData result = new PlayerData();
                result.index = player.index;
                result.board = player.board;
                result.aiEnabled = player.aiEnabled;
                result.aIModuleData = player.aiEnabled ? player.aiModule : new AI.AIModuleData();
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
        public bool aiEnabled;
        public Color[,] flag;
        public AmmoRegistry arsenal
        {
            get
            {
                return Effect.GetEffectsInQueue(x => { return x.targetedPlayer == this; }, typeof(AmmoRegistry), 1)[0] as AmmoRegistry;
            }
        }


        public AI aiModule;
        public void Initialize(PlayerData data)
        {
            index = data.index;
            board = new GameObject("Board").AddComponent<Board>();
            board.transform.SetParent(transform);
            board.Initialize(data.board);

            aiEnabled = data.aiEnabled;
            if (aiEnabled)
            {
                aiModule = (AI)ScriptableObject.CreateInstance(typeof(AI));
                aiModule.owner = this;
                aiModule.Initialize(data.aIModuleData);
            }

            flag = new Color[data.flag.GetLength(0), data.flag.GetLength(1)];
            for (int x = 0; x < flag.GetLength(0); x++)
            {
                for (int y = 0; y < flag.GetLength(1); y++)
                {
                    flag[x, y] = new Color(data.flag[x, y, 0], data.flag[x, y, 1], data.flag[x, y, 2]);
                }
            }

            //hitTiles - REF
        }



        public void AssignReferences(PlayerData data)
        {
            board.AssignReferences(data.board);

            Board targetBoard = board == Battle.main.attacker.board ? Battle.main.defender.board : Battle.main.attacker.board;
            hitTiles = new List<Tile>();
            if (data.hitTiles != null)
            {
                for (int i = 0; i < data.hitTiles.GetLength(0); i++)
                {
                    hitTiles.Add(targetBoard.tiles[data.hitTiles[i, 0], data.hitTiles[i, 1]]);
                }
            }

            if (aiEnabled)
            {
                aiModule.AssignReferences(data.aIModuleData);
            }
        }

        /// <summary>
        /// Executes every time a new turn starts.
        /// </summary>
        public override void OnTurnStart()
        {
            base.OnTurnStart();
            if (board.ships != null)
            {
                for (int i = 0; i < board.ships.Length; i++)
                {
                    board.ships[i].OnTurnStart();
                }
            }
        }

        /// <summary>
        /// Executes every time a game is loaded and the current turn is therefore resumed.
        /// </summary>
        public override void OnTurnResume()
        {
            base.OnTurnResume();
            if (board.ships != null)
            {
                for (int i = 0; i < board.ships.Length; i++)
                {
                    board.ships[i].OnTurnResume();
                }
            }
        }

        /// <summary>
        /// Executes every time a turn ends.
        /// </summary>
        public override void OnTurnEnd()
        {
            base.OnTurnEnd();
            if (board.ships != null)
            {
                for (int i = 0; i < board.ships.Length; i++)
                {
                    board.ships[i].OnTurnEnd();
                }
            }
        }
    }
}