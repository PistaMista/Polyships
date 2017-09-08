
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingUserInterface_Master : InputEnabledUserInterface
{
    SlidingUserInterface[] interfaces;
    public int selectedPosition;
    public int lastPosition;
    int defaultPosition;
    float transitionVelocity;
    public static float transitionDistance;
    RectTransform rectTransform;
    public static bool locked;
    bool afterDragLock;

    void Start()
    {
        interfaces = gameObject.GetComponentsInChildren<SlidingUserInterface>(true);
        rectTransform = gameObject.GetComponent<RectTransform>();
        defaultPosition = selectedPosition;
    }

    protected override void Update()
    {
        rectTransform.anchoredPosition = Vector2.right * Mathf.SmoothDamp(rectTransform.anchoredPosition.x, (defaultPosition - selectedPosition) * Screen.width, ref transitionVelocity, 0.2f, Mathf.Infinity);

        base.Update();

        if (!dragging)
        {
            afterDragLock = false;
        }

        transitionDistance = Mathf.Abs(rectTransform.anchoredPosition.x - (defaultPosition - selectedPosition) * Screen.width);
    }

    protected override void ProcessInput()
    {
        base.ProcessInput();
        if (!locked && !afterDragLock)
        {
            int moveDirection = dragging ? (int)Mathf.Clamp(Mathf.Floor((initialInputPosition.screen.x - currentInputPosition.screen.x) / (Screen.width / 3.0f)), -1.0f, 1.0f) : 0;
            if (moveDirection != 0)
            {
                RecalculateChildrenPositions();
                int candidatePosition = selectedPosition + moveDirection;
                if (candidatePosition >= 0 && candidatePosition <= lastPosition)
                {
                    selectedPosition = candidatePosition;
                    RecalculateChildrenPositions();
                }

                afterDragLock = true;
            }
        }
    }

    void RecalculateChildrenPositions()
    {
        int widthOffset = 0;

        for (int i = 0; i < interfaces.Length; i++)
        {
            int absolutePosition = i + widthOffset - defaultPosition;
            int relativePosition = absolutePosition - selectedPosition;

            if (relativePosition <= 0 && relativePosition > -interfaces[i].width)
            {
                interfaces[i].AEnable();
            }
            else
            {
                interfaces[i].ADisable();
            }

            interfaces[i].rect.anchoredPosition = Vector2.right * absolutePosition * Screen.width;
            widthOffset += interfaces[i].width - 1;
        }

        lastPosition = widthOffset + interfaces.Length - 1;
    }
}
