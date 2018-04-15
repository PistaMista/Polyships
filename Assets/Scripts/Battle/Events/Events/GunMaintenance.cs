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
            OnTurnResume();
        }
        public override void OnTurnResume()
        {
            base.OnTurnResume();

            if (Battle.main.attacker == affectedPlayer)
            {
                artilleryAttackDecrease = Mathf.Clamp(artilleryAttackDecrease, 0, Battle.main.attackerCapabilities.maximumArtilleryCount - 1);
                if (artilleryAttackDecrease == 0)
                {
                    Effect.RemoveFromQueue(this);
                }
                else
                {
                    Battle.main.attackerCapabilities.maximumArtilleryCount -= artilleryAttackDecrease;
                }
            }
        }

        public override int GetAdditionalAllowed(bool ignoreObjectValues)
        {
            if (ignoreObjectValues)
            {
                return (GetEffectsInQueue(null, typeof(GunMaintenance), 1).Length == 0 && Battle.main.attackerCapabilities.maximumArtilleryCount > artilleryAttackDecrease) ? 1 : 0;
            }

            return (GetEffectsInQueue(x => { return x.affectedPlayer == Battle.main.attacker; }, typeof(GunMaintenance), 1).Length == 0 && Battle.main.attackerCapabilities.maximumArtilleryCount > artilleryAttackDecrease) ? 1 : 0;
        }

        protected override void OnSummon()
        {
            affectedPlayer = Battle.main.attacker;
        }


        public override string GetDescription()
        {
            return "Gun Maintenance - Number of your gun attacks is decreased by " + artilleryAttackDecrease + " for " + duration + (duration == 1 ? " turn" : " turns") + ".";
        }
    }
}