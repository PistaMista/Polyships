using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleUIAgents.Base;

namespace BattleUIAgents.Agents
{
    public class Flagvoxel : WorldBattleUIAgent
    {
        public GameObject voxel;
        public float xOffset;
        protected override void Update()
        {
            base.Update();
            if (linked)
            {
                voxel.transform.localPosition = Vector3.forward * (Mathf.Sin((xOffset + Time.time) / 2.0f) - 0.5f) / 20.0f * MiscellaneousVariables.it.flagVoxelScale;
            }
        }
    }
}