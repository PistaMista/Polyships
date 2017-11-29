using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagRendererSecondaryBUI : PlayerIDBoundSecondaryBUI
{
    public struct FlagVoxel
    {
        public GameObject voxel;
        public Vector3 velocity;
        public Vector3 smoothedPosition;
    }

    public Material flagMaterial;
    public FlagVoxel[,] voxels;

    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.ENABLING:
                if (voxels == null)
                {
                    Initialize();
                }
                targetHeightModifier = 0.0f;
                break;
            case UIState.DISABLING:
                targetHeightModifier = -4.0f * MiscellaneousVariables.it.flagVoxelScale;
                break;
            case UIState.DISABLED:
                voxels = null;
                break;
        }

        referenceSmoothedPosition = managedPlayer.transform.position - new Vector3(voxels.GetLength(0) / 2.0f, 0, voxels.GetLength(1) / 2.0f) * MiscellaneousVariables.it.flagVoxelScale + Vector3.up * targetHeightModifier;
    }

    float targetHeightModifier;
    Vector3 referenceSmoothedPosition;
    protected override void Update()
    {
        base.Update();
        for (int x = 0; x < voxels.GetLength(0); x++)
        {
            for (int z = 0; z < voxels.GetLength(1); z++)
            {
                FlagVoxel v = voxels[x, z];
                if (v.voxel.transform.position.y > referenceSmoothedPosition.y * 0.8f || targetHeightModifier >= -1)
                {
                    Vector3 targetSmoothedPosition = referenceSmoothedPosition + new Vector3(x + 0.5f, 0, z + 0.5f) * MiscellaneousVariables.it.flagVoxelScale;


                    v.smoothedPosition = Vector3.SmoothDamp(v.smoothedPosition, targetSmoothedPosition, ref v.velocity, 0.1f + (x + z) / 20.0f);
                    v.voxel.transform.position = v.smoothedPosition + Vector3.forward * (Mathf.Sin((x + Time.time) / 2.0f) - 0.5f) / 10.0f * MiscellaneousVariables.it.flagVoxelScale;
                    voxels[x, z] = v;
                }
            }
        }
    }

    void Initialize()
    {
        voxels = new FlagVoxel[managedPlayer.flag.GetLength(0), managedPlayer.flag.GetLength(1)];
        for (int x = 0; x < voxels.GetLength(0); x++)
        {
            for (int z = 0; z < voxels.GetLength(1); z++)
            {
                GameObject voxel = CreateDynamicAgent(0).gameObject;
                voxel.transform.localScale = Vector3.one * MiscellaneousVariables.it.flagVoxelScale;

                Renderer rend = voxel.GetComponent<Renderer>();
                rend.material = flagMaterial;

                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetColor("_Color", managedPlayer.flag[x, z]);

                rend.SetPropertyBlock(block);

                voxels[x, z].voxel = voxel;
            }
        }

        Vector3 startingPosition = managedPlayer.transform.position - new Vector3(voxels.GetLength(0) / 2.0f, 0, voxels.GetLength(1) / 2.0f) * MiscellaneousVariables.it.flagVoxelScale /*+ ( ? (managedPlayer.boardCameraPoint.transform.position.y + 10) * Vector3.up : Vector3.zero)*/;
        for (int x = 0; x < voxels.GetLength(0); x++)
        {
            for (int z = 0; z < voxels.GetLength(1); z++)
            {
                voxels[x, z].voxel.transform.position = startingPosition + new Vector3(x + 0.5f, 0, z + 0.5f) * MiscellaneousVariables.it.flagVoxelScale /*+ Vector3.forward * (Mathf.Sin(x) - 0.5f)*/;
                voxels[x, z].smoothedPosition = voxels[x, z].voxel.transform.position;
            }
        }
    }
}
