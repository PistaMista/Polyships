using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class CombatantUIRelay : UIAgent
{
    public bool managingAttacker;
    public CombatantUI[] combatantUIPair;
    CombatantUI correspondingCombatantUI
    {
        get { return combatantUIPair[(managingAttacker ? Battle.main.attacker : Battle.main.defender).index]; }
    }

    protected override void ChangeState(UIState state)
    {
        correspondingCombatantUI.relay = this;
        correspondingCombatantUI.State = state;
        this.state = state;
    }

    protected override void Update()
    {

    }

    public T GetCurrentlyConnectedUIOfType<T>()
    {
        T result = default(T);
        try
        {
            result = (T)Convert.ChangeType(correspondingCombatantUI, typeof(T));
        }
        catch
        {

        }

        return result;
    }
}
