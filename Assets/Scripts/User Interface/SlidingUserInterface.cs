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
        if (State == UIState.ENABLING && SlidingUserInterface_Master.transitionDistance < Screen.width * 0.05f)
        {
            State = UIState.ENABLED;
        }
        else if (State == UIState.DISABLING && SlidingUserInterface_Master.transitionDistance < Screen.width * 0.01f)
        {
            State = UIState.DISABLED;
        }
    }

    public virtual void OnMasterEnable()
    {

    }

    public virtual void OnMasterDisable()
    {

    }
}
