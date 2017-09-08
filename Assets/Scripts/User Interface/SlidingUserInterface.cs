using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingUserInterface : InputEnabledUserInterface
{
    public int position;
    public int width;
    public RectTransform rect;

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
        if (state == UIState.DISABLING && SlidingUserInterface_Master.transitionDistance < Screen.width * 0.05f)
        {
            gameObject.SetActive(false);
            state = UIState.DISABLED;
        }
    }
}
