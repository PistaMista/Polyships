using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleUIAgents.Base;

namespace BattleUIAgents.Agents
{
    public class Flag : WorldBattleUIAgent
    {
        [Header("Flag Configuration")]
        public float voxelScale;
        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            Vector3 startingPosition = -new Vector3(player.flag.GetLength(0) / 2.0f, 0, player.flag.GetLength(1) / 2.0f) * voxelScale + Vector3.up * MiscellaneousVariables.it.flagRenderHeight + player.transform.position;

            BattleUIAgent[] foundVoxels = FindAgents(x => { return x is Flagvoxel && x.player == player && x.IsInvoking("DestroyAgent"); }, player.flag.Length);

            if (foundVoxels.Length != 0)
            {
                LinkAgents(foundVoxels);
            }
            else
            {
                for (int x = 0; x < player.flag.GetLength(0); x++)
                {
                    for (int z = 0; z < player.flag.GetLength(1); z++)
                    {
                        Flagvoxel voxel = (Flagvoxel)CreateAndLinkAgent<Flagvoxel>("");
                        voxel.player = player;

                        voxel.transform.localScale = Vector3.one * voxelScale;
                        voxel.hookedPosition = new Vector3(x + 0.5f, 0, z + 0.5f) * voxelScale + startingPosition;
                        voxel.unhookedPosition = voxel.hookedPosition;
                        voxel.unhookedPosition.y = -10;
                        voxel.transform.position = voxel.unhookedPosition;

                        voxel.xOffset = x;
                        voxel.movementTime = 0.025f + (x + z) / 80.0f;

                        Renderer rend = voxel.GetComponentInChildren<Renderer>();
                        MaterialPropertyBlock block = new MaterialPropertyBlock();
                        block.SetColor("_Color", player.flag[x, z]);
                        rend.SetPropertyBlock(block);
                    }
                }
            }
        }

        public bool IsPositionOnFlag(Vector3 position)
        {
            relativeWorldInputPosition = position - transform.position;
            return Mathf.Abs(relativeWorldInputPosition.x) < player.flag.GetLength(0) / 2.0f * voxelScale && Mathf.Abs(relativeWorldInputPosition.z) < player.flag.GetLength(1) / 2.0f * voxelScale;
        }
    }
}

