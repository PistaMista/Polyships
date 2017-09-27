using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleUIType
{
    ATTACKER_INFO,
    ATTACK_VIEW,
    BATTLE_OVERVIEW,
    DAMAGE_REPORT,
    TURN_NOTIFIER,
    FLEET_PLACEMENT
}
public class BattleUserInterface : InputEnabledUserInterface
{
    public TutorialUserInterface[] tutorials;
    public MaskableGraphicFader[] graphicFaders;
    public int beginningTutorialStage;
    public void Tutorial()
    {
        int candidateTutorial = Battle.main.tutorialStage - beginningTutorialStage;
        if (candidateTutorial >= 0 && candidateTutorial < tutorials.Length)
        {
            for (int i = 0; i < tutorials.Length; i++)
            {
                if (i == candidateTutorial)
                {
                    tutorials[i].State = UIState.ENABLING;
                    tutorials[i].onTutorialComplete += Tutorial;
                }
                else
                {
                    tutorials[i].State = UIState.DISABLING;
                    tutorials[i].onTutorialComplete -= Tutorial;
                }
            }
        }


    }

    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.ENABLING:
                Tutorial();
                break;
            case UIState.DISABLING:
                break;
        }

        for (int i = 0; i < graphicFaders.Length; i++)
        {
            graphicFaders[i].SetTargetColor(state == UIState.ENABLING ? Color.black : Color.clear, new Vector4(1, 1, 1, 0));
        }
    }

    public void SwitchToInterface(BattleUIType switchTo)
    {
        State = UIState.DISABLING;
        BattleUserInterface_Master.EnableUI(switchTo);
    }
}
