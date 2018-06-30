using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

namespace Gameplay.Effects
{
    public class LastStand : Event
    {
        [Range(0.1f, 1.0f)]
        public float attackerAdvantageTriggerThreshold;
        public int artilleryAttackIncrease;
        protected override void SetupEvent()
        {
            targetedPlayer = Battle.main.attacker;
        }

        public override void OnEffectAdd(Effect addedEffect)
        {
            base.OnEffectAdd(addedEffect);
            if (addedEffect == this)
            {
                targetedPlayer.arsenal.guns += artilleryAttackIncrease;
            }
        }

        public override void OnEffectRemove(Effect removedEffect)
        {
            base.OnEffectAdd(removedEffect);
            if (removedEffect == this)
            {
                targetedPlayer.arsenal.guns -= artilleryAttackIncrease;
            }
        }
        protected override bool IsTriggered()
        {
            float attacker_advantage = Battle.main.attacker.board.ships.Sum(x => x.health) / (float)Battle.main.defender.board.ships.Sum(x => x.health);
            return attacker_advantage < attackerAdvantageTriggerThreshold;
        }

        public override int Max()
        {
            return 1;
        }

        protected override bool Legal()
        {
            bool oneOfThePlayersHasLastStand = Battle.main.effects.Exists(x => x is LastStand);
            return !oneOfThePlayersHasLastStand;
        }

        public override string GetDescription()
        {
            return "Gain " + artilleryAttackIncrease + " gun attacks. Show them what we're really made of!";
        }
    }
}
