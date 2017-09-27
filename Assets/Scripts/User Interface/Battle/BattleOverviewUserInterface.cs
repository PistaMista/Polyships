using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleOverviewUserInterface : BattleUserInterface
{

    public static BattleOverviewUserInterface it;

    void Awake()
    {
        it = this;
    }

    public static void SetState(UIState state)
    {
        it.State = state;
    }
}
