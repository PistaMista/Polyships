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

        public override void OnTurnStart()
        {
            if (targetedPlayer.arsenal.guns <= 0) Expire(true, true);
            base.OnTurnStart();
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

        public override int Max()
        {
            return 2;
        }

        protected override bool Legal()
        {
            bool playerAlreadyHasGunMaintenance = Battle.main.effects.Exists(x => x is GunMaintenance && x.targetedPlayer == targetedPlayer);
            bool playerHasEnoughGunsToStillFireAfterDecrease = targetedPlayer.arsenal.guns > artilleryAttackDecrease;
            return !playerAlreadyHasGunMaintenance && playerHasEnoughGunsToStillFireAfterDecrease;
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