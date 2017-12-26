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

    public override void Place(Tile[] location)
    {
        base.Place(location);

        concealmentArea = new List<Tile>();
        foreach (Tile shipTile in tiles)
        {
            for (int x = -2; x <= 2; x++)
            {
                for (int y = -2; y <= 2; y++)
                {
                    Vector2Int p = shipTile.coordinates + new Vector2Int(x, y);
                    if ((!(Mathf.Abs(x) < 2 && Mathf.Abs(y) < 2)) && p.x >= 0 && p.x < parentBoard.tiles.GetLength(0) && p.y >= 0 && p.y < parentBoard.tiles.GetLength(1))
                    {
                        Tile candidateTile = parentBoard.tiles[p.x, p.y];
                        if (!concealmentArea.Contains(candidateTile))
                        {
                            concealmentArea.Add(candidateTile);
                        }
                    }
                }
            }
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
