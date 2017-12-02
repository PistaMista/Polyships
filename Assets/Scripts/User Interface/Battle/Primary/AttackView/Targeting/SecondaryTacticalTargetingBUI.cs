using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondaryTacticalTargetingBUI : TacticalTargetingBattleUserInterface
{
    protected override void DropHeldToken()
    {
        base.DropHeldToken();
        heldToken = null;
    }
}
