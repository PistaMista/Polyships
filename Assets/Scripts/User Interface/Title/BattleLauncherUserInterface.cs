using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleLauncherUserInterface : SlidingUserInterface
{
    public SlidingUserInterface_Master masterInterface;
    protected override void SetState(UIState state)
    {
        base.SetState(state);
        switch (state)
        {
            case UIState.ENABLING:
                SlidingUserInterface_Master.lockedDirections = new bool[] { true, true };
                Battle.BattleData toLoad = GameModeSelectorUserInterface.selectedMode == 2 ? GameLoaderUserInterface.saveSlotData : GameLoaderUserInterface.newBattleData;
                Battle battle = new GameObject("Battle").AddComponent<Battle>();
                battle.Initialize(toLoad);
                battle.AssignReferences(toLoad);
                break;
            case UIState.ENABLED:
                masterInterface.State = UIState.DISABLED;
                break;
        }
    }
}
