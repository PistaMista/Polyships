using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Gameplay;

namespace Gameplay.Effects
{
    public class RadarRecon : Effect
    {
        public Heatmap result;
        public override void OnTurnResume()
        {
            base.OnTurnResume();
            if (Battle.main.defender == targetedPlayer)
            {
                AI.Datamap datamap = new AI.Datamap(Battle.main.defender.board);

                result = AI.GetTargetmap(AI.GetStatisticalHeatmap(datamap).normalized + Battle.main.attacker.heatmap_recon * 0.65f, datamap);
            }
        }
        public override void OnTurnEnd()
        {
            base.OnTurnEnd();
            editable = false;
        }

        protected override bool IsExpired()
        {
            return base.IsExpired() || visibleTo.arsenal.radars <= 0;
        }
        public override int GetTheoreticalMaximumAddableAmount()
        {
            return Effect.GetEffectsInQueue(x => x is RadarRecon && x.targetedPlayer == Battle.main.defender || x is RadarRecharge && targetedPlayer == Battle.main.attacker, typeof(Effect), 1).Length == 0 && Battle.main.attacker.arsenal.radars > 0 ? 1 : 0;
        }

        protected override bool CheckGameplayRulesForAddition()
        {
            return true;
        }

        protected override bool IsConflictingWithEffect(Effect effect)
        {
            return effect is RadarRecon && effect.targetedPlayer == targetedPlayer || effect is RadarRecharge && effect.targetedPlayer == visibleTo; //Conflicts with radar targeted at the same player
        }

        public override string GetDescription()
        {
            string desc = "";
            if (!editable)
                desc = "Displays an overlay indicating likely ship positions. Darker means more likely. Lasts for " + FormattedDuration + ".";
            else
                desc = "Deploys radar, which provides statistical data about enemy ship positions.";

            return desc;
        }
    }
}
