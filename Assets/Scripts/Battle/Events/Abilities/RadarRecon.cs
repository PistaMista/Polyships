using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Gameplay;

namespace Gameplay.Effects
{
    public class RadarRecon : Effect
    {
        public Heatmap result;
        public override void OnTurnStart()
        {
            base.OnTurnStart();
            OnTurnResume();
        }
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
        public override int GetTheoreticalMaximumAddableAmount()
        {
            return Effect.GetEffectsInQueue(x => { return x.targetedPlayer == Battle.main.defender; }, typeof(RadarRecon), 1).Length == 0 && Battle.main.attacker.arsenal.radars > 0 ? 1 : 0;
        }

        protected override bool CheckGameplayRulesForAddition()
        {
            return true;
        }

        protected override bool IsConflictingWithEffect(Effect effect)
        {
            return effect is RadarRecon && effect.targetedPlayer == targetedPlayer; //Conflicts with radar targeted at the same player
        }

        public override string GetDescription()
        {
            string desc = "Shows most likely enemy positions. Isn't guarenteed to be right.";
            if (!editable)
            {
                desc += " Lasts for " + FormattedDuration + ".";
            }

            return desc;
        }
    }
}
