using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class TorpedoCooldown : Effect
    {
        public int[] durations;
        protected override bool IsForcedToExpire()
        {
            return targetedPlayer.arsenal.torpedoes <= 0;
        }

        protected override void OnExpire(bool forced)
        {
            base.OnExpire(forced);
            AmmoRegistry arsenal = targetedPlayer.arsenal;
            if (!forced && arsenal.torpedoes - arsenal.loadedTorpedoes > 0 && arsenal.loadedTorpedoes < arsenal.loadedTorpedoCap)
            {
                turnEndAction += () =>
                {
                    Effect torpedoReload = CreateEffect(typeof(TorpedoReload));

                    torpedoReload.targetedPlayer = targetedPlayer;
                    torpedoReload.visibleTo = visibleTo;

                    AddToStack(torpedoReload);
                };
            }
        }

        // protected override bool IsTriggered()
        // {
        //     int last = Battle.main.attacker.arsenal.torpedoesFiredLastTurn;
        //     return last > 0;
        // }

        // protected override void SetupEvent()
        // {
        //     base.SetupEvent();
        //     targetedPlayer = Battle.main.attacker;
        //     duration = durations[targetedPlayer.arsenal.torpedoesFiredLastTurn];

        //     Effect[] candidate = GetEffectsInQueue(x => { return x.targetedPlayer == targetedPlayer; }, typeof(TorpedoReload), 1);
        //     if (candidate.Length > 0)
        //     {
        //         RemoveFromQueue(candidate[0]);
        //     }
        // }

        public override int Max()
        {
            return 2;
        }

        protected override bool Legal()
        {
            return true;
        }

        protected override bool Conflicts(Effect effect)
        {
            return effect.targetedPlayer == targetedPlayer && effect is TorpedoCooldown;
        }

        public override string GetDescription()
        {
            return "No torpedoes can be fired/reloaded for " + FormattedDuration + ".";
        }
    }
}
