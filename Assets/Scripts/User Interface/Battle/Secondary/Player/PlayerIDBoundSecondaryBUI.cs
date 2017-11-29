﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIDBoundSecondaryBUI : BattleUserInterface
{
    public int managedPlayerID;
    protected Player managedPlayer;
    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        if (managedPlayer == null)
        {
            managedPlayer = Battle.main.attacker.index == managedPlayerID ? Battle.main.attacker : Battle.main.defender;
        }

        switch (state)
        {
            case UIState.ENABLING:
                UIAgentParent.position = managedPlayer.transform.position;
                break;
        }
    }
}
