using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Token_TTAgent : MovingUIAgent
{
    public object value;
    public TTUI owner;
    public float stackHeight;
    public float maxStackDeviation;
    bool onPedestal;
    public bool OnPedestal
    {
        get
        {
            return onPedestal;
        }
        set
        {
            if (value != onPedestal)
            {
                transform.SetParent(value ? owner.stackPedestal.transform : owner.childAgentDefaultParent);
                if (value)
                {
                    Vector3 stackStart = Vector3.up * (owner.stackPedestal.height / 2.0f - stackHeight / 2.0f);
                    Vector3 stackDeviation = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f)).normalized * Random.Range(maxStackDeviation * 0.2f, maxStackDeviation);
                    enabledPositions[0] = owner.stackPedestal.transform.InverseTransformDirection(stackStart + stackDeviation + Vector3.up * stackHeight * owner.stackTokens.Count);
                }

                onPedestal = value;

                OnPedestalStateChange();
            }
        }
    }

    protected override void SetState(UIState state)
    {
        base.SetState(state);
        if ((int)state < 2 && value != null)
        {
            movementFinishingDistance = 1;
            disabledPosition = enabledPositions[1];
            disabledPosition.y = owner.managedBoard.owner.boardCameraPoint.transform.position.y + 5.0f;
        }
        else
        {
            movementFinishingDistance = -1;
            disabledPosition = enabledPositions[0];
        }
    }

    protected override int GetTargetPositionIndex()
    {
        return onPedestal ? 0 : 1;
    }

    protected virtual void OnPedestalStateChange()
    {

    }
}
