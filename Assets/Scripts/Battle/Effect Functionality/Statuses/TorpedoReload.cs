using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class TorpedoReload : Effect
    {
        public override void OnTurnStart()
        {
            bool torpedoesOnCooldown = Battle.main.effects.Exists(x => x is TorpedoCooldown && x.targetedPlayer == targetedPlayer);
            bool torpedoesNoLongerReloadable = !TorpedoesReloadableForPlayer(targetedPlayer);
            if (torpedoesOnCooldown || torpedoesNoLongerReloadable) Expire(true, true);
            base.OnTurnStart();
        }

        protected override void Expire(bool forced, bool removeAtStart)
        {
            base.Expire(forced, removeAtStart);
            if (!forced)
            {
                targetedPlayer.arsenal.loadedTorpedoes++;

                if (TorpedoesReloadableForPlayer(targetedPlayer))
                {
                    turnEndAction += () =>
                    {
                        Effect reload = CreateEffect(typeof(TorpedoReload));

                        reload.targetedPlayer = targetedPlayer;
                        reload.visibleTo = visibleTo;

                        AddToStack(reload);
                    };
                }
            }
        }

        public override int Max()
        {
            return 2;
        }

        protected override bool Legal()
        {
            bool torpedoesOnCooldown = Battle.main.effects.Exists(x => x is TorpedoCooldown && x.targetedPlayer == targetedPlayer);
            bool torpedoesReloading = Battle.main.effects.Exists(x => x is TorpedoReload && x.targetedPlayer == targetedPlayer);
            bool torpedoesReloadable = TorpedoesReloadableForPlayer(Battle.main.attacker);
            return !torpedoesReloading && !torpedoesOnCooldown && torpedoesReloadable;
        }

        public static bool TorpedoesReloadableForPlayer(Player player)
        {
            AmmoRegistry arsenal = player.arsenal;
            return arsenal.torpedoes - arsenal.loadedTorpedoes > 0 && arsenal.loadedTorpedoes < arsenal.loadedTorpedoCap;
        }

        public override string GetDescription()
        {
            return "Next torpedo will be loaded in " + FormattedDuration + ".";
        }
    }
}