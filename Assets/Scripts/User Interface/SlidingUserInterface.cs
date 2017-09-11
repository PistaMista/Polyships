using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingUserInterface : InputEnabledUserInterface
{
    public int position;
    public int width;

    public override void AEnable()
    {
        base.AEnable();
    }

    public override void ADisable()
    {
        base.ADisable();
    }

    protected override void Update()
    {
        base.Update();
        if ((state == UIState.DISABLING || state == UIState.ENABLING) && SlidingUserInterface_Master.transitionDistance < Screen.width * 0.05f)
        {
            state = state == UIState.ENABLING ? UIState.ENABLED : UIState.DISABLED;
            if (state == UIState.DISABLED)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
