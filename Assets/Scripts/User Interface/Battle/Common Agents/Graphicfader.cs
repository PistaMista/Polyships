using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BattleUIAgents.Base;

namespace BattleUIAgents.Agents
{
    public class Graphicfader : BattleUIAgent
    {
        Graphic graphic;
        float colorRate;
        public float fadeTime;
        [Range(0.00f, 1.00f)]
        public float maxAlpha;

        protected override void Start()
        {
            graphic = GetComponent<Graphic>();
        }

        protected override void Update()
        {
            base.Update();
            float targetAlpha = linked ? maxAlpha : 0.0f;
            Color color = graphic.color;

            if (Mathf.Abs(targetAlpha - color.a) > 0.008f)
            {
                color.a = Mathf.SmoothDamp(color.a, targetAlpha, ref colorRate, fadeTime);
            }
            else
            {
                color.a = targetAlpha;
            }

            graphic.color = color;
        }
    }
}
