using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimaryTacticalTargetingBUI : TacticalTargetingBattleUserInterface
{
    protected override void DropHeldToken()
    {
        base.DropHeldToken();
        if (heldToken.value == null && placedTokens.Count == 0)
        {
            attackViewUserInterface.activePrimaryTargeter = null;
        }
        else
        {
            attackViewUserInterface.activePrimaryTargeter = this;
        }
        heldToken = null;
    }

    protected override bool IsSelectable()
    {
        return base.IsSelectable() && (attackViewUserInterface.activePrimaryTargeter == null || attackViewUserInterface.activePrimaryTargeter == this);
    }

    public virtual void ConfirmTargeting()
    {
        attackViewUserInterface.activePrimaryTargeter = null;
        attackViewUserInterface.State = UIState.DISABLING;
        SwitchToInterface(BattleUIType.CINEMATIC_VIEW);
    }
}
