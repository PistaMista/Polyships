using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : Ship
{
    public int torpedoCount;

    public override int[] GetMetadata()
    {
        return new int[] { torpedoCount };
    }

    public override void Initialize(ShipData data)
    {
        base.Initialize(data);
        torpedoCount = data.metadata[0];
    }
}
