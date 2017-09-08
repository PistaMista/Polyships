using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputEnabledUserInterface : BasicUserInterface
{
    public struct ScreenWorldCoordinatePair
    {
        public Vector3 world;
        public Vector2 screen;
    }
    protected static int inputPoints;
    protected static ScreenWorldCoordinatePair initialInputPosition;
    protected static ScreenWorldCoordinatePair currentInputPosition;
    protected static bool beginPress;
    protected static bool endPress;
    protected static bool tap;
    protected static bool dragging;
    protected static bool pressed;
    [Range(0.0f, 1.0f)]
    public float dragRegisterDistanceInScreenHeightPercentage;
    public bool processInputWhenEnabling;
    protected override void Update()
    {
        base.Update();
        if (state == UIState.ENABLED || state == UIState.ENABLING && processInputWhenEnabling)
        {
            ProcessInput();
        }
    }

    protected virtual void ProcessInput() 
    {
        if (pressed)
        {
            if (Vector2.Distance(currentInputPosition.screen, initialInputPosition.screen) / Screen.height > dragRegisterDistanceInScreenHeightPercentage)
            {
                dragging = true;
            }
        }
        else
        {
            dragging = false;
        }
    }
}
