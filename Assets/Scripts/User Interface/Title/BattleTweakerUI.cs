using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleTweakerUI : TitleSlaveUI
{
    protected override void SetState(UIState state)
    {
        base.SetState(state);
        SetInteractable((int)state >= 2);
    }
    public static bool aiOpponent;
    public static bool tutorial;
    public static int boardSize;
    public static int saveSlot;
    public void LaunchBattle()
    {
        Battle.main = Battle.CreateBattle(Battle.GetBlankBattleData(MiscellaneousVariables.it.boardSizes[boardSize], aiOpponent, tutorial, saveSlot));
    }
}
