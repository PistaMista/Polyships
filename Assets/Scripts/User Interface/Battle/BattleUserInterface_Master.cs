using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUserInterface_Master : InputEnabledUserInterface
{
    static BattleUserInterface_Master it;
    public BattleUserInterface[] interfaces;

    void Awake()
    {
        it = this;
    }

    public static void Disable()
    {
        for (int i = 0; i < it.interfaces.Length; i++)
        {
            BattleUserInterface x = it.interfaces[i];
            if (x.State != UIState.DISABLED && x.State != UIState.DISABLING)
            {
                x.State = UIState.DISABLING;
            }
        }
    }

    public static void EnableUI(BattleUIType type)
    {
        Battle.main.lastOpenUserInterface = type;
        it.interfaces[(int)type].State = UIState.ENABLING;
    }
}
