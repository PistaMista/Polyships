using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphicFader_UIAgent : UIAgent
{
    Graphic graphic;
    public Color disabledColor;
    public Color[] enabledColors = new Color[0];
    public float transitionEndThreshhold;
    public float transitionTime;
    public Color transitionVelocity;

    void Awake()
    {
        graphic = GetComponent<Graphic>();
    }

    Color lastTargetColor;
    protected override void Update()
    {
        base.Update();
        Color targetColor = (int)State >= 2 ? enabledColors[GetTargetColorIndex()] : disabledColor;
        if (targetColor != lastTargetColor)
        {
            lastTargetColor = targetColor;
            State = (int)State >= 2 ? UIState.ENABLING : UIState.DISABLING;
        }

        if (State == UIState.DISABLING || State == UIState.ENABLING)
        {
            if (Mathf.Abs(targetColor.r - graphic.color.r) < transitionEndThreshhold && Mathf.Abs(targetColor.g - graphic.color.g) < transitionEndThreshhold && Mathf.Abs(targetColor.b - graphic.color.b) < transitionEndThreshhold && Mathf.Abs(targetColor.a - graphic.color.a) < transitionEndThreshhold)
            {
                State = (int)State >= 2 ? UIState.ENABLED : UIState.DISABLED;
            }
            else
            {
                graphic.color = new Color(Mathf.SmoothDamp(graphic.color.r, targetColor.r, ref transitionVelocity.r, transitionTime), Mathf.SmoothDamp(graphic.color.g, targetColor.g, ref transitionVelocity.g, transitionTime), Mathf.SmoothDamp(graphic.color.b, targetColor.b, ref transitionVelocity.b, transitionTime), Mathf.SmoothDamp(graphic.color.a, targetColor.a, ref transitionVelocity.a, transitionTime));
            }
        }
    }

    protected virtual int GetTargetColorIndex()
    {
        return 0;
    }
}
