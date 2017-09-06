using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingUserInterface : InputEnabledUserInterface
{
    public int position;
    public int width;
    public static int selectedPosition;
    public static int reservedPositions;
    public static SlidingUserInterface rootInterface;
    public static bool locked;
    public static float transitionVelocity;
    public static float transitionDistancePercentage;

    public RectTransform rect;

    void Start ()
    {
        rect = gameObject.GetComponent<RectTransform>();
    }

    protected override void Update ()
    {
        base.Update();
        if (rootInterface == this)
        {
            if (!locked)
            {
                ProcessInput();
            }
            float targetPos = ( position - selectedPosition ) * rect.rect.width;
            rect.anchoredPosition = Vector2.right * Mathf.SmoothDamp( rect.anchoredPosition.x, targetPos, ref transitionVelocity, 0.2f, Mathf.Infinity );
            transitionDistancePercentage = Mathf.Abs( rect.anchoredPosition.x - targetPos ) / Screen.width;
        }
        else
        {
            rect.anchoredPosition = Vector2.right * ( rootInterface.rect.anchoredPosition.x + ( position - rootInterface.position ) * rect.rect.width );
        }

        state = ( selectedPosition < position + width && selectedPosition >= position ) ? ( transitionDistancePercentage < 0.15f ? UIState.ENABLED : UIState.ENABLING ) : ( ( transitionDistancePercentage < 0.15f || state == UIState.DISABLED ) ? UIState.DISABLED : UIState.DISABLING );
    }

    void ProcessInput ()
    {

    }
}
