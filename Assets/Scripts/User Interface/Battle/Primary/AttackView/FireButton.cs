using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireButton : MonoBehaviour
{
    public bool pushed;
    public GameObject buttonPart;
    public Vector3 activePosition;
    public Vector3 inactivePosition;
    public AttackViewUserInterface owner;
    public float pushRadius;
    public float transitionTime;
    Vector3 velocity;
    void Update()
    {
        if (owner)
        {
            Vector3 targetPosition = Vector3.zero;
            if (owner.State == UIState.DISABLING)
            {
                targetPosition = inactivePosition - Vector3.up * (inactivePosition.y + 10);
            }
            else
            {
                if (owner.activePrimaryTargeter)
                {
                    targetPosition = activePosition;
                }
                else
                {
                    targetPosition = inactivePosition;
                }
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, transitionTime);
        }
    }

    public void Push(Vector3 position)
    {
        position.y = 0;
        Vector3 buttonPosition = transform.position;
        buttonPosition.y = 0;
        pushed = Vector3.Distance(buttonPosition, position) < pushRadius;
    }
}
