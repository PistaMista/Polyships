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
                turnEndAction += () =>
                {
                    Effect radarRecharge = CreateEffect(typeof(RadarRecharge));

                    radarRecharge.targetedPlayer = visibleTo;
                    radarRecharge.visibleTo = visibleTo;

                    AddToStack(radarRecharge);
                };
            }
        }
        public override int Max()
        {
            return (Battle.main.attacker.arsenal.radars > 0 && !Battle.main.effects.Exists(x => x is RadarRecharge && x.targetedPlayer == Battle.main.attacker) && !Battle.main.effects.Exists(x => x is RadarRecon && x.targetedPlayer == Battle.main.defender)) ? 1 : 0;
        }

        protected override bool Legal()
        {
            bool playerHasRadar = visibleTo.arsenal.radars > 0;
            bool radarAlreadyRunning = Battle.main.effects.Exists(x => x is RadarRecon && x.targetedPlayer == targetedPlayer);
            bool radarRecharging = Battle.main.effects.Exists(x => x is RadarRecharge && x.targetedPlayer == visibleTo);
            return !radarAlreadyRunning && !radarRecharging;
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
