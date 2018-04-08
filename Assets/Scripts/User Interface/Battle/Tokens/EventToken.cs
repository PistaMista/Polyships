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

        public override bool HasConnectionWithExistingEffect()
        {
            foreach (Effect effect in Battle.main.effects)
            {
                if (effect.GetType() == effectType.GetType() && effect.affectedPlayer != Battle.main.defender)
                {
                    if (FindAgent(x => { return (x as Token).effect == effect; }, typeof(Token)) == null)
                    {
                        this.effect = effect;
                        return true;
                    }
                }
            }

            return false;
        }

        protected sealed override void Pickup()
        {
            heldToken = this;
        }

        public sealed override void Drop()
        {
            heldToken = null;
        }
    }
}
