
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlidingUserInterface_Master : InputEnabledUserInterface
{
    SlidingUserInterface[] interfaces;
    public int selectedPosition;
    public int lastPosition;
    public int defaultPosition;
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
        rect.anchoredPosition = Vector2.right * Mathf.SmoothDamp(rect.anchoredPosition.x, (defaultPosition - selectedPosition) * referenceResolution.x, ref transitionVelocity, 0.65f, Mathf.Infinity);

        base.Update();

        if (!dragging)
        {
            afterDragLock = false;
        }

        transitionDistance = Mathf.Abs(rect.anchoredPosition.x - (defaultPosition - selectedPosition) * referenceResolution.x);
        //ManageSwipeHint();
    }

    // public float maximumCluelessTime;
    // public float cluelessTime;
    // public float successRating;
    // public float backwardSwipeSuccessFalloff = 1;
    // public bool hintEnabled;
    // public Image[] leftChevrons;
    // public Image[] rightChevrons;
    // public RectTransform chevronParent;
    // void ManageSwipeHint()
    // {
    //     if (transitionDistance < referenceWidth * 0.1f)
    //     {
    //         cluelessTime += Time.deltaTime;
    //         hintEnabled = cluelessTime > maximumCluelessTime * successRating;
    //     }
    //     else
    //     {
    //         hintEnabled = false;
    //     }


    //     ManageChevrons(rightChevrons, selectedPosition == lastPosition || !hintEnabled);
    //     ManageChevrons(leftChevrons, selectedPosition == 0 || !hintEnabled);
    // }

    // void ManageChevrons(Image[] chevrons, bool fade)
    // {
    //     float excessTime = cluelessTime - maximumCluelessTime * successRating;

    //     for (int i = 0; i < chevrons.Length; i++)
    //     {
    //         Image c = chevrons[i];
    //         Color color = Color.black;
    //         color.a = fade ? 0 : (Mathf.Sin(Mathf.Clamp(excessTime * 2.0f, 0.0f, Mathf.Infinity) + i / Mathf.PI) + 1) / 2.0f * Mathf.Clamp01(excessTime * 0.5f);
    //         c.color = color;
    //     }
    // }

    protected override void ProcessInput()
    {
        base.ProcessInput();
        if (!afterDragLock)
        {
            int moveDirection = dragging && Mathf.Abs(initialInputPosition.screen.x - currentInputPosition.screen.x) > referenceResolution.x / 3.0f ? (int)Mathf.Sign(initialInputPosition.screen.x - currentInputPosition.screen.x) : 0;
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
            foreach (SlidingUserInterface i in interfaces)
            {
                i.OnMasterEnable();
            }
        }
    }
}
