using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Gameplay;
using BattleUIAgents.Base;
using BattleUIAgents.UI;

namespace TitleUI
{
    public class BattleTweaker : TitleSlave
    {
        protected override void SetState(UIState state)
        {
            base.SetState(state);
            tutorialToggle.isOn = BattleLoader.allSlotsEmpty;
            SetInteractable((int)state >= 2);
        }

        public static bool aiOpponent;
        public Toggle tutorialToggle;
        public Slider boardSizeSlider;
        public static int saveSlot;
        public static float[][,,] flags;
        public void LaunchBattle()
        {
            Battle.main = Battle.CreateBattle(Battle.GetBlankBattleData((int)boardSizeSlider.value, aiOpponent, tutorialToggle.isOn, saveSlot, flags));
            State = UIState.DISABLED;
            BattleUIAgent.FindAgent(x => { return x is TurnNotifier; }).gameObject.SetActive(true);
        }



        public override void OnTitleSetState(UIState state)
        {
            base.OnTitleSetState(state);
            if ((int)state >= 2)
            {
                flags = new float[2][,,];
                for (int i = 0; i < 2; i++)
                {
                    flags[i] = new float[MiscellaneousVariables.it.flagResolution.x, MiscellaneousVariables.it.flagResolution.y, 3];
                    for (int x = 0; x < flags[i].GetLength(0); x++)
                    {
                        for (int y = 0; y < flags[i].GetLength(1); y++)
                        {
                            flags[i][x, y, 0] = 1;
                            flags[i][x, y, 1] = 1;
                            flags[i][x, y, 2] = 1;
                        }
                    }
                }
            }
        }
    }
}
