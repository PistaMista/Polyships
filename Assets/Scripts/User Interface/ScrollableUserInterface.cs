using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollableUserInterface : SlidingUserInterface
{
    public float maximumScrollDistance;
    public float currentScrollDistance;
    public float scrollDecceleration;
    public float scrollVelocity;

    protected override void Update()
    {
        base.Update();
        float candidateScrollDistance = currentScrollDistance + scrollVelocity * Time.deltaTime;
        if (candidateScrollDistance > maximumScrollDistance || candidateScrollDistance < 0)
        {
            if (candidateScrollDistance > maximumScrollDistance)
            {
                currentScrollDistance = maximumScrollDistance;
            }
            else
            {
                currentScrollDistance = 0;
            }
        }
        else
        {
            currentScrollDistance = candidateScrollDistance;
        }

        if (!dragging)
        {
            float newScrollVelocity = scrollVelocity - Mathf.Sign(scrollVelocity) * scrollDecceleration * Time.deltaTime;
            scrollVelocity = Mathf.Sign(newScrollVelocity) == Mathf.Sign(scrollVelocity) ? newScrollVelocity : 0;
        }

        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, currentScrollDistance);
    }

    protected override void ProcessInput()
    {
        base.ProcessInput();
        SlidingUserInterface_Master.lockedDirections = new bool[] { false, false };
        if (dragging)
        {
            scrollVelocity = dragVelocity.screen.y;
            if (scrollVelocity / Screen.height > 0.2f)
            {
                SlidingUserInterface_Master.lockedDirections = new bool[] { true, true };
            }
        }
    }

    protected override void SetState(UIState state)
    {
        base.SetState(state);
        if (state == UIState.ENABLING)
        {
            scrollVelocity = 0.0f;
        }
    }
}
