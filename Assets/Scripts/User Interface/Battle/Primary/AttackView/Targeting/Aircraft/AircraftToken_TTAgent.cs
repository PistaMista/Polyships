using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AircraftToken_TTAgent : Token_TTAgent
{
    public bool horizontal;
    LineMarker_UIAgent marker;
    public float markerRotationSpeed;


    protected override void OnPedestalStateChange()
    {
        base.OnPedestalStateChange();
        if (OnPedestal)
        {
            RemoveDynamicAgents<LineMarker_UIAgent>("", false);
        }
        else
        {
            marker = (LineMarker_UIAgent)CreateDynamicAgent("aircraft_targeting_line");
            marker.lineWidth = (1.00f - MiscellaneousVariables.it.boardTileSideLength) * 1.1f;

            Vector3 mod = Vector3.up * 0.0105f;
            Vector3[] nodes = new Vector3[2];

            nodes[0] = mod;
            nodes[1] = mod + Vector3.back * (owner.managedBoard.tiles.GetLength(1) + 1.0f);

            marker.Set(nodes, new int[][] { new int[] { 1 }, new int[0] });
            marker.transform.localPosition = Vector3.zero;
            marker.transform.rotation = Quaternion.LookRotation(Vector3.back);
            marker.State = UIState.ENABLING;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (marker != null)
        {
            marker.transform.rotation = Quaternion.RotateTowards(marker.transform.rotation, Quaternion.LookRotation(horizontal ? Vector3.left : Vector3.back), markerRotationSpeed * Time.deltaTime);
        }
    }
}
