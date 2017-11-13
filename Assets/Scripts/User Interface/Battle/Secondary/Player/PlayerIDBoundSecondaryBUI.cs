using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIDBoundSecondaryBUI : SecondaryBattleUserInterface
{
    public int managedPlayerID;
    protected Player managedPlayer;
    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.ENABLING:
                if (managedPlayer == null)
                {
                    managedPlayer = Battle.main.attacker.index == managedPlayerID ? Battle.main.attacker : Battle.main.defender;
                }
                worldSpaceParent.position = managedPlayer.transform.position;
                break;
        }
    }
}
