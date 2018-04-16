using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class LastStand : Event
    {
        public int triggerGunCountThreshold;
        public int triggerTorpedoCountThreshold;
        public int triggerLiveShipCountThreshold;
        public int artilleryAttackIncrease;

        protected override void OnSummon()
        {
            affectedPlayer = Battle.main.attacker;
        }

        public override void OnAnyEffectAdd(Effect addedEffect)
        {
            base.OnAnyEffectAdd(addedEffect);
            if (addedEffect == this)
            {
                affectedPlayer.arsenal.guns += artilleryAttackIncrease;
            }
        }

        public override void OnAnyEffectRemove(Effect removedEffect)
        {
            base.OnAnyEffectAdd(removedEffect);
            if (removedEffect == this)
            {
                affectedPlayer.arsenal.guns -= artilleryAttackIncrease;
            }
        }
        protected override bool GetSummoningRoll()
        {
            return base.GetSummoningRoll() && (Battle.main.attacker.arsenal.guns <= triggerGunCountThreshold && Battle.main.attacker.arsenal.torpedoes <= triggerTorpedoCountThreshold && Battle.main.attacker.board.intactShipCount <= triggerLiveShipCountThreshold);
        }

        protected override bool ConflictsWith(Effect effect)
        {
            return effect is LastStand;
        }

        public override string GetDescription()
        {
            return "We WILL NOT go down without a fight!";
        }
    }
}
