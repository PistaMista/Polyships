using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicViewUI : InputEnabledUI
{
    public Battle.TurnInfo processedTurn;
    public float cinematicTime = 0;
    protected override void SetState(UIState state)
    {
        base.SetState(state);
        switch (state)
        {
            case UIState.ENABLING:
                processedTurn = Battle.main.log[1];
                //TEST
                cinematicTime = 1.0f;
                break;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (State == UIState.ENABLING)
        {
            if (cinematicTime < 0)
            {
                Finish();
            }
            cinematicTime -= Time.deltaTime;
        }
    }

    void Finish()
    {
        State = UIState.DISABLING;
        BattleUIMaster.EnablePrimaryBUI(BattleUIType.TURN_NOTIFIER);
    }
}
