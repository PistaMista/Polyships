using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class TorpedoReload : Event
    {
        public override void OnTurnStart()
        {
            base.OnTurnStart();
            if (targetedPlayer.arsenal.torpedoes <= 0)
            {
                expiredEffects.Add(this);
            }
        }

        public override void OnTurnEnd()
        {
            base.OnTurnEnd();
            if (duration == 0 && targetedPlayer.arsenal.torpedoes - targetedPlayer.arsenal.loadedTorpedoes > 0)
            {
                targetedPlayer.arsenal.loadedTorpedoes++;
            }
        }

        protected override bool IsTriggered()
        {
            AmmoRegistry arsenal = Battle.main.attacker.arsenal;
            return arsenal.torpedoes - arsenal.loadedTorpedoes > 0;
        }

        protected override void SetupEvent()
        {
            base.SetupEvent();
            targetedPlayer = Battle.main.attacker;
        }

        public override int GetTheoreticalMaximumAddableAmount()
        {
            return 2;
        }

        protected override bool CheckGameplayRulesForAddition()
        {
            return IsTriggered();
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