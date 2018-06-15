using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class TorpedoCooldown : Effect
    {
        public int[] durations;
        public override void OnTurnStart()
        {
            if (!TorpedoReload.TorpedoesReloadableForPlayer(targetedPlayer)) Expire(true, true);
            base.OnTurnStart();
        }

        protected override void Expire(bool forced, bool removeAtStart)
        {
            base.Expire(forced, removeAtStart);
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
