using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnNotifierUI : InputEnabledUI
{
    public string humanTurnText;
    public string computerTurnText;
    public float computerTurnNotificationTime;
    public Text textbox;
    protected override void SetState(UIState state)
    {
        base.SetState(state);
        switch (state)
        {
            case UIState.DISABLING:
                SetInteractable(false);
                break;
            case UIState.ENABLING:
                SetInteractable(true);
                CameraControl.GoToWaypoint(Battle.main.attacker.flagCameraPoint, MiscellaneousVariables.it.playerCameraTransitionTime * 0.2f);
                if (Battle.main.attacker.aiType == AIType.NONE)
                {
                    textbox.text = humanTurnText;
                }
                else
                {
                    Invoke("DoAITurn", computerTurnNotificationTime);
                    textbox.text = computerTurnText;
                }
                break;
        }
    }

    protected override void ProcessInput()
    {
        base.ProcessInput();
        if (tap && Battle.main.attacker.aiType == AIType.NONE)
        {
            State = UIState.DISABLING;
            if (Battle.main.attacker.board.ships == null)
            {
                BattleUIMaster.EnablePrimaryBUI(BattleUIType.FLEET_PLACEMENT);
            }
            else
            {
                BattleUIMaster.EnablePrimaryBUI(BattleUIType.BATTLE_OVERVIEW);
            }
        }
    }

    void DoAITurn()
    {
        Battle.main.attacker.aiModule.DoTurn();
        State = UIState.DISABLING;
        BattleUIMaster.EnablePrimaryBUI(BattleUIType.CINEMATIC_VIEW);
    }
}
