using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Effects
{
    public class RadarRecharge : Effect
    {
        protected override bool IsForcedToExpire()
        {
            return targetedPlayer.arsenal.radars <= 0;
        }

        public override int Max()
        {
            return 2;
        }

        protected override bool Legal()
        {
            return targetedPlayer.arsenal.radars > 0;
        }

        protected override bool Conflicts(Effect effect)
        {
            return effect.targetedPlayer == targetedPlayer && effect is RadarRecharge;
        }

        public override string GetDescription()
        {
            return "Radar recharging for " + FormattedDuration + ".";
        }
    }
}
