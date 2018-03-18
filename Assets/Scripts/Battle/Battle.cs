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
    public List<Effect> effects = new List<Effect>(); //Effects change the battle parameters - at the start/end of each turn or when another effect is added/removed. Effects are applied in the order of this list.
    public int saveSlot;
    public int tutorialStage;
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

        //BattleUIMaster.EnablePrimaryBUI(BattleUIType.TURN_NOTIFIER);
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
        //BattleUIMaster.ForceResetAllBUIs();
        MiscellaneousVariables.it.titleUI.State = UIState.ENABLED;
        Destroy(this.gameObject);
    }

    public void NextTurn()
    {
        attacker.OnTurnEnd();
        foreach (Effect effect in effects)
        {
            effect.OnTurnEnd();
        }

        Effect.RemoveExpiredEffectsFromQueue();

        Player lastAttacker = attacker;
        attacker = defender;
        defender = lastAttacker;

        if (attacker.board.ships != null)
        {
            log.Insert(0, new TurnInfo(1));
            CollectAttackerCapabilities();
        }

        attacker.OnTurnStart();
        foreach (Effect effect in effects)
        {
            effect.OnTurnStart();
        }

        SaveToDisk();
    }

    public struct AttackerCapabilities
    {
        public int maximumArtilleryCount;
        public int maximumTorpedoCount;
        public int maximumAircraftCount;
        public bool[] torpedoFiringArea;
        public int torpedoFiringAreaSize;
        public int[,] airReconResults;
    }
    public AttackerCapabilities attackerCapabilities;
    void CollectAttackerCapabilities()
    {
        AttackerCapabilities gathered = new AttackerCapabilities();
        gathered.maximumArtilleryCount = 1;
        gathered.torpedoFiringArea = new bool[attacker.board.tiles.GetLength(0)];

        List<int[]> airReconResults = new List<int[]>();
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
                        Destroyer destroyer = (Destroyer)ship;
                        gathered.maximumTorpedoCount += destroyer.torpedoCount;
                        for (int x = 0; x < destroyer.firingAreaBlockages.Length; x++)
                        {
                            if (destroyer.firingAreaBlockages[x] < 0)
                            {
                                gathered.torpedoFiringArea[x] = true;
                                gathered.torpedoFiringAreaSize++;
                            }
                        }
                        break;
                    case ShipType.CARRIER:
                        Carrier carrier = (Carrier)ship;
                        gathered.maximumAircraftCount += carrier.aircraftCount;
                        for (int resultID = 0; resultID < carrier.reconResults.GetLength(0); resultID++)
                        {
                            airReconResults.Add(new int[] { carrier.reconResults[resultID, 0], carrier.reconResults[resultID, 1] });
                        }
                        break;
                }
            }
        }

        gathered.airReconResults = new int[airReconResults.Count, 2];

        for (int i = 0; i < airReconResults.Count; i++)
        {
            gathered.airReconResults[i, 0] = airReconResults[i][0];
            gathered.airReconResults[i, 1] = airReconResults[i][1];
        }

        attackerCapabilities = gathered;
    }

    //Inserts an effect into the queue based on its priority if it doesnt conflict with any other effect present
    // public bool AddEffect(Effect effect)
    // {
    //     if (effect.GetAdditionalAllowed() <= 0)
    //     {
    //         return false;
    //     }

    //     int insertionIndex = 0;
    //     foreach (Effect measure in effects)
    //     {
    //         if (measure.priority >= effect.priority)
    //         {
    //             insertionIndex++;
    //         }
    //         else
    //         {
    //             break;
    //         }
    //     }

    //     effects.Insert(insertionIndex, effect);
    //     foreach (Effect affected in effects)
    //     {
    //         if (affected != effect)
    //         {
    //             affected.OnOtherEffectAdd(effect);
    //         }
    //     }

    //     effect.transform.SetParent(transform);
    //     return true;
    // }

    // public bool EffectAllowed(Effect effect)
    // {
    //     foreach (Effect conflictCandidate in effects)
    //     {
    //         if (conflictCandidate.ConflictsWith(effect))
    //         {
    //             return false;
    //         }
    //     }

    //     return true;
    // }

    // public bool EffectTypeAllowed(Type type)
    // {
    //     foreach (Effect conflictCandidate in effects)
    //     {
    //         if (conflictCandidate.ConflictsWithType(type))
    //         {
    //             return false;
    //         }
    //     }

    //     return true;
    // }

    //Removes an effect from the queue
    // public void RemoveEffect(Effect effect)
    // {
    //     effects.Remove(effect);
    //     foreach (Effect affected in effects)
    //     {
    //         affected.OnOtherEffectRemove(effect);
    //     }

    //     Destroy(effect.gameObject);
    // }

    // public void ExecuteArtilleryAttack(Tile[] targets)
    // {
    //     //Check if the number of targets is correct
    //     if (targets.Length > attackerCapabilities.maximumArtilleryCount || targets.Length == 0)
    //     {
    //         Debug.LogWarning("ARTILLERY ATTACK has a possibly invalid number of targets: " + targets.Length + "/" + attackerCapabilities.maximumArtilleryCount);
    //     }

    //     //Determine the final hits to ships and other tiles
    //     List<Tile> actualHits = new List<Tile>();
    //     Dictionary<Ship, List<int>> shipHits = new Dictionary<Ship, List<int>>();
    //     for (int targetIndex = 0; targetIndex < targets.Length; targetIndex++)
    //     {
    //         Tile target = targets[targetIndex];

    //         //If there is a concealed ship in this tile displace the shot
    //         if (target.containedShip != null)
    //         {
    //             if (target.containedShip.concealedBy)
    //             {
    //                 for (int x = (target.coordinates.x == 0 ? 0 : -1); x <= ((target.coordinates.x == defender.board.tiles.GetLength(0) - 1) ? 0 : 1); x++)
    //                 {
    //                     for (int y = (target.coordinates.y == 0 ? 0 : -1); y <= ((target.coordinates.y == defender.board.tiles.GetLength(1) - 1) ? 0 : 1); y++)
    //                     {
    //                         if (!(y == 0 && x == 0))
    //                         {
    //                             Tile candidate = defender.board.tiles[x + (int)target.coordinates.x, y + (int)target.coordinates.y];
    //                             if (!attacker.hitTiles.Contains(candidate) && !actualHits.Contains(candidate) && candidate.containedShip == null)
    //                             {
    //                                 target = candidate;
    //                                 break;
    //                             }
    //                         }
    //                     }
    //                 }
    //             }
    //         }

    //         if (!attacker.hitTiles.Contains(target))
    //         {
    //             actualHits.Add(target);
    //             if (target.containedShip)
    //             {
    //                 if (!shipHits.ContainsKey(target.containedShip))
    //                 {
    //                     shipHits.Add(target.containedShip, new List<int>());
    //                 }

    //                 for (int i = 0; i < target.containedShip.tiles.Length; i++)
    //                 {
    //                     if (target.containedShip.tiles[i] == target)
    //                     {
    //                         shipHits[target.containedShip].Add(i);
    //                         break;
    //                     }
    //                 }
    //             }
    //         }
    //     }

    //     //Apply the hits
    //     log[0].artilleryImpacts.AddRange(actualHits);
    //     log[0].damagedTiles.AddRange(actualHits);

    //     actualHits.ForEach(x => x.hit = true);

    //     foreach (KeyValuePair<Ship, List<int>> shipHitInfo in shipHits)
    //     {
    //         log[0].damagedShips.Add(shipHitInfo.Key);
    //         shipHitInfo.Key.Damage(shipHitInfo.Value.ToArray());
    //         if (shipHitInfo.Key.health <= 0)
    //         {
    //             log[0].destroyedShips.Add(shipHitInfo.Key);
    //         }
    //     }

    //     attacker.hitTiles.AddRange(actualHits);

    //     NextTurn();
    // }

    // public void ExecuteTorpedoAttack(int[] targets)
    // {
    //     //Determine the final hits to ships and other tiles
    //     List<Tile> impacts = new List<Tile>();
    //     List<Tile> hits = new List<Tile>();
    //     Dictionary<Ship, List<int>> shipDamage = new Dictionary<Ship, List<int>>();

    //     for (int targetIndex = 0; targetIndex < targets.Length; targetIndex++)
    //     {
    //         int target = targets[targetIndex];
    //         List<Tile> singularHits = new List<Tile>();
    //         Tile impact = null;

    //         for (int y = defender.board.tiles.GetLength(1) - 1; y >= 0; y--)
    //         {
    //             Tile candidate = defender.board.tiles[target, y];
    //             if (candidate.containedShip && candidate.containedShip.health > 0)
    //             {
    //                 impact = candidate;
    //                 break;
    //             }
    //             else
    //             {
    //                 singularHits.Add(candidate);
    //             }
    //         }

    //         if (impact)
    //         {
    //             if (impact.containedShip.health < impact.containedShip.maxHealth)
    //             {
    //                 singularHits.AddRange(impact.containedShip.tiles);
    //             }
    //             else
    //             {
    //                 singularHits.Add(impact);
    //             }

    //             impacts.Add(impact);
    //         }

    //         foreach (Tile hit in singularHits)
    //         {
    //             if (!attacker.hitTiles.Contains(hit) && !hits.Contains(hit))
    //             {
    //                 hits.Add(hit);
    //                 if (hit.containedShip)
    //                 {
    //                     if (!shipDamage.ContainsKey(hit.containedShip))
    //                     {
    //                         shipDamage.Add(hit.containedShip, new List<int>());
    //                     }

    //                     for (int i = 0; i < hit.containedShip.tiles.Length; i++)
    //                     {
    //                         if (hit.containedShip.tiles[i] == hit)
    //                         {
    //                             shipDamage[hit.containedShip].Add(i);
    //                             break;
    //                         }
    //                     }
    //                 }
    //             }
    //         }
    //     }

    //     //Apply the hits
    //     log[0].torpedoImpacts.AddRange(impacts);
    //     log[0].damagedTiles.AddRange(hits);

    //     hits.ForEach(x => x.hit = true);

    //     foreach (KeyValuePair<Ship, List<int>> shipHitInfo in shipDamage)
    //     {
    //         log[0].damagedShips.Add(shipHitInfo.Key);
    //         shipHitInfo.Key.Damage(shipHitInfo.Value.ToArray());
    //         if (shipHitInfo.Key.health <= 0)
    //         {
    //             log[0].destroyedShips.Add(shipHitInfo.Key);
    //         }
    //     }

    //     attacker.hitTiles.AddRange(hits);

    //     //Take away the consumed torpedoes from the destroyers
    //     int ammoCost = targets.Length;
    //     for (int i = 0; i < attacker.board.ships.Length; i++)
    //     {
    //         Ship ship = attacker.board.ships[i];
    //         if (ship.health > 0 && ship.type == ShipType.DESTROYER)
    //         {
    //             Destroyer destroyer = (Destroyer)ship;
    //             int initialTorpedoCount = destroyer.torpedoCount;
    //             destroyer.torpedoCount -= Mathf.Clamp(ammoCost, 0, destroyer.torpedoCount);

    //             ammoCost -= initialTorpedoCount - destroyer.torpedoCount;
    //             if (ammoCost == 0)
    //             {
    //                 break;
    //             }
    //         }
    //     }

    //     NextTurn();
    // }

    public static Battle CreateBattle(BattleData data)
    {
        Battle battle = new GameObject("Battle").AddComponent<Battle>();
        battle.Initialize(data);
        battle.AssignReferences(data);
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
        data.defender.aiEnabled = aiOpponent;

        data.tutorialStage = tutorialEnabled ? 1 : 0;
        data.saveSlot = saveSlot;

        return data;
    }
}
