using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSecondaryBUI : SecondaryBattleUserInterface
{
    protected Player managedPlayer;
    public bool managingAttacker;
    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.ENABLING:
                managedPlayer = managingAttacker ? Battle.main.attacker : Battle.main.attacked;
                worldSpaceParent.position = managedPlayer.transform.position;
                break;
        }
    }
}
