using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class BattleLoaderUI : TitleSlaveUI
{
    Battle.BattleData[] saveSlotContents;
    public UIAgent battleCreatorUI;

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
            for (int i = 0; i < saveSlotContents.Length; i++)
            {
                try
                {
                    FileStream stream = new FileStream(Path.Combine(Application.persistentDataPath, i.ToString()), FileMode.Open);
                    BinaryFormatter formatter = new BinaryFormatter();

                    saveSlotContents[i] = (Battle.BattleData)formatter.Deserialize(stream);
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
        Battle.BattleData saveSlotData = saveSlotContents[slot];
        State = UIState.DISABLING;

        if (saveSlotData.log != null)
        {
            Battle.main = Battle.CreateBattle(saveSlotData);
        }
        else
        {
            battleCreatorUI.State = UIState.ENABLING;
        }
    }
}
