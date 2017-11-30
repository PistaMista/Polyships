using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UIState
{
    DISABLED,
    DISABLING,
    ENABLING,
    ENABLED
}

public class BasicUserInterface : UIAgent
{
    public RectTransform rect;
    public bool enableOnStart;
    public static Vector2 referenceResolution;

    protected virtual void Start()
    {
        if (enableOnStart)
        {
            State = UIState.ENABLED;
        }
    }

    protected Vector4 GetIntersection(Vector2 lowerLeftCorner, Vector2 upperRightCorner, Vector2 position)
    {
        Vector2 size = upperRightCorner - lowerLeftCorner;
        Vector4 result = new Vector4((position.x - lowerLeftCorner.x) / size.x, (position.y - lowerLeftCorner.y) / size.y, position.x - lowerLeftCorner.x, position.y - lowerLeftCorner.y);
        return result;
    }

    protected bool CheckIntersection(Vector2 intersection)
    {
        return intersection.x >= 0 && intersection.x <= 1 && intersection.y >= 0 && intersection.y <= 1;
    }
}
