using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionToken : MonoBehaviour
{
    public Object value;
    public TacticalTargetingBattleUserInterface owner;
    public Vector3 targetPosition;
    public Vector3 defaultPositionRelativeToPedestal;
    public float stackHeight;
    bool onPedestal;
    public bool OnPedestal
    {
        get
        {
            return onPedestal;
        }

        set
        {
            onPedestal = value;
            transform.SetParent(onPedestal ? owner.stackPedestal.transform : owner.transform);
        }
    }

    public float transitionTime;
    public Vector3 velocity;
    void Update()
    {
        if (onPedestal)
        {
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, defaultPositionRelativeToPedestal, ref velocity, transitionTime);
        }
        else
        {
            Vector3 hideModifier = owner.State == UIState.DISABLING ? -Vector3.up * (targetPosition.y + 10f) : Vector3.zero;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition + hideModifier, ref velocity, transitionTime);
        }
    }
}
