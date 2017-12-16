using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagRendererUI : CombatantUI
{
    protected override void SetState(UIState state)
    {
        base.SetState(state);
        switch (state)
        {
            case UIState.ENABLING:
                if (dynamicUIAgents.Count == 0)
                {
                    Initialize();
                }
                break;
            case UIState.DISABLED:
                DestroyDynamicAgents<FlagVoxel_FlagRendererAgent>("");
                break;
        }
    }

    protected override void Update()
    {
        base.Update();
    }

    void Initialize()
    {
        childAgentDefaultParent.transform.position = managedBoard.owner.transform.position;
        Vector3 startingPosition = -new Vector3(managedBoard.owner.flag.GetLength(0) / 2.0f, 0, managedBoard.owner.flag.GetLength(1) / 2.0f) * MiscellaneousVariables.it.flagVoxelScale + Vector3.up * MiscellaneousVariables.it.flagRenderHeight;

        for (int x = 0; x < managedBoard.owner.flag.GetLength(0); x++)
        {
            for (int z = 0; z < managedBoard.owner.flag.GetLength(1); z++)
            {
                FlagVoxel_FlagRendererAgent voxel = (FlagVoxel_FlagRendererAgent)CreateDynamicAgent("flag_voxel");
                voxel.transform.localScale = Vector3.one * MiscellaneousVariables.it.flagVoxelScale;
                voxel.enabledPositions = new Vector3[1] { new Vector3(x + 0.5f, 0, z + 0.5f) * MiscellaneousVariables.it.flagVoxelScale + startingPosition };
                voxel.disabledPosition = voxel.enabledPositions[0];
                voxel.disabledPosition.y = -10;

                voxel.transform.localPosition = voxel.disabledPosition;

                voxel.xOffset = x;
                voxel.movementTime = 0.025f + (x + z) / 80.0f;

                Renderer rend = voxel.GetComponentInChildren<Renderer>();
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetColor("_Color", managedBoard.owner.flag[x, z]);
                rend.SetPropertyBlock(block);
            }
        }
    }
}
