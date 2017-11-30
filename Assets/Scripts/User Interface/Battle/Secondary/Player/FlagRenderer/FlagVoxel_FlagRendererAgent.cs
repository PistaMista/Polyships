using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagVoxel_FlagRendererAgent : MovingUIAgent
{
    public GameObject voxel;
    public float xOffset;
    protected override void Update()
    {
        base.Update();
        if (State != UIState.DISABLED)
        {
            voxel.transform.localPosition = Vector3.forward * (Mathf.Sin((xOffset + Time.time) / 2.0f) - 0.5f) / 10.0f * MiscellaneousVariables.it.flagVoxelScale;
        }
    }
}
