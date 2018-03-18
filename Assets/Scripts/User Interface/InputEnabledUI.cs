using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputEnabledUI : BasicUI
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
    public float defaultConversionDistance;
    public bool interactable;
    protected override void Update()
    {
        base.Update();
        if (interactable)
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

        float conversionDistance = CalculateConversionDistance();
        initialInputPosition.world = ConvertToWorldInputPosition(initialInputPosition.screen, conversionDistance);
        currentInputPosition.world = ConvertToWorldInputPosition(currentInputPosition.screen, conversionDistance);
        lastFrameInputPosition.world = ConvertToWorldInputPosition(lastFrameInputPosition.screen, conversionDistance);
        dragVelocity.world = ConvertToWorldInputPosition(dragVelocity.screen, conversionDistance);
    }

    protected Vector3 ConvertToWorldInputPosition(Vector2 screenPosition, float conversionDistance)
    {
        return Camera.main.ScreenToWorldPoint((Vector3)screenPosition + Vector3.forward * conversionDistance);
    }

    protected virtual float CalculateConversionDistance()
    {
        return defaultConversionDistance;
    }

    public void SetInteractable(bool enabled)
    {
        foreach (Selectable selectable in GetComponentsInChildren<Selectable>())
        {
            selectable.interactable = enabled;
        }

        foreach (InputEnabledUI i in GetComponentsInChildren<InputEnabledUI>())
        {
            i.interactable = enabled;
        }
    }
}
