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
        [Serializable]
        public struct BattleData
        {
            public Player.PlayerData attacker;
            public Player.PlayerData defender;
            public int turnCount;
            public int saveSlot;
            public Effect.EffectData[] effects;

            public static implicit operator BattleData(Battle battle)
            {
                BattleData result = new BattleData();
                result.attacker = battle.attacker;
                result.defender = battle.defender;
                result.turnCount = battle.turnCount;
                result.saveSlot = battle.saveSlot;

                result.effects = new Effect.EffectData[battle.effects.Count];
                for (int i = 0; i < battle.effects.Count; i++)
                {
                    result.effects[i] = battle.effects[i];
                }
                return result;
            }
        }

        public static Battle main;
        public Player attacker;
        public Player defender;
        public List<Effect> effects = new List<Effect>(); //Effects change the battle parameters - at the start/end of each turn or when another effect is added/removed. Effects are applied in the order of this list.
        public int turnCount;
        public int saveSlot;
        public bool fighting
        {
            get
            {
                return turnCount >= 2;
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

            turnCount = data.turnCount;
            saveSlot = data.saveSlot;
        }

        public void AssignReferences(BattleData data)
        {
            attacker.AssignReferences(data.attacker);
            defender.AssignReferences(data.defender);

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
            if (fighting) Event.ConsiderEvents();

            if (turnCount == 2) OnBattleStart();

            Player lastAttacker = attacker;
            attacker = defender;
            defender = lastAttacker;

            OnTurnStart();
            turnCount++;

            SaveToDisk();
        }

        /// <summary>
        /// Executes every time a new turn starts.
        /// </summary>
        public override void OnTurnStart()
        {
            base.OnTurnStart();
            OnTurnResume();

            if (fighting) Event.ConsiderEvents();

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
                attackerAmmo.visibleTo = battle.attacker;

                defenderAmmo.targetedPlayer = battle.defender;
                defenderAmmo.visibleTo = battle.defender;

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

            data.saveSlot = saveSlot;
            data.effects = new Effect.EffectData[0];

            return data;
        }
    }
}