using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionToken : MonoBehaviour
{
    // public Object value;
    // public TTUI owner;
    // public Vector3 targetPosition;
    // public Vector3 defaultPositionRelativeToPedestal;
    // public float stackHeight;
    // bool onPedestal;
    // public bool OnPedestal
    // {
    //     get
    //     {
    //         return onPedestal;
    //     }
    //     set
    //     {
    //         if (value != onPedestal)
    //         {
    //             transform.SetParent(value ? owner.stackPedestal.transform : owner.transform);
    //             velocity = value ? owner.stackPedestal.transform.InverseTransformDirection(velocity) : owner.stackPedestal.transform.TransformDirection(velocity);

    //             if (value)
    //             {
    //                 Vector3 stackStart = Vector3.up * (owner.stackPedestalHeight / 2.0f - stackHeight / 2.0f);
    //                 Vector3 stackDeviation = new Vector3(Random.Range(-1.0f, 1.0f), 0, Random.Range(-1.0f, 1.0f)).normalized * Random.Range(owner.stackMaximumDeviation * 0.2f, owner.stackMaximumDeviation);
    //                 defaultPositionRelativeToPedestal = owner.stackPedestal.transform.InverseTransformDirection(stackStart + stackDeviation + Vector3.up * stackHeight * owner.stackTokens.Count);
    //             }

    //             onPedestal = value;
    //         }
    //     }
    // }

    // public float transitionTime;
    // public Vector3 velocity;
    // void Update()
    // {
    //     if (onPedestal)
    //     {
    //         transform.localPosition = Vector3.SmoothDamp(transform.localPosition, defaultPositionRelativeToPedestal, ref velocity, transitionTime);
    //     }
    //     else
    //     {
    //         Vector3 hideModifier = owner.State == UIState.DISABLING ? -Vector3.up * (targetPosition.y + 10f) : Vector3.zero;
    //         transform.position = Vector3.SmoothDamp(transform.position, targetPosition + hideModifier, ref velocity, transitionTime);
    //     }
    // }
}
