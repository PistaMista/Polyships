using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagRendererSecondaryBUI : PlayerIDBoundSecondaryBUI
{
    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.ENABLING:
                if (dynamicUIAgents.Count == 0)
                {
                    Initialize();
                }
                break;
            case UIState.DISABLED:
                DestroyDynamicAgents();
                break;
        }
    }

    protected override void Update()
    {
        base.Update();
    }

    void Initialize()
    {
        UIAgentParent.transform.position = managedPlayer.transform.position;
        Vector3 startingPosition = -new Vector3(managedPlayer.flag.GetLength(0) / 2.0f, 0, managedPlayer.flag.GetLength(1) / 2.0f) * MiscellaneousVariables.it.flagVoxelScale /*+ ( ? (managedPlayer.boardCameraPoint.transform.position.y + 10) * Vector3.up : Vector3.zero)*/;

        for (int x = 0; x < managedPlayer.flag.GetLength(0); x++)
        {
            for (int z = 0; z < managedPlayer.flag.GetLength(1); z++)
            {
                FlagVoxel_FlagRendererAgent voxel = (FlagVoxel_FlagRendererAgent)CreateDynamicAgent("flag_voxel");
                voxel.transform.localScale = Vector3.one * MiscellaneousVariables.it.flagVoxelScale;
                voxel.enabledPositions = new Vector3[1] { new Vector3(x + 0.5f, 0, z + 0.5f) * MiscellaneousVariables.it.flagVoxelScale + startingPosition };
                voxel.disabledPosition = voxel.enabledPositions[0];
                voxel.disabledPosition.y = -10;

                voxel.xOffset = x;
                voxel.movementTime = 0.1f + (x + z) / 20.0f;

                Renderer rend = voxel.GetComponentInChildren<Renderer>();
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetColor("_Color", managedPlayer.flag[x, z]);
                rend.SetPropertyBlock(block);
            }
        }
    }
}
