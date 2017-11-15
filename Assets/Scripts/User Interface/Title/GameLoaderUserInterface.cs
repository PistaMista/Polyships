using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;

public class GameLoaderUserInterface : SlidingUserInterface
{
    public int selectedSlotID;
    public static Battle.BattleData saveSlotData;
    public static Battle.BattleData newBattleData;
    public ExclusiveTogglingButtonGroup toggleGroup;
    public Text[] saveSlotLabels;
    Battle.BattleData[] saveSlotContents;
    public static bool neverPlayed;

    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.ENABLING:
                if (selectedSlotID < 0)
                {
                    SlidingUserInterface_Master.lockedDirections[1] = true;
                }
                break;
            case UIState.DISABLING:
                SlidingUserInterface_Master.lockedDirections = new bool[2];
                break;
        }

    }

    public override void OnMasterEnable()
    {
        base.OnMasterEnable();
        selectedSlotID = -1;
        saveSlotContents = new Battle.BattleData[toggleGroup.buttons.Length];

        neverPlayed = true;
        for (int i = 0; i < saveSlotContents.Length; i++)
        {
            try
            {
                FileStream stream = new FileStream(Path.Combine(Application.persistentDataPath, i.ToString()), FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();

                saveSlotContents[i] = (Battle.BattleData)formatter.Deserialize(stream);
                saveSlotLabels[i].text = "SAVED BATTLE - " + saveSlotContents[i].log.Length + " TURNS PLAYED";
                neverPlayed = false;
            }
            catch
            {
                saveSlotContents[i] = new Battle.BattleData();
                saveSlotLabels[i].text = "EMPTY SLOT";
            }
        }
    }

    public override void OnMasterDisable()
    {
        base.OnMasterDisable();
        toggleGroup.ResetColors();
    }

    public void SelectSlot(int slot)
    {
        if (slot != selectedSlotID)
        {
            selectedSlotID = slot;
            saveSlotData = saveSlotContents[slot];
            SlidingUserInterface_Master.lockedDirections = new bool[2];

            GameModeSelectorUserInterface.selectedMode = -1;

            newBattleData = new Battle.BattleData();
            newBattleData.saveSlot = slot;
            newBattleData.attacker.index = 0;
            newBattleData.defender.index = 1;
            newBattleData.stage = BattleStage.SHIP_PLACEMENT;
        }
    }
}
