using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class TorpedoCooldown : Effect
    {
        public int[] durations;
        protected override bool IsExpired()
        {
            return base.IsExpired() || targetedPlayer.arsenal.torpedoes <= 0;
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

        public override int GetTheoreticalMaximumAddableAmount()
        {
            return 2;
        }

        protected override bool CheckGameplayRulesForAddition()
        {
            return true;
        }

        protected override bool IsConflictingWithEffect(Effect effect)
        {
            return effect.targetedPlayer == targetedPlayer && effect is TorpedoCooldown;
        }

        public override string GetDescription()
        {
            return "No torpedoes can be fired/reloaded for " + FormattedDuration + ".";
        }
    }
}
