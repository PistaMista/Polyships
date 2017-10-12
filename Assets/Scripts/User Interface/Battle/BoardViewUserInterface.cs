using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardViewUserInterface : BattleUserInterface
{
    public FlagRendererSecondaryBUI flagRenderer;

    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.ENABLING:
                if (flagRenderer.gameObject.activeInHierarchy)
                {
                    flagRenderer.onCameraOcclusion += DeployWorldElements;
                }
                else
                {
                    DeployWorldElements();
                }
                break;
            case UIState.DISABLING:
                flagRenderer.onCameraOcclusion += HideWorldElements;
                break;
        }
    }

    protected virtual void DeployWorldElements()
    {
        SetInteractable(true);
    }

    protected virtual void HideWorldElements()
    {

    }
}
