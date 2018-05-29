using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleUIAgents.Base;

namespace BattleUIAgents.Agents
{
    [RequireComponent(typeof(Renderer))]
    public class Tile : WorldBattleUIAgent
    {
        public void SetMaterialAndColor(Material material, Color color)
        {
            Renderer renderer = GetComponent<Renderer>();
            renderer.material = material;
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_Color", color);

            renderer.SetPropertyBlock(block);
        }
    }
}
