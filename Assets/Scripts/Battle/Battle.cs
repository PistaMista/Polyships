using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

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
            result.tutorialStage = battle.tutorialStage;
            return result;
        }
    }

    public static Battle main;
    public Player attacker;
    public Player defender;
    public List<TurnInfo> log;
    public int saveSlot;
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

        if (attacker.board.ships != null)
        {
            CollectAttackerCapabilities();
        }

        BattleUIMaster.EnablePrimaryBUI(BattleUIType.TURN_NOTIFIER);
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
        BattleUIMaster.ForceResetAllBUIs();
        MiscellaneousVariables.it.titleInterfaceMaster.State = UIState.ENABLED;
        Destroy(this.gameObject);
    }

    public void NextTurn()
    {
        Player lastAttacker = attacker;
        attacker = defender;
        defender = lastAttacker;

        lastAttacker.OnTurnEnd();
        attacker.OnTurnStart();

        if (attacker.board.ships != null)
        {
            log.Insert(0, new TurnInfo(1));
            CollectAttackerCapabilities();
        }

        SaveToDisk();
    }

    public struct AttackerCapabilities
    {
        public int maximumArtilleryCount;
        public int maximumTorpedoCount;
    }
    public AttackerCapabilities attackerCapabilities;
    void CollectAttackerCapabilities()
    {
        AttackerCapabilities gathered = new AttackerCapabilities();
        gathered.maximumArtilleryCount = 1;
        for (int i = 0; i < attacker.board.ships.Length; i++)
        {
            Ship ship = attacker.board.ships[i];
            if (ship.health > 0)
            {
                switch (ship.type)
                {
                    case ShipType.BATTLESHIP:
                        gathered.maximumArtilleryCount += ((Battleship)ship).artilleryBonus;
                        break;
                    case ShipType.DESTROYER:
                        gathered.maximumTorpedoCount += ((Destroyer)ship).torpedoCount;
                        break;
                }
            }
        }

        attackerCapabilities = gathered;
    }

    public void ExecuteArtilleryAttack(Tile[] targets)
    {
        //Check if the number of targets is correct
        if (targets.Length > attackerCapabilities.maximumArtilleryCount || targets.Length == 0)
        {
            Debug.LogWarning("ARTILLERY ATTACK has a possibly invalid number of targets: " + targets.Length + "/" + attackerCapabilities.maximumArtilleryCount);
        }

        //Determine the final hits to ships and other tiles
        List<Tile> actualHits = new List<Tile>();
        Dictionary<Ship, List<int>> shipHits = new Dictionary<Ship, List<int>>();
        for (int targetIndex = 0; targetIndex < targets.Length; targetIndex++)
        {
            Tile target = targets[targetIndex];

            //If there is a concealed ship in this tile displace the shot
            if (target.containedShip != null)
            {
                if (target.containedShip.concealedBy)
                {
                    for (int x = (target.coordinates.x == 0 ? 0 : -1); x <= ((target.coordinates.x == defender.board.tiles.GetLength(0) - 1) ? 0 : 1); x++)
                    {
                        for (int y = (target.coordinates.y == 0 ? 0 : -1); y <= ((target.coordinates.y == defender.board.tiles.GetLength(1) - 1) ? 0 : 1); y++)
                        {
                            if (!(y == 0 && x == 0))
                            {
                                Tile candidate = defender.board.tiles[x + (int)target.coordinates.x, y + (int)target.coordinates.y];
                                if (!attacker.hitTiles.Contains(candidate) && !actualHits.Contains(candidate) && candidate.containedShip == null)
                                {
                                    target = candidate;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (!attacker.hitTiles.Contains(target))
            {
                actualHits.Add(target);
                if (target.containedShip)
                {
                    if (!shipHits.ContainsKey(target.containedShip))
                    {
                        shipHits.Add(target.containedShip, new List<int>());
                    }

                    for (int i = 0; i < target.containedShip.tiles.Length; i++)
                    {
                        if (target.containedShip.tiles[i] == target)
                        {
                            shipHits[target.containedShip].Add(i);
                            break;
                        }
                    }
                }
            }
        }

        //Apply the hits
        log[0].artilleryImpacts.AddRange(actualHits);
        log[0].damagedTiles.AddRange(actualHits);

        actualHits.ForEach(x => x.hit = true);

        foreach (KeyValuePair<Ship, List<int>> shipHitInfo in shipHits)
        {
            log[0].damagedShips.Add(shipHitInfo.Key);
            shipHitInfo.Key.Damage(shipHitInfo.Value.ToArray());
            if (shipHitInfo.Key.health <= 0)
            {
                log[0].destroyedShips.Add(shipHitInfo.Key);
            }
        }

        attacker.hitTiles.AddRange(actualHits);

        NextTurn();
    }
}
