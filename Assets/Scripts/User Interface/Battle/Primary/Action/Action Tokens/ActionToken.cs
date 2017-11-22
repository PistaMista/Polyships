using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionToken : MonoBehaviour
{
    public Object value;
    public TacticalTargetingBattleUserInterface owner;
    public Vector3 targetPosition;
    public Vector3 defaultPositionRelativeToPedestal;
    public bool inPedestal;
}
