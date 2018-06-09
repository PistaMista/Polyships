using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class TorpedoReload : Effect
    {
        protected override bool IsForcedToExpire()
        {
            return targetedPlayer.arsenal.torpedoes <= 0;
        }

        protected override void OnExpire(bool forced)
        {
            if (!forced)
            {
                targetedPlayer.arsenal.loadedTorpedoes++;

                if (TorpedoesReloadableForPlayer(targetedPlayer))
                {
                    post_action += () =>
                    {
                        Effect reload = CreateEffect(typeof(TorpedoReload));

                        reload.targetedPlayer = targetedPlayer;
                        reload.visibleTo = visibleTo;

                        AddToQueue(reload);
                    };
                }
            }
        }

        public override int GetTheoreticalMaximumAddableAmount()
        {
            return 2;
        }

        protected override bool CheckGameplayRulesForAddition()
        {
            return TorpedoesReloadableForPlayer(Battle.main.attacker);
        }

        public static bool TorpedoesReloadableForPlayer(Player player)
        {
            AmmoRegistry arsenal = player.arsenal;
            return arsenal.torpedoes - arsenal.loadedTorpedoes > 0 && arsenal.loadedTorpedoes < arsenal.loadedTorpedoCap;
        }

        protected override bool IsConflictingWithEffect(Effect effect)
        {
            return effect.targetedPlayer == targetedPlayer && (effect is TorpedoReload || effect is TorpedoCooldown);
        }

        public override string GetDescription()
        {
            return "Next torpedo will be loaded in " + FormattedDuration + ".";
        }
    }
}