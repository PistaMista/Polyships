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
            return !TorpedoReload.TorpedoesReloadableForPlayer(targetedPlayer);
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

        public override int Max()
        {
            return 2;
        }

        protected override bool Legal()
        {
            bool playerCanLoadTorpedoes = TorpedoReload.TorpedoesReloadableForPlayer(targetedPlayer);
            bool playerTorpedoesOnCooldown = Battle.main.effects.Exists(x => x is TorpedoCooldown && x.targetedPlayer == targetedPlayer);
            return playerCanLoadTorpedoes && !playerTorpedoesOnCooldown;
        }

        public override string GetDescription()
        {
            return "No torpedoes can be fired/reloaded for " + FormattedDuration + ".";
        }
    }
}
