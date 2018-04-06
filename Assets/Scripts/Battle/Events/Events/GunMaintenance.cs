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
            Battle.main.attackerCapabilities.maximumArtilleryCount -= artilleryAttackDecrease;
        }

        public override void OnTurnEnd()
        {
            artilleryAttackDecrease = Mathf.Clamp(artilleryAttackDecrease, 0, Battle.main.attackerCapabilities.maximumArtilleryCount - 1);
            if (artilleryAttackDecrease == 0)
            {
                duration = 1;
            }

            base.OnTurnEnd();
        }
        public override int GetAdditionalAllowed()
        {
            return (GetEffectsInQueue<GunMaintenance>().Length == 0 && Battle.main.attackerCapabilities.maximumArtilleryCount > artilleryAttackDecrease) ? 1 : 0;
        }

        protected override void OnSummon()
        {
            affectedPlayer = Battle.main.attacker;
        }


        public override string GetDescription()
        {
            return "Number of gun attacks decreased by " + artilleryAttackDecrease + " for " + duration + " turns.";
        }
    }
}