using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatantUI : BoardViewUI
{
    public CombatantUIRelay relay;
    protected override void SetState(UIState state)
    {
        if (managedBoard == null)
        {
            int targetID = this == relay.combatantUIPair[0] ? 0 : 1;
            managedBoard = (targetID == Battle.main.attacker.index ? Battle.main.attacker : Battle.main.defender).board;
        }
        base.SetState(state);
        relay.state = state;
    }
}
