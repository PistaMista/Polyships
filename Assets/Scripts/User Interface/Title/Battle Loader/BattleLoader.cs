﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using Gameplay;
using BattleUIAgents.Base;
using BattleUIAgents.UI;

namespace TitleUI
{
    public class BattleLoader : TitleSlave
    {
        public Battle.BattleData[] saveSlotContents;
        public static bool allSlotsEmpty;

        protected override void SetState(UIState state)
        {
            base.SetState(state);
            SetInteractable((int)state >= 2);
        }

        public override void OnTitleSetState(UIState state)
        {
            if ((int)state >= 2)
            {
                saveSlotContents = new Battle.BattleData[3];
                allSlotsEmpty = true;
                for (int i = 0; i < saveSlotContents.Length; i++)
                {
                    try
                    {
                        FileStream stream = new FileStream(Path.Combine(Application.persistentDataPath, i.ToString()), FileMode.Open);
                        BinaryFormatter formatter = new BinaryFormatter();

                        Debug.Log(stream.Length);
                        object deserialized = formatter.Deserialize(stream);
                        stream.Close();
                        saveSlotContents[i] = (Battle.BattleData)deserialized;
                        allSlotsEmpty = false;
                    }
                    catch
                    {
                        saveSlotContents[i] = new Battle.BattleData();
                    }
                }
            }
        }

        public void SelectSlot(int slot)
        {
            State = UIState.DISABLED;

            if (CheckSlot(slot))
            {
                Battle.main = Battle.CreateBattle(saveSlotContents[slot]);
                BattleUIAgent.FindAgent(x => { return true; }, typeof(TurnNotifier)).gameObject.SetActive(true);
            }
            else
            {
                BattleTweaker.saveSlot = slot;
                Next();
            }
        }

        public bool CheckSlot(int slot)
        {
            return saveSlotContents[slot].effects != null;
        }

        public void ClearSlot(int slot)
        {
            saveSlotContents[slot] = new Battle.BattleData();

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(Path.Combine(Application.persistentDataPath, slot.ToString()), FileMode.Create);

            formatter.Serialize(stream, 0);

            stream.Close();
        }
    }
}
