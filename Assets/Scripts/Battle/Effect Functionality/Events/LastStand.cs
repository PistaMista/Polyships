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
            return base.IsTriggered() && (Battle.main.defender.arsenal.guns - Battle.main.attacker.arsenal.guns >= triggerGunCountThreshold && Battle.main.attacker.arsenal.torpedoes <= triggerTorpedoCountThreshold && Battle.main.attacker.board.intactShipCount <= triggerLiveShipCountThreshold);
        }

        public override int Max()
        {
            return 1;
        }

        protected override bool Conflicts(Effect effect)
        {
            return effect is LastStand;
        }

        protected override bool Legal()
        {
            return true;
        }

        public override string GetDescription()
        {
            return "Gain " + artilleryAttackIncrease + " gun attacks. Show them what we're really made of!";
        }
    }
}
