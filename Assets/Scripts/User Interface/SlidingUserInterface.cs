using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingUserInterface : InputEnabledUserInterface
{
    public int position;
    public int width;

    protected override void Update()
    {
        base.Update();
        if ((State == UIState.DISABLING || State == UIState.ENABLING) && SlidingUserInterface_Master.transitionDistance < Screen.width * 0.05f)
        {
            State = State == UIState.ENABLING ? UIState.ENABLED : UIState.DISABLED;
        }
    }
}
