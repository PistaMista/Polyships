using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackerInformationUserInterface : BoardViewUserInterface
{
    public static AttackerInformationUserInterface it;

    void Awake()
    {
        it = this;
    }

    public static void SetState(UIState state)
    {
        it.State = state;
    }
}
