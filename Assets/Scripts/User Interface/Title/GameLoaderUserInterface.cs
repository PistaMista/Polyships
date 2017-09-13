using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class GameLoaderUserInterface : SlidingUserInterface
{
    public int selectedSlot;
    public Button[] saveSlotButtons;
    public Text[] saveSlotLabels;
    Battle.BattleData[] saveSlotContents;

    protected override void Start()
    {
        base.Start();
    }

    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        if (state == UIState.ENABLING)
        {
            selectedSlot = -1;
            saveSlotContents = new Battle.BattleData[saveSlotButtons.Length];

            for (int i = 0; i < saveSlotContents.Length; i++)
            {
                try
                {
                    FileStream stream = new FileStream(Path.Combine(Application.persistentDataPath, i.ToString()), FileMode.Open);
                    BinaryFormatter formatter = new BinaryFormatter();

                    saveSlotContents[i] = (Battle.BattleData)formatter.Deserialize(stream);
                    saveSlotLabels[i].text = "SAVED BATTLE - " + 225 + " TURNS";
                }
                catch
                {
                    saveSlotContents[i] = new Battle.BattleData();
                    saveSlotLabels[i].text = "EMPTY SLOT";
                }
            }
        }
    }

    public void SelectSlot(int slot)
    {

    }
}
