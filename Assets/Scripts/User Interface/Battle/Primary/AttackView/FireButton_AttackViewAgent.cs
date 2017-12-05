using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireButton_AttackViewAgent : MovingUIAgent
{
    void Awake()
    {
        initialPartPosition = buttonPart.transform.localPosition;
    }
    public bool pushed;
    public GameObject buttonPart;
    public Vector3 initialPartPosition;
    public AttackViewUI owner;
    public float pushRadius;

    public void Push(Vector3 position)
    {
        position.y = 0;
        Vector3 buttonPosition = transform.position;
        buttonPosition.y = 0;
        pushed = Vector3.Distance(buttonPosition, position) < pushRadius;
    }

    protected override void Update()
    {
        base.Update();
        buttonPart.transform.localPosition = pushed ? Vector3.zero : initialPartPosition;
    }

    protected override int GetTargetPositionIndex()
    {
        return owner.activePrimaryTargeter ? 0 : 1;
    }
}
