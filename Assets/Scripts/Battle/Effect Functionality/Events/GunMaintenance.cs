using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class GunMaintenance : Event
    {
        public int artilleryAttackDecrease;

        protected override int[] GetMetadata()
        {
            return new int[1] { artilleryAttackDecrease };
        }
        public override void Initialize(EffectData data)
        {
            base.Initialize(data);
            artilleryAttackDecrease = data.metadata[0];
        }
        protected override bool IsForcedToExpire()
        {
            return targetedPlayer.arsenal.guns <= 0;
        }

        public override void OnEffectAdd(Effect addedEffect)
        {
            base.OnEffectAdd(addedEffect);
            if (addedEffect == this)
            {
                targetedPlayer.arsenal.guns -= artilleryAttackDecrease;
            }
        }

        public override void OnEffectRemove(Effect removedEffect)
        {
            base.OnEffectAdd(removedEffect);
            if (removedEffect == this)
            {
                targetedPlayer.arsenal.guns += artilleryAttackDecrease;
            }
        }

        public override int GetTheoreticalMaximumAddableAmount()
        {
            return 2;
        }

        protected override bool IsConflictingWithEffect(Effect effect)
        {
            return effect is GunMaintenance && effect.targetedPlayer == targetedPlayer;
        }

        protected override bool CheckGameplayRulesForAddition()
        {
            return targetedPlayer.arsenal.guns > artilleryAttackDecrease;
        }

        protected override void SetupEvent()
        {
            targetedPlayer = Battle.main.attacker;
        }


        public override string GetDescription()
        {
            return "Gun Maintenance - Number of your gun attacks is decreased by " + artilleryAttackDecrease + " for " + FormattedDuration + ".";
        }
    }
}