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
        State = UIState.DISABLED;
    }

    public override void OnTitleSetState(UIState state)
    {
        base.OnTitleSetState(state);
        if ((int)state >= 2)
        {
            flags = new float[2][,,];
            for (int i = 0; i < 2; i++)
            {
                flags[i] = new float[MiscellaneousVariables.it.flagResolution.x, MiscellaneousVariables.it.flagResolution.y, 3];
                for (int x = 0; x < flags[i].GetLength(0); x++)
                {
                    for (int y = 0; y < flags[i].GetLength(1); y++)
                    {
                        flags[i][x, y, 0] = 1;
                        flags[i][x, y, 1] = 1;
                        flags[i][x, y, 2] = 1;
                    }
                }
            }
        }
    }
}
