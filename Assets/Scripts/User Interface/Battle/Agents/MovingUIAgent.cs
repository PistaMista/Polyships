using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingUIAgent : UIAgent
{
    public Vector3 disabledPosition;
    public Vector3[] enabledPositions = new Vector3[0];
    public float movementMaxSpeed;
    public float movementTime;
    public float movementFinishingDistance;

    public Vector3 globalVelocity;
    Vector3 lastTargetPosition;
    protected override void Update()
    {
        base.Update();
        Vector3 targetPosition = (int)State >= 2 ? enabledPositions[GetTargetPositionIndex()] : disabledPosition;
        if (targetPosition != lastTargetPosition)
        {
            State = (int)State >= 2 ? UIState.ENABLING : UIState.DISABLING;
        }

        if (State == UIState.DISABLING || State == UIState.ENABLING)
        {
            if (Vector3.Distance(transform.localPosition, targetPosition) < movementFinishingDistance)
            {
                State = (int)State >= 2 ? UIState.ENABLED : UIState.DISABLED;
                lastTargetPosition = targetPosition;
            }
            else
            {
                Vector3 localVelocity = transform.parent ? transform.parent.InverseTransformDirection(globalVelocity) : globalVelocity;
                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref localVelocity, movementTime, movementMaxSpeed);
                globalVelocity = transform.parent ? transform.parent.TransformDirection(localVelocity) : localVelocity;
            }
        }
    }

    protected virtual int GetTargetPositionIndex()
    {
        return 0;
    }
}
