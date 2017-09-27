using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackViewUserInterface : BoardViewUserInterface
{

    public static AttackViewUserInterface it;
    bool attackConfirmed;
    void Awake()
    {
        it = this;
    }

    public static void SetState(UIState state)
    {
        it.State = state;
    }

    public void ConfirmAttack()
    {

    }
}
