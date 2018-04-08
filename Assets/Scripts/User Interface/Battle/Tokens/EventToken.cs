using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Gameplay;

namespace BattleUIAgents.Tokens
{
    public class EventToken : Token
    {
        protected override void RefreshEffectRepresentation()
        {
            base.RefreshEffectRepresentation();
            MoveToStack();
        }

        protected override void Pickup()
        {
            heldToken = this;
        }

        public override void Drop()
        {
            heldToken = null;
        }
    }
}
