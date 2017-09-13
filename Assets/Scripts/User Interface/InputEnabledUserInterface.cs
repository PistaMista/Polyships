using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputEnabledUserInterface : BasicUserInterface
{
    public struct ScreenWorldCoordinatePair
    {
        public Vector3 world;
        public Vector2 screen;

        public ScreenWorldCoordinatePair(Vector3 world, Vector2 screen)
        {
            this.world = world;
            this.screen = screen;
        }

        public static ScreenWorldCoordinatePair operator -(ScreenWorldCoordinatePair a, ScreenWorldCoordinatePair b)
        {
            return new ScreenWorldCoordinatePair(a.world - b.world, a.screen - b.screen);
        }

        public static ScreenWorldCoordinatePair operator /(ScreenWorldCoordinatePair a, float b)
        {
            return new ScreenWorldCoordinatePair(a.world / b, a.screen / b);
        }

        public static ScreenWorldCoordinatePair operator *(ScreenWorldCoordinatePair a, float b)
        {
            return new ScreenWorldCoordinatePair(a.world * b, a.screen * b);
        }
    }
    protected static int inputPoints;
    protected static ScreenWorldCoordinatePair initialInputPosition;
    protected static ScreenWorldCoordinatePair currentInputPosition;
    protected static ScreenWorldCoordinatePair lastFrameInputPosition;
    protected static ScreenWorldCoordinatePair dragVelocity;
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
        if (State == UIState.ENABLED || State == UIState.ENABLING && processInputWhenEnabling)
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

    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        foreach (Selectable selectable in GetComponentsInChildren<Selectable>())
        {
            selectable.interactable = state == UIState.ENABLED || (processInputWhenEnabling && state == UIState.ENABLING);
        }
    }
}
