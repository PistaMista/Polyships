using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class RadarRecharge : Event
    {
        public override void OnTurnStart()
        {
            base.OnTurnStart();
            if (targetedPlayer.arsenal.radars <= 0)
            {
                expiredEffects.Add(this);
            }
        }

        protected override bool IsTriggered()
        {
            return false;
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
            return targetedPlayer.arsenal.radars > 0;
        }

        protected override bool IsConflictingWithEffect(Effect effect)
        {
            return effect.targetedPlayer == targetedPlayer && effect is RadarRecharge;
        }

        public override string GetDescription()
        {
            return "Radar recharging for " + FormattedDuration + ".";
        }
    }
}
