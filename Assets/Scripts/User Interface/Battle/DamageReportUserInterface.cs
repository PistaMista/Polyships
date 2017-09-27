using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReportUserInterface : BoardViewUserInterface
{

    public static DamageReportUserInterface it;

    void Awake()
    {
        it = this;
    }

    public static void SetState(UIState state)
    {
        it.State = state;
    }
}
