using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

enum BattleStage 
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
    }
    
    [Serializable]
    public struct BattleData
    {
        public static implicit operator BattleData(Battle battle)
        {
            return null;
        }
    }

    public static Battle main;
    public Player attacker;
    public Player attacked;
    public List<TurnInfo> log;
    public int saveSlot;
    public BattleStage stage;

    public void SaveToDisk()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(Path.Combine(Application.persistentDataPath, saveSlot.ToString()), FileMode.Create);

        formatter.Serialize(stream, (BattleData)this);

        stream.Close();
    }
}
