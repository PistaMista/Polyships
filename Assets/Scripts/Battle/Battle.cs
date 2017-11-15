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
                result.Add(owner.ships[array[i]]);
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
        public BattleStage stage;
        public TurnInfoData[] log;
        public int tutorialStage;

        public static implicit operator BattleData(Battle battle)
        {
            BattleData result = new BattleData();
            result.attacker = battle.attacker;
            result.defender = battle.defender;
            result.saveSlot = battle.saveSlot;
            result.stage = battle.stage;
            result.log = new TurnInfoData[battle.log.Count];
            for (int i = 0; i < battle.log.Count; i++)
            {
                result.log[i] = battle.log[i];
            }
            result.tutorialStage = battle.tutorialStage;
            return result;
        }
    }

    public static Battle main;
    public Player attacker;
    public Player defender;
    public List<TurnInfo> log;
    public int saveSlot;
    public BattleStage stage;
    public int tutorialStage;
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

        defender = new GameObject("Defender").AddComponent<Player>();
        defender.gameObject.transform.SetParent(transform);
        defender.Initialize(data.defender);

        //LOG - REF

        saveSlot = data.saveSlot;
        stage = data.stage;
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

        attacker.transform.position = Vector3.left * MiscellaneousVariables.it.boardDistanceFromCenter;
        defender.transform.position = Vector3.right * MiscellaneousVariables.it.boardDistanceFromCenter;

        BattleUserInterface_Master.EnablePrimaryBUI(BattleUIType.TURN_NOTIFIER);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        //SaveToDisk();
        //QuitBattle();
    }

    void OnApplicationQuit()
    {
        SaveToDisk();
    }

    public void QuitBattle()
    {
        SaveToDisk();
        BattleUserInterface_Master.ForceResetAllBUIs();
        MiscellaneousVariables.it.titleInterfaceMaster.State = UIState.ENABLED;
        Destroy(this.gameObject);
    }

    public void NextTurn()
    {


        Player lastAttacker = attacker;
        attacker = defender;
        defender = lastAttacker;

        if (attacker.ships != null)
        {
            log.Insert(0, new TurnInfo(1));
        }

        SaveToDisk();
    }
}
