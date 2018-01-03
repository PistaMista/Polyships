using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carrier : Ship
{
    public int aircraftCount;
    public int aircraftCapacity;
    public int[,] polarSearchLines = new int[0, 0];

    public override void OnTurnEnd()
    {
        base.OnTurnEnd();
        if (health > 0)
        {
            for (int lineIndex = 0; lineIndex < polarSearchLines.GetLength(0); lineIndex++)
            {

            }
        }
        else
        {
            polarSearchLines = new int[0, 0];
        }
    }

}
