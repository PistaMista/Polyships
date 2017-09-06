using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputEnabledUserInterface : BasicUserInterface
{
    public struct ScreenWorldCoordPair
    {
        public Vector3 world;
        public Vector2 screen;
    }

    protected static int inputPoints;
    protected static ScreenWorldCoordPair initialInputPosition;
    protected static ScreenWorldCoordPair currentInputPosition;
    protected static bool beginPress;
    protected static bool endPress;
    protected static bool tap;
    protected static bool dragging;
    protected static bool pressed;
    public float dragRegisterDistanceInScreenHeightPercentage;

    protected override void Update ()
    {
        base.Update();
        if (pressed)
        {
            if (Vector2.Distance( currentInputPosition.screen, initialInputPosition.screen ) / Screen.height * 100f > dragRegisterDistanceInScreenHeightPercentage)
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
