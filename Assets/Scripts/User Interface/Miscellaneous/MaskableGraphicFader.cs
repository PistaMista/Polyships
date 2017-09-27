using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaskableGraphicFader : MonoBehaviour
{
    Graphic graphic;
    public Color startingColor;
    public Color targetColor;
    public Color transitionVelocity;
    public float transitionTime;
    void Awake()
    {
        graphic = GetComponent<Graphic>();
        graphic.color = startingColor;
    }

    void Update()
    {
        graphic.color = new Color(Mathf.SmoothDamp(graphic.color.r, targetColor.r, ref transitionVelocity.r, transitionTime), Mathf.SmoothDamp(graphic.color.g, targetColor.g, ref transitionVelocity.g, transitionTime), Mathf.SmoothDamp(graphic.color.b, targetColor.b, ref transitionVelocity.b, transitionTime), Mathf.SmoothDamp(graphic.color.a, targetColor.a, ref transitionVelocity.a, transitionTime));
    }

    public void SetTargetColor(Color color, Vector4 ignoreMask)
    {
        targetColor = new Color(ignoreMask.x != 0 ? graphic.color.r : color.r, ignoreMask.y != 0 ? graphic.color.g : color.g, ignoreMask.z != 0 ? graphic.color.b : color.b, ignoreMask.w != 0 ? graphic.color.a : color.a);
    }
}
