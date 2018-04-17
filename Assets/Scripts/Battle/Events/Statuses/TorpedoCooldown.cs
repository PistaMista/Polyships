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
            if (targetedPlayer.arsenal.torpedoes <= 0)
            {
                duration = 1;
            }
        }

        protected override bool IsTriggered()
        {
            int last = Battle.main.attacker.arsenal.torpedoesFiredLastTurn;
            return last > 0;
        }

        protected override void SetupEvent()
        {
            base.SetupEvent();
            targetedPlayer = Battle.main.attacker;
            duration = durations[targetedPlayer.arsenal.torpedoesFiredLastTurn];

            Effect[] candidate = GetEffectsInQueue(x => { return x.targetedPlayer == targetedPlayer; }, typeof(TorpedoReload), 1);
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
