using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputEnabledUIMaster : InputEnabledUI
{
    protected override void Update()
    {
        base.Update();
    }
    bool lastPressState;
    protected override void ProcessInput()
    {
        pressed = Input.GetMouseButton(0);
        currentInputPosition.screen = Input.mousePosition;


        beginPress = !lastPressState && pressed;
        endPress = lastPressState && !pressed;
        tap = endPress && !dragging;

        if (beginPress)
        {
            initialInputPosition.screen = currentInputPosition.screen;
        }

        if (endPress)
        {
            dragging = false;
        }

        inputPoints = Input.touchCount;

        if (dragging)
        {
            dragVelocity = (currentInputPosition - lastFrameInputPosition) / Time.deltaTime;
        }

        lastFrameInputPosition = currentInputPosition;
        lastPressState = pressed;
    }

}
