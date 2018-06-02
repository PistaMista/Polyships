using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using Gameplay.Ships;
using Gameplay.Effects;

namespace Gameplay
{
    public class Battle : BattleBehaviour
    {
        public struct TurnInfo
        {
            public Player attacker;
            public Player defender;
            public List<Tile> artilleryImpacts;
            public List<Tile> torpedoImpacts;
            public List<Tile> damagedTiles;
            public List<Ship> damagedShips;
            public List<Ship> destroyedShips;
            public bool aircraftTargetChanged;

            public TurnInfo(int r)
            {
                this.attacker = Battle.main.attacker;
                this.defender = Battle.main.defender;
                artilleryImpacts = new List<Tile>();
                torpedoImpacts = new List<Tile>();
                damagedTiles = new List<Tile>();
                damagedShips = new List<Ship>();
                destroyedShips = new List<Ship>();
                aircraftTargetChanged = false;
            }

            public static implicit operator TurnInfo(TurnInfoData data)
            {
                TurnInfo result = new TurnInfo(1);
                result.attacker = Battle.main.attacker.index == data.attacker ? Battle.main.attacker : Battle.main.defender;
                result.defender = Battle.main.defender.index == data.defender ? Battle.main.defender : Battle.main.attacker;
                result.artilleryImpacts = ConvertTileArray(data.artilleryImpacts, result.defender);
                result.torpedoImpacts = ConvertTileArray(data.torpedoImpacts, result.defender);
                result.damagedTiles = ConvertTileArray(data.damagedTiles, result.defender);
                result.damagedShips = ConvertShipArray(data.damagedShips, result.defender);
                result.destroyedShips = ConvertShipArray(data.destroyedShips, result.defender);
                result.aircraftTargetChanged = data.aircraftTargetChanged;
                return result;
            }

            static List<Tile> ConvertTileArray(int[,] array, Player owner)
            {
                List<Tile> result = new List<Tile>();
                for (int i = 0; i < array.GetLength(0); i++)
                {
                    result.Add(owner.board.tiles[array[i, 0], array[i, 1]]);
                }

                return result;
            }

            static List<Ship> ConvertShipArray(int[] array, Player owner)
            {
                List<Ship> result = new List<Ship>();
                for (int i = 0; i < array.Length; i++)
                {
                    result.Add(owner.board.ships[array[i]]);
                }

                return result;
            }
        }

        [Serializable]
        public struct TurnInfoData
        {
            public int attacker;
            public int defender;
            public int[,] artilleryImpacts;
            public int[,] torpedoImpacts;
            public int[,] damagedTiles;
            public int[] damagedShips;
            public int[] destroyedShips;
            public bool aircraftTargetChanged;

            public static implicit operator TurnInfoData(TurnInfo info)
            {
                TurnInfoData result = new TurnInfoData();
                result.attacker = info.attacker.index;
                result.defender = info.defender.index;
                result.artilleryImpacts = ConvertTileArray(info.artilleryImpacts.ToArray());
                result.torpedoImpacts = ConvertTileArray(info.torpedoImpacts.ToArray());
                result.damagedTiles = ConvertTileArray(info.damagedTiles.ToArray());
                result.damagedShips = ConvertShipArray(info.damagedShips.ToArray());
                result.destroyedShips = ConvertShipArray(info.destroyedShips.ToArray());
                result.aircraftTargetChanged = info.aircraftTargetChanged;
                return result;
            }

            static int[,] ConvertTileArray(Tile[] array)
            {
                int[,] result = new int[array.Length, 2];
                for (int i = 0; i < array.Length; i++)
                {
                    result[i, 0] = (int)array[i].coordinates.x;
                    result[i, 1] = (int)array[i].coordinates.y;
                }

                return result;
            }

            static int[] ConvertShipArray(Ship[] array)
            {
                int[] result = new int[array.Length];
                for (int i = 0; i < array.Length; i++)
                {
                    result[i] = array[i].index;
                }

                return result;
            }
        }

