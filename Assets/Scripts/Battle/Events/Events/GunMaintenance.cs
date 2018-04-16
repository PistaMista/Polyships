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
            base.OnTurnStart();
            if (affectedPlayer.arsenal.guns <= 0)
            {
                duration = 1;
            }
        }

        public override void OnAnyEffectAdd(Effect addedEffect)
        {
            base.OnAnyEffectAdd(addedEffect);
            if (addedEffect == this)
            {
                affectedPlayer.arsenal.guns -= artilleryAttackDecrease;
            }
        }

        public override void OnAnyEffectRemove(Effect removedEffect)
        {
            base.OnAnyEffectAdd(removedEffect);
            if (removedEffect == this)
            {
                affectedPlayer.arsenal.guns += artilleryAttackDecrease;
            }
        }

        public override int GetAdditionalAllowed(bool ignoreObjectValues)
        {
            if (ignoreObjectValues)
            {
                return (GetEffectsInQueue(null, typeof(GunMaintenance), 1).Length == 0 && Battle.main.attacker.arsenal.guns > artilleryAttackDecrease) ? 1 : 0;
            }

            return (GetEffectsInQueue(x => { return x.affectedPlayer == Battle.main.attacker; }, typeof(GunMaintenance), 1).Length == 0 && Battle.main.attacker.arsenal.guns > artilleryAttackDecrease) ? 1 : 0;
        }

        protected override void OnSummon()
        {
            affectedPlayer = Battle.main.attacker;
        }


        public override string GetDescription()
        {
            return "Gun Maintenance - Number of your gun attacks is decreased by " + artilleryAttackDecrease + " for " + FormattedDuration + ".";
        }
    }
}