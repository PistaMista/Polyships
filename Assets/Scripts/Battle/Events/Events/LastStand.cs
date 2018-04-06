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
            if (Battle.main.attacker == affectedPlayer)
            {
                Battle.main.attackerCapabilities.maximumArtilleryCount += artilleryAttackIncrease;
            }
        }

        protected override void OnSummon()
        {
            affectedPlayer = Battle.main.attacker;
        }
        protected override bool ProcessSummoningRoll()
        {
            return base.ProcessSummoningRoll() && (Battle.main.attackerCapabilities.maximumArtilleryCount <= triggerGunCountThreshold && Battle.main.attackerCapabilities.torpedoFiringAreaSize <= triggerTorpedoCountThreshold && Battle.main.attacker.board.intactShipCount <= triggerLiveShipCountThreshold);
        }
        public override int GetAdditionalAllowed()
        {
            return (GetEffectsInQueue<LastStand>().Length == 0) ? 1 : 0;
        }
    }
}
