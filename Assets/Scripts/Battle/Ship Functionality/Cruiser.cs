using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cruiser : Ship
{
    public Ship concealing;

    public override void OnDestruction()
    {
        base.OnDestruction();
        if (concealing)
        {
            concealing.concealed = false;
        }
    }

    public override int[] GetMetadata()
    {
        return new int[] { concealing ? concealing.index : -1 };
    }

    public override void AssignReferences(ShipData data)
    {
        base.AssignReferences(data);
        if (data.metadata[0] >= 0)
        {
            concealing = owner.ships[data.metadata[0]];
        }
    }
}
