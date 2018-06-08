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

        protected override bool IsForcedToExpire()
        {
            return visibleTo.arsenal.radars <= 0;
        }

        protected override void OnExpire(bool forced)
        {
            base.OnExpire(forced);
            if (!forced)
            {
                post_action += () =>
                {
                    Effect radarRecharge = CreateEffect(typeof(RadarRecharge));

                    radarRecharge.targetedPlayer = visibleTo;
                    radarRecharge.visibleTo = visibleTo;

                    AddToQueue(radarRecharge);
                };
            }
        }
        public override int GetTheoreticalMaximumAddableAmount()
        {
            int radarRechargeCount = Effect.GetEffectsInQueue(x => x.targetedPlayer == Battle.main.attacker, typeof(RadarRecharge), int.MaxValue).Length;
            int radarReconCount = Effect.GetEffectsInQueue(x => x.targetedPlayer == Battle.main.defender, typeof(RadarRecon), int.MaxValue).Length;
            return radarReconCount == 0 && radarRechargeCount == 0 && Battle.main.attacker.arsenal.radars > 0 ? 1 : 0;
        }

        protected override bool CheckGameplayRulesForAddition()
        {
            return true;
        }

        protected override bool IsConflictingWithEffect(Effect effect)
        {
            return effect is RadarRecon && effect.targetedPlayer == targetedPlayer || effect is RadarRecharge && effect.targetedPlayer == visibleTo; //Conflicts with radar targeted at the same player or the radar recharging
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
