using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimaryTacticalTargetingBUI : TacticalTargetingBattleUserInterface
{
    protected override void PickupToken(ActionToken token)
    {
        base.PickupToken(token);
        attackViewUserInterface.activePrimaryTargeter = this;
    }

    protected override void DropHeldToken()
    {
        base.DropHeldToken();
        if (heldToken.value == null && placedTokens.Count == 0)
        {
            attackViewUserInterface.activePrimaryTargeter = null;
        }
        heldToken = null;
    }

    protected override void CheckTokensForPickup()
    {
        if (attackViewUserInterface.activePrimaryTargeter == null)
        {
            base.CheckTokensForPickup();
        }
    }
}
