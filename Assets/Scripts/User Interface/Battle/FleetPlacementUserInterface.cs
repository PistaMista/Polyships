using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleetPlacementUserInterface : BoardViewUserInterface
{
    public static FleetPlacementUserInterface it;

    void Awake()
    {
        it = this;
    }

    public static void SetState(UIState state)
    {
        it.State = state;
    }
}
