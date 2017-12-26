using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cruiser : Ship
{
    public Ship concealing;
    public List<Tile> concealmentArea;

    public override void Destroy()
    {
        base.Destroy();
        if (concealing)
        {
            concealing.concealedBy = null;
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
            concealing = parentBoard.ships[data.metadata[0]];
        }
    }
}
