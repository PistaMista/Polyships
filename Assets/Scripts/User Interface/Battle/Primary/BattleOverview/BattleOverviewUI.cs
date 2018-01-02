using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleOverviewUI : InputEnabledUI
{
    public Waypoint cameraWaypoint;
    protected override void SetState(UIState state)
    {
        base.SetState(state);
        switch (state)
        {
            case UIState.ENABLING:
                cameraWaypoint.transform.position = Vector3.up * (CameraControl.CalculateCameraWaypointHeight(new Vector2(2 * MiscellaneousVariables.it.boardDistanceFromCenter + 20 * MiscellaneousVariables.it.flagVoxelScale, 0)) + MiscellaneousVariables.it.flagRenderHeight);
                cameraWaypoint.transform.LookAt(Vector3.zero);
                CameraControl.GoToWaypoint(cameraWaypoint, 0.55f);
                break;
        }
        SetInteractable(state == UIState.ENABLED);
    }

    protected override void ProcessInput()
    {
        base.ProcessInput();
        if (tap)
        {
            Vector3 tapPosition = ConvertToWorldInputPosition(currentInputPosition.screen);
            bool selectedAttacker = Mathf.Sign(tapPosition.x) == Mathf.Sign(Battle.main.attacker.transform.position.x);
            State = UIState.DISABLING;
            if (selectedAttacker)
            {
                BattleUIMaster.EnablePrimaryBUI(BattleUIType.ATTACKER_INFO);
            }
            else
            {
                BattleUIMaster.EnablePrimaryBUI(BattleUIType.ATTACK_VIEW);
            }
        }
        else if (endPress && inputPoints == 2)
        {
            Battle.main.QuitBattle();
        }
    }
}