        [Serializable]
        public struct BattleData
        {
            public Player.PlayerData attacker;
            public Player.PlayerData defender;
            public int saveSlot;
            public TurnInfoData[] log;
            public Effect.EffectData[] effects;
            public int tutorialStage;

            public static implicit operator BattleData(Battle battle)
            {
                BattleData result = new BattleData();
                result.attacker = battle.attacker;
                result.defender = battle.defender;
                result.saveSlot = battle.saveSlot;
                result.log = new TurnInfoData[battle.log.Count];
                for (int i = 0; i < battle.log.Count; i++)
                {
                    result.log[i] = battle.log[i];
                }
                result.effects = new Effect.EffectData[battle.effects.Count];
                for (int i = 0; i < battle.effects.Count; i++)
                {
                    result.effects[i] = battle.effects[i];
                }
                result.tutorialStage = battle.tutorialStage;
                return result;
            }
        }

        public static Battle main;
        public Player attacker;
        public Player defender;
        public List<TurnInfo> log;
        public List<Effect> effects = new List<Effect>(); //Effects change the battle parameters - at the start/end of each turn or when another effect is added/removed. Effects are applied in the order of this list.
        public int saveSlot;
        public int tutorialStage;
        public bool fighting
        {
            get
            {
                return attacker.board.ships != null && defender.board.ships != null;
            }
        }
        public void SaveToDisk()
        {
            bool success = true;
            BattleData saveData;
            try
            {
                saveData = (BattleData)this;
            }
            catch (System.Exception)
            {
                success = false;
                Debug.LogError("An error occured during saving the game state.");
                throw;
            }

            if (success)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream stream = new FileStream(Path.Combine(Application.persistentDataPath, saveSlot.ToString()), FileMode.Create);

                formatter.Serialize(stream, saveData);

                stream.Close();
            }
        }

        public void Initialize(BattleData data)
        {
            main = this;

            attacker = Instantiate(MiscellaneousVariables.it.playerPrefab).GetComponent<Player>();
            attacker.name = "Attacker";
            attacker.gameObject.transform.SetParent(transform);
            attacker.Initialize(data.attacker);

            defender = Instantiate(MiscellaneousVariables.it.playerPrefab).GetComponent<Player>();
            defender.name = "Defender";
            defender.gameObject.transform.SetParent(transform);
            defender.Initialize(data.defender);

            attacker.transform.position = Vector3.left * MiscellaneousVariables.it.boardDistanceFromCenter;
            defender.transform.position = Vector3.right * MiscellaneousVariables.it.boardDistanceFromCenter;

            //LOG - REF

            for (int i = 0; i < data.effects.Length; i++)
            {
                Effect initializedEffect = Instantiate(MiscellaneousVariables.it.effectPrefabs[data.effects[i].prefabIndex]).gameObject.GetComponent<Effect>();
                initializedEffect.Initialize(data.effects[i]);
                effects.Add(initializedEffect);
            }

            saveSlot = data.saveSlot;
            tutorialStage = data.tutorialStage;
        }

        public void AssignReferences(BattleData data)
        {
            attacker.AssignReferences(data.attacker);
            defender.AssignReferences(data.defender);

            log = new List<TurnInfo>();
            if (data.log != null)
            {
                for (int i = 0; i < data.log.Length; i++)
                {
                    TurnInfo turn = data.log[i];

                    log.Add(turn);
                }
            }

            for (int i = 0; i < effects.Count; i++)
            {
                effects[i].AssignReferences(data.effects[i]);
            }



            attacker.OnTurnResume();
            foreach (Effect effect in effects)
            {
                effect.OnTurnResume();
            }

            //BattleUIMaster.EnablePrimaryBUI(BattleUIType.TURN_NOTIFIER);
        }

        void OnApplicationPause(bool pauseStatus)
        {
            //SaveToDisk();
            //QuitBattle();
        }

        void OnApplicationQuit()
        {
            QuitBattle();
        }

