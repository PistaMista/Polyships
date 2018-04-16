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
            if (affectedPlayer.arsenal.torpedoes <= 0)
            {
                duration = 1;
            }
        }

        public override void OnTurnEnd()
        {
            base.OnTurnEnd();
            if (duration == 0)
            {
                affectedPlayer.arsenal.loadedTorpedoes++;
            }
        }

        protected override bool GetSummoningRoll()
        {
            AmmoRegistry arsenal = Battle.main.attacker.arsenal;
            return arsenal.loadedTorpedoes < arsenal.loadedTorpedoCap && arsenal.torpedoes > 0;
        }

        protected override void OnSummon()
        {
            base.OnSummon();
            affectedPlayer = Battle.main.attacker;
        }

        protected override bool ConflictsWith(Effect effect)
        {
            return effect.affectedPlayer == Battle.main.attacker && (effect is TorpedoReload || effect is TorpedoCooldown);
        }

        public override string GetDescription()
        {
            return "Next torpedo will be loaded in " + FormattedDuration + ".";
        }
    }
}