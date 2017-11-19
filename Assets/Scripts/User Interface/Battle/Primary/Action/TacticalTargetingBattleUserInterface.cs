using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticalTargetingBattleUserInterface : PrimaryBattleUserInterface
{
    public bool isPrimary;
    public AttackViewUserInterface attackViewUserInterface;
    protected ActionToken[] tokens;
    protected ActionToken selectedToken;
    public void ResetTokens()
    {
        ResetWorldSpaceParent();
        tokens = null;
        AddTokens();
    }

    protected virtual void AddTokens()
    {

    }

    protected override void Update()
    {
        base.Update();
        if (attackViewUserInterface.State == UIState.DISABLING)
        {
            HideAllTokens();
        }
        else
        {

        }
    }

    protected virtual void HideAllTokens()
    {
        for (int i = 0; i < tokens.Length; i++)
        {
            ActionToken token = tokens[i];
            token.targetPosition.y = -10f;
        }
    }
}
