using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestal_TTAgent : MovingUIAgent
{
    public float height;
    public float radius;
    public TTUI owner;

    protected override int GetTargetPositionIndex()
    {
        return owner.IsSelectable() ? 1 : 2;
    }
}
