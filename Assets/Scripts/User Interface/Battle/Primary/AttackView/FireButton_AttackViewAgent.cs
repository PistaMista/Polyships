using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireButton_AttackViewAgent : MovingUIAgent
{

    public bool pushed;
    public GameObject buttonPart;
    public AttackViewUI owner;
    public float pushRadius;
    protected override void Update()
    {
        base.Update();

    }

    public void Push(Vector3 position)
    {
        position.y = 0;
        Vector3 buttonPosition = transform.position;
        buttonPosition.y = 0;
        pushed = Vector3.Distance(buttonPosition, position) < pushRadius;
    }
}
