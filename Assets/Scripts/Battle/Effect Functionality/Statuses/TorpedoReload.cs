using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class TorpedoReload : Effect
    {
        protected override bool IsForcedToExpire()
        {
            return !TorpedoesReloadableForPlayer(targetedPlayer) || Battle.main.effects.Exists(x => (x is TorpedoCooldown && x.targetedPlayer == targetedPlayer) || (x is TorpedoAttack && x.targetedPlayer != targetedPlayer));
        }

        protected override void OnExpire(bool forced)
        {
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
            return TorpedoesReloadableForPlayer(Battle.main.attacker);
        }

        public static bool TorpedoesReloadableForPlayer(Player player)
        {
            AmmoRegistry arsenal = player.arsenal;
            return arsenal.torpedoes - arsenal.loadedTorpedoes > 0 && arsenal.loadedTorpedoes < arsenal.loadedTorpedoCap;
        }

        protected override bool Conflicts(Effect effect)
        {
            return effect.targetedPlayer == targetedPlayer && (effect is TorpedoReload || effect is TorpedoCooldown);
        }

        public override string GetDescription()
        {
            return "Next torpedo will be loaded in " + FormattedDuration + ".";
        }
    }
}