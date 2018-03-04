using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class BattleLoaderUI : InputEnabledUI
{
    public Text[] saveSlotLabels;
    Battle.BattleData[] saveSlotContents;
    public UIAgent battleCreatorUI;


    protected override void SetState(UIState state)
    {
        base.SetState(state);
        // switch (state)
        // {
        //     case UIState.ENABLING:
        //         if (selectedSlotID < 0)
        //         {
        //             SlidingUserInterface_Master.lockedDirections[1] = true;
        //         }
        //         break;
        //     case UIState.DISABLING:
        //         SlidingUserInterface_Master.lockedDirections = new bool[2];
        //         break;
        // }

    }

    // public override void OnMasterEnable()
    // {
    //     base.OnMasterEnable();
    //     selectedSlotID = -1;
    //     saveSlotContents = new Battle.BattleData[toggleGroup.buttons.Length];

    //     neverPlayed = true;
    //     for (int i = 0; i < saveSlotContents.Length; i++)
    //     {
    //         try
    //         {
    //             FileStream stream = new FileStream(Path.Combine(Application.persistentDataPath, i.ToString()), FileMode.Open);
    //             BinaryFormatter formatter = new BinaryFormatter();

    //             saveSlotContents[i] = (Battle.BattleData)formatter.Deserialize(stream);
    //             saveSlotLabels[i].text = "SAVED BATTLE - " + saveSlotContents[i].log.Length + " TURNS PLAYED";
    //             neverPlayed = false;
    //         }
    //         catch
    //         {
    //             saveSlotContents[i] = new Battle.BattleData();
    //             saveSlotLabels[i].text = "EMPTY SLOT";
    //         }
    //     }
    // }

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
