using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class TorpedoCooldown : Event
    {
        public int[] durations;
        public override void OnTurnStart()
        {
            base.OnTurnStart();
            if (affectedPlayer.arsenal.torpedoes <= 0)
            {
                duration = 1;
            }
        }

        protected override bool GetSummoningRoll()
        {
            int last = Battle.main.attacker.arsenal.torpedoesFiredLastTurn;
            return last > 0;
        }

        protected override void OnSummon()
        {
            base.OnSummon();
            affectedPlayer = Battle.main.attacker;
            duration = durations[affectedPlayer.arsenal.torpedoesFiredLastTurn];

            Effect[] candidate = GetEffectsInQueue(x => { return x.affectedPlayer == affectedPlayer; }, typeof(TorpedoReload), 1);
            if (candidate.Length > 0)
            {
                RemoveFromQueue(candidate[0]);
            }
        }

        public override string GetDescription()
        {
            return "No torpedoes can be loaded for " + FormattedDuration + ".";
        }
    }
}
