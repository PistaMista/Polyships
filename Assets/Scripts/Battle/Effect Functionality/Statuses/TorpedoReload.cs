using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class TorpedoReload : Effect
    {
        public override void OnTurnStart()
        {
            base.OnTurnStart();

        }

        protected override bool IsForcedToExpire()
        {
            return targetedPlayer.arsenal.torpedoes <= 0;
        }

        protected override void OnExpire(bool forced)
        {
            if (!forced)
            {
                targetedPlayer.arsenal.loadedTorpedoes++;
            }
        }

        // protected override bool IsTriggered()
        // {
        //     AmmoRegistry arsenal = Battle.main.attacker.arsenal;
        //     return arsenal.torpedoes - arsenal.loadedTorpedoes > 0 && arsenal.loadedTorpedoes < arsenal.loadedTorpedoCap;
        // }

        // protected override void SetupEvent()
        // {
        //     base.SetupEvent();
        //     targetedPlayer = Battle.main.attacker;
        // }

        public override int GetTheoreticalMaximumAddableAmount()
        {
            return 2;
        }

        protected override bool CheckGameplayRulesForAddition()
        {
            AmmoRegistry arsenal = Battle.main.attacker.arsenal;
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