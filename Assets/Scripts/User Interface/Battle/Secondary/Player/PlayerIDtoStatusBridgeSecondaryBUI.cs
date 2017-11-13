using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PlayerIDtoStatusBridgeSecondaryBUI : SecondaryBattleUserInterface
{
    public bool managingAttacker;
    public PlayerIDBoundSecondaryBUI[] playerIDBUIPair;
    PlayerIDBoundSecondaryBUI boundPlayerIDBUI
    {
        get { return playerIDBUIPair[(managingAttacker ? Battle.main.attacker : Battle.main.defender).index]; }
    }

    protected override void ChangeState(UIState state)
    {
        boundPlayerIDBUI.State = state;
    }

    public T GetCurrentlyConnectedBUIOfType<T>()
    {
        T result = default(T);
        try
        {
            result = (T)Convert.ChangeType(boundPlayerIDBUI, typeof(T));
        }
        catch
        {

        }

        return result;
    }
}
