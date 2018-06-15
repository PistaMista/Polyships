using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class RadarRecharge : Effect
    {
        public override void OnTurnStart()
        {
            if (targetedPlayer.arsenal.radars <= 0) Expire(true, true);
            base.OnTurnStart();
        }

        public override int Max()
        {
            return 2;
        }

        protected override bool Legal()
        {
            bool playerHasRadar = targetedPlayer.arsenal.radars > 0;
            bool playerRadarRecharging = Battle.main.effects.Exists(x => x is RadarRecharge && x.targetedPlayer == targetedPlayer);
            return playerHasRadar && !playerRadarRecharging;
        }
        public override string GetDescription()
        {
            return "Radar recharging for " + FormattedDuration + ".";
        }
    }
}
