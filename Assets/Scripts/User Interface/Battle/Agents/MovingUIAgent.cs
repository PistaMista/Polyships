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
    public Vector3 lastTargetPosition;
    public bool firstRun = true;
    protected override void Update()
    {
        base.Update();
        Vector3 targetPosition = (int)State >= 2 ? enabledPositions[GetTargetPositionIndex()] : disabledPosition;
        if (targetPosition != lastTargetPosition || firstRun)
        {
            State = (int)State >= 2 ? UIState.ENABLING : UIState.DISABLING;

            if (Vector3.Distance(transform.localPosition, targetPosition) < movementFinishingDistance)
            {
                lastTargetPosition = targetPosition;
                firstRun = false;
            }
            else
            {
                Vector3 localVelocity = transform.parent ? transform.parent.InverseTransformDirection(globalVelocity) : globalVelocity;
                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref localVelocity, movementTime, movementMaxSpeed);
                globalVelocity = transform.parent ? transform.parent.TransformDirection(localVelocity) : localVelocity;
            }
        }
        else
        {
            State = (int)State >= 2 ? UIState.ENABLED : UIState.DISABLED;
        }
    }

    protected virtual int GetTargetPositionIndex()
    {
        return 0;
    }
}
