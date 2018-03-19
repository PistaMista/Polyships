using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TitleUI
{
    public class BattleModeSelector : TitleSlave
    {
        protected override void SetState(UIState state)
        {
            base.SetState(state);
            SetInteractable((int)state >= 2);
        }

        public void SelectMode(bool ai)
        {
            Next();
            BattleTweaker.aiOpponent = ai;
        }
    }
}
