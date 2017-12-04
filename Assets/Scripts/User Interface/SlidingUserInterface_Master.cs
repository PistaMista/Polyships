
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlidingUserInterface_Master : InputEnabledUI
{
    SlidingUserInterface[] interfaces;
    public int selectedPosition;
    public int lastPosition;
    public int defaultPosition;
    public float screenTransitionTime;
    float transitionVelocity;
    public static float transitionDistance;
    public static bool[] lockedDirections;
    bool afterDragLock;
    public Vector2 defaultReferenceResolution;


    protected override void Start()
    {
        referenceResolution = defaultReferenceResolution;
        interfaces = gameObject.GetComponentsInChildren<SlidingUserInterface>(true);
        RecalculateChildrenPositions();
        defaultPosition = selectedPosition;
        lockedDirections = new bool[2];
        base.Start();
    }

    protected override void Update()
    {
        rect.anchoredPosition = Vector2.right * Mathf.SmoothDamp(rect.anchoredPosition.x, (defaultPosition - selectedPosition) * referenceResolution.x, ref transitionVelocity, screenTransitionTime, Mathf.Infinity);

        base.Update();

        if (!dragging)
        {
            afterDragLock = false;
        }

        transitionDistance = Mathf.Abs(rect.anchoredPosition.x - (defaultPosition - selectedPosition) * referenceResolution.x);
    }

    protected override void ProcessInput()
    {
        base.ProcessInput();
        if (!afterDragLock)
        {
            int moveDirection = dragging && Mathf.Abs(initialInputPosition.screen.x - currentInputPosition.screen.x) > Screen.width / 10.0f ? (int)Mathf.Sign(initialInputPosition.screen.x - currentInputPosition.screen.x) : 0;
            if (moveDirection != 0 && !lockedDirections[(moveDirection + 1) / 2])
            {
                RecalculateChildrenPositions();
                int candidatePosition = selectedPosition + moveDirection;
                if (candidatePosition >= 0 && candidatePosition <= lastPosition)
                {
                    selectedPosition = candidatePosition;
                    // successRating += moveDirection > 0 ? 1 : -backwardSwipeSuccessFalloff;
                    // cluelessTime = 0;
                    // if (moveDirection > 0)
                    // {
                    //     backwardSwipeSuccessFalloff *= 0.6f;
                    // }
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
                if (interfaces[i].State != UIState.ENABLED)
                {
                    interfaces[i].State = UIState.ENABLING;
                }
            }
            else
            {
                if (interfaces[i].State != UIState.DISABLED)
                {
                    interfaces[i].State = UIState.DISABLING;
                }
            }

            interfaces[i].rect.anchoredPosition = Vector2.right * absolutePosition * referenceResolution.x;
            widthOffset += interfaces[i].width - 1;
        }

        lastPosition = widthOffset + interfaces.Length - 1;
    }

    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        if ((int)state <= 2)
        {
            foreach (SlidingUserInterface i in interfaces)
            {
                i.OnMasterDisable();
            }
        }
        else
        {
            selectedPosition = defaultPosition;
            lockedDirections = new bool[] { false, false };
            RecalculateChildrenPositions();
            foreach (SlidingUserInterface i in interfaces)
            {
                i.OnMasterEnable();
            }
        }
    }
}
