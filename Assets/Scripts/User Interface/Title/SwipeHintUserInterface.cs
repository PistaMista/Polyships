using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwipeHintUserInterface : BasicUserInterface
{
    public float maximumCluelessTime;
    public float cluelessTime;
    public float successRating;
    public float backwardSwipeSuccessFalloff = 1;
    int lastPosition;
    public bool hintEnabled;
    public Image[] leftChevrons;
    public Image[] rightChevrons;
    protected override void Update()
    {
        base.Update();
        if (SlidingUserInterface_Master.transitionDistance < Screen.width * 0.1f)
        {
            int change = SlidingUserInterface_Master.selectedPosition - lastPosition;
            if (change != 0)
            {
                if (change > 0)
                {
                    successRating += change;
                    backwardSwipeSuccessFalloff -= 0.3f * (SlidingUserInterface_Master.selectedPosition - SlidingUserInterface_Master.defaultPosition);
                }
                else
                {
                    successRating += change * backwardSwipeSuccessFalloff;
                }

                lastPosition += change;
                cluelessTime = 0;
            }
            else
            {
                cluelessTime += Time.deltaTime;
            }

            hintEnabled = cluelessTime > maximumCluelessTime * successRating;
        }
        else
        {
            hintEnabled = false;
        }


        ManageChevrons(rightChevrons, SlidingUserInterface_Master.selectedPosition == SlidingUserInterface_Master.lastPosition || !hintEnabled);
        ManageChevrons(leftChevrons, SlidingUserInterface_Master.selectedPosition == 0 || !hintEnabled);
        rect.anchoredPosition = Vector2.right * SlidingUserInterface_Master.selectedPosition * Screen.width;
    }

    void ManageChevrons(Image[] chevrons, bool fade)
    {
        float excessTime = cluelessTime - maximumCluelessTime * successRating;

        for (int i = 0; i < chevrons.Length; i++)
        {
            Image c = chevrons[i];
            Color color = Color.black;
            color.a = fade ? 0 : (Mathf.Sin(Mathf.Clamp(excessTime * 2.0f, 0.0f, Mathf.Infinity) + i / Mathf.PI) + 1) / 2.0f * Mathf.Clamp01(excessTime * 0.5f);
            c.color = color;
        }
    }
}
