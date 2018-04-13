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
                Battle.main.attackerCapabilities.maximumArtilleryCount += artilleryAttackIncrease;
            }
        }

        protected override void OnSummon()
        {
            affectedPlayer = Battle.main.attacker;
        }
        protected override bool GetSummoningRoll()
        {
            return base.GetSummoningRoll() && (Battle.main.attackerCapabilities.maximumArtilleryCount <= triggerGunCountThreshold && Battle.main.attackerCapabilities.torpedoFiringAreaSize <= triggerTorpedoCountThreshold && Battle.main.attacker.board.intactShipCount <= triggerLiveShipCountThreshold);
        }
        public override int GetAdditionalAllowed(bool ignoreObjectValues)
        {
            return (GetEffectsInQueue(null, typeof(LastStand), 1).Length == 0) ? 1 : 0;
        }

        public override string GetDescription()
        {
            return "We WILL NOT go down without a fight!";
        }
    }
}
