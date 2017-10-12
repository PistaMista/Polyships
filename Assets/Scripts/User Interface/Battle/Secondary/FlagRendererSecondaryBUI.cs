using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagRendererSecondaryBUI : PlayerSecondaryBUI
{
    public struct FlagVoxel
    {
        public GameObject voxel;
        public Vector3 velocity;
        public Vector3 smoothedPosition;
    }

    public Material flagMaterial;
    public FlagVoxel[,] voxels;

    public delegate void OnCameraOcclusion();
    public OnCameraOcclusion onCameraOcclusion;

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
                break;
            case UIState.DISABLING:
                break;
        }
    }

    float targetHeightModifier;
    protected override void Update()
    {
        base.Update();
        targetHeightModifier = (state == UIState.DISABLING && Vector3.Distance(Camera.main.transform.position, managedPlayer.flagCameraPoint.transform.position) < 2.0f) ? (managedPlayer.boardCameraPoint.transform.position.y + 10) : (state == UIState.ENABLING) ? MiscellaneousVariables.it.boardUIRenderHeight : targetHeightModifier;

        Vector3 referenceSmoothedPosition = managedPlayer.transform.position - new Vector3(voxels.GetLength(0) / 2.0f, 0, voxels.GetLength(1) / 2.0f) + (state == UIState.DISABLING ? (managedPlayer.boardCameraPoint.transform.position.y + 10) * Vector3.up : Vector3.zero);
        for (int x = 0; x < voxels.GetLength(0); x++)
        {
            for (int z = 0; z < voxels.GetLength(1); z++)
            {
                Vector3 targetSmoothedPosition = referenceSmoothedPosition + new Vector3(x + 0.5f, 0, z + 0.5f);

                FlagVoxel v = voxels[x, z];
                v.smoothedPosition = Vector3.SmoothDamp(v.smoothedPosition, targetSmoothedPosition, ref v.velocity, 0.5f + (x + z) / 10.0f);
                v.voxel.transform.position = v.smoothedPosition + Vector3.forward * (Mathf.Sin((x + Time.time) / 2.0f) - 0.5f) / 10.0f;
                voxels[x, z] = v;
            }
        }

        if (onCameraOcclusion != null)
        {
            FlagVoxel voxel = voxels[voxels.GetLength(0) / 2, voxels.GetLength(1) / 2];
            if ((state == UIState.DISABLING && voxel.voxel.transform.position.y >= (Camera.main.transform.position.y - voxel.voxel.transform.lossyScale.y)) || (state == UIState.ENABLING && voxel.voxel.transform.position.y <= (Camera.main.transform.position.y - voxel.voxel.transform.lossyScale.y)))
            {
                onCameraOcclusion();
                onCameraOcclusion = null;
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
                GameObject voxel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                voxel.transform.SetParent(worldSpaceParent);

                Renderer rend = voxel.GetComponent<Renderer>();
                rend.material = flagMaterial;

                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetColor("_Color", managedPlayer.flag[x, z]);

                rend.SetPropertyBlock(block);

                voxels[x, z].voxel = voxel;
            }
        }

        Vector3 startingPosition = managedPlayer.transform.position - new Vector3(voxels.GetLength(0) / 2.0f, 0, voxels.GetLength(1) / 2.0f) /*+ ( ? (managedPlayer.boardCameraPoint.transform.position.y + 10) * Vector3.up : Vector3.zero)*/;
        for (int x = 0; x < voxels.GetLength(0); x++)
        {
            for (int z = 0; z < voxels.GetLength(1); z++)
            {
                voxels[x, z].voxel.transform.position = startingPosition + new Vector3(x + 0.5f, 0, z + 0.5f) /*+ Vector3.forward * (Mathf.Sin(x) - 0.5f)*/;
                voxels[x, z].smoothedPosition = voxels[x, z].voxel.transform.position;
            }
        }
    }
}