        public void QuitBattle()
        {
            BattleUIAgents.Base.ScreenBattleUIAgent.DelinkAllScreenAgents();
            Destroy(this.gameObject);
        }

        public void NextTurn()
        {
            OnTurnEnd();

            Player lastAttacker = attacker;
            attacker = defender;
            defender = lastAttacker;

            OnTurnStart();
            SaveToDisk();
        }

        /// <summary>
        /// Executes every time a new turn starts.
        /// </summary>
        public override void OnTurnStart()
        {
            base.OnTurnStart();
            OnTurnResume();

            if (fighting)
            {
                log.Insert(0, new TurnInfo(1));
                Event.ConsiderEvents();
            }

            attacker.OnTurnStart();

            if (Effect.pre_action != null)
            {
                Effect.pre_action();
                Effect.pre_action = null;
            }
            foreach (Effect effect in effects)
            {
                effect.OnTurnStart();
            }
        }

        /// <summary>
        /// Executes every time a game is loaded and the current turn is therefore resumed.
        /// </summary>
        public override void OnTurnResume()
        {
            base.OnTurnResume();
            attacker.OnTurnResume();
            foreach (Effect effect in effects)
            {
                effect.OnTurnResume();
            }
        }

        /// <summary>
        /// Executes every time a turn ends.
        /// </summary>
        public override void OnTurnEnd()
        {
            base.OnTurnEnd();
            attacker.OnTurnEnd();
            foreach (Effect effect in effects)
            {
                effect.OnTurnEnd();
            }

            if (Effect.post_action != null)
            {
                Effect.post_action();
                Effect.post_action = null;
            }
        }

        public static Battle CreateBattle(BattleData data)
        {
            Battle battle = new GameObject("Battle").AddComponent<Battle>();
            battle.Initialize(data);
            battle.AssignReferences(data);
            if (battle.effects.Count == 0)
            {
                Effect attackerAmmo = Effect.CreateEffect(typeof(AmmoRegistry));
                Effect defenderAmmo = Effect.CreateEffect(typeof(AmmoRegistry));

                attackerAmmo.targetedPlayer = battle.attacker;
                defenderAmmo.targetedPlayer = battle.defender;

                Effect.AddToQueue(attackerAmmo);
                Effect.AddToQueue(defenderAmmo);
            }

            battle.SaveToDisk();
            return battle;
        }

        public static BattleData GetBlankBattleData(int boardSideLength, bool aiOpponent, bool tutorialEnabled, int saveSlot, float[][,,] flags)
        {
            BattleData data = new BattleData();

            Board.BoardData boardData = new Board.BoardData();
            boardData.ownedByAttacker = true;
            boardData.tiles = new Tile.TileData[boardSideLength, boardSideLength];
            for (int x = 0; x < boardSideLength; x++)
            {
                for (int y = 0; y < boardSideLength; y++)
                {
                    boardData.tiles[x, y].coordinates = new int[] { x, y };
                    boardData.tiles[x, y].containedShip = -1;
                    boardData.tiles[x, y].ownedByAttacker = true;
                }
            }

            data.attacker.board = boardData;
            data.attacker.heatmap_recon = new Heatmap(boardSideLength, boardSideLength);
            data.attacker.flag = flags[0];

            boardData = new Board.BoardData();
            boardData.tiles = new Tile.TileData[boardSideLength, boardSideLength];
            for (int x = 0; x < boardSideLength; x++)
            {
                for (int y = 0; y < boardSideLength; y++)
                {
                    boardData.tiles[x, y].coordinates = new int[] { x, y };
                    boardData.tiles[x, y].containedShip = -1;
                }
            }

            data.defender.index = 1;
            data.defender.board = boardData;
            data.defender.flag = flags[1];
            data.defender.heatmap_recon = new Heatmap(boardSideLength, boardSideLength);
            data.defender.aiEnabled = aiOpponent;

            data.tutorialStage = tutorialEnabled ? 1 : 0;
            data.saveSlot = saveSlot;
            data.effects = new Effect.EffectData[0];

            return data;
        }
    }
}