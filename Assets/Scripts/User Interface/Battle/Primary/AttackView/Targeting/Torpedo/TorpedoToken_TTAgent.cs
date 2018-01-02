using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorpedoToken_TTAgent : Token_TTAgent
{
    protected override void OnPedestalStateChange()
    {
        base.OnPedestalStateChange();
        if (OnPedestal)
        {
            RemoveDynamicAgents<LineMarker_UIAgent>("", false);
        }
        else
        {
            LineMarker_UIAgent marker = (LineMarker_UIAgent)CreateDynamicAgent("torpedo_targeting_line");
            Vector3 mod = Vector3.up * 0.011f;
            Vector3[] nodes = new Vector3[2];

            nodes[0] = mod;
            nodes[1] = mod + Vector3.back * (owner.managedBoard.tiles.GetLength(1) + 2.0f);

            marker.Set(nodes, new int[][] { new int[] { 1 }, new int[0] });
            marker.transform.localPosition = Vector3.zero;
            marker.State = UIState.ENABLING;
        }
    }
}
