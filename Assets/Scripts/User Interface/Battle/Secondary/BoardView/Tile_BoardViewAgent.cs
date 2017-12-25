using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile_BoardViewAgent : MovingUIAgent
{
    protected override void SetState(UIState state)
    {
        base.SetState(state);
        if (state == UIState.DISABLED)
        {
            Destroy(gameObject);
        }
    }
}
