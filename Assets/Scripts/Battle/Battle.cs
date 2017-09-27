using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public enum BattleStage
{
    NOT_INITIALIZED,
    SHIP_PLACEMENT,
    FIGHTING,
    FINISHED
}

public class Battle : MonoBehaviour
{
    public struct TurnInfo
    {
        public Player attacker;
        public Player attacked;
        public List<Tile> artilleryImpacts;
        public List<Tile> torpedoImpacts;
        public List<Tile> damagedTiles;
        public List<Ship> damagedShips;
        public List<Ship> destroyedShips;
        public bool aircraftTargetChanged;

        public TurnInfo(int r)
        {
            this.attacker = Battle.main.attacker;
            this.attacked = Battle.main.attacked;
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
            result.attacker = Battle.main.attacker.index == data.attacker ? Battle.main.attacker : Battle.main.attacked;
            result.attacked = Battle.main.attacked.index == data.attacked ? Battle.main.attacked : Battle.main.attacker;
            result.artilleryImpacts = ConvertTileArray(data.artilleryImpacts, result.attacked);
            result.torpedoImpacts = ConvertTileArray(data.torpedoImpacts, result.attacked);
            result.damagedTiles = ConvertTileArray(data.damagedTiles, result.attacked);
            result.damagedShips = ConvertShipArray(data.damagedShips, result.attacked);
            result.destroyedShips = ConvertShipArray(data.destroyedShips, result.attacked);
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
                result.Add(owner.ships[array[i]]);
            }

            return result;
        }
    }

    [Serializable]
    public struct TurnInfoData
    {
        public int attacker;
        public int attacked;
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
            result.attacked = info.attacked.index;
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
        public Player.PlayerData attacked;
        public int saveSlot;
        public BattleStage stage;
        public TurnInfoData[] log;
        public int tutorialStage;
        public BattleUIType lastOpenUserInterface;

        public static implicit operator BattleData(Battle battle)
        {
            BattleData result = new BattleData();
            result.attacker = battle.attacker;
            result.attacked = battle.attacked;
            result.saveSlot = battle.saveSlot;
            result.stage = battle.stage;
            result.log = new TurnInfoData[battle.log.Count];
            for (int i = 0; i < battle.log.Count; i++)
            {
                result.log[i] = battle.log[i];
            }
            result.tutorialStage = battle.tutorialStage;
            result.lastOpenUserInterface = battle.lastOpenUserInterface;
            return result;
        }
    }

    public static Battle main;
    public Player attacker;
    public Player attacked;
    public List<TurnInfo> log;
    public int saveSlot;
    public BattleStage stage;
    public int tutorialStage;
    public BattleUIType lastOpenUserInterface;

    public void SaveToDisk()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(Path.Combine(Application.persistentDataPath, saveSlot.ToString()), FileMode.Create);

        formatter.Serialize(stream, (BattleData)this);

        stream.Close();
    }

    public void Initialize(BattleData data)
    {
        main = this;

        attacker = new GameObject("Attacker").AddComponent<Player>();
        attacker.gameObject.transform.SetParent(transform);
        attacker.Initialize(data.attacker);

        attacked = new GameObject("Attacked").AddComponent<Player>();
        attacked.gameObject.transform.SetParent(transform);
        attacked.Initialize(data.attacked);

        //LOG - REF

        saveSlot = data.saveSlot;
        stage = data.stage;
        tutorialStage = data.tutorialStage;
        lastOpenUserInterface = data.lastOpenUserInterface;
        BattleUserInterface_Master.EnableUI(lastOpenUserInterface);
    }

    public void AssignReferences(BattleData data)
    {
        attacker.AssignReferences(data.attacker);
        attacked.AssignReferences(data.attacked);

        log = new List<TurnInfo>();
        if (data.log != null)
        {
            for (int i = 0; i < data.log.Length; i++)
            {
                TurnInfo turn = data.log[i];

                log.Add(turn);
            }
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        QuitBattle();
    }

    void OnApplicationQuit()
    {
        QuitBattle();
    }

    public void QuitBattle()
    {
        SaveToDisk();
        BattleUserInterface_Master.Disable();
    }
}
