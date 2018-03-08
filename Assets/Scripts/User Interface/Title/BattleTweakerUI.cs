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
    public static float[][,,] flags;
    public void LaunchBattle()
    {
        Battle.main = Battle.CreateBattle(Battle.GetBlankBattleData(MiscellaneousVariables.it.boardSizes[boardSize], aiOpponent, tutorial, saveSlot, flags));
    }

    public override void OnTitleSetState(UIState state)
    {
        base.OnTitleSetState(state);
        flags = new float[2][,,];
        if ((int)state >= 2)
        {
            for (int i = 0; i < 2; i++)
            {
                flags[i] = new float[MiscellaneousVariables.it.flagResolution.x, MiscellaneousVariables.it.flagResolution.y, 3];
            }
        }
    }
}
