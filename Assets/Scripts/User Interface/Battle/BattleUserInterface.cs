using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BattleUserInterface : InputEnabledUserInterface
{
    public TutorialUserInterface[] tutorials;
    public SecondaryBattleUserInterface[] secondaries;
    public MaskableGraphicFader[] graphicFaders;
    public int beginningTutorialStage;
    public Transform worldSpaceParent;

    void Awake()
    {
        ResetWorldSpaceParent();
    }

    public void SetWorldSpaceParent(bool enabled)
    {
        worldSpaceParent.gameObject.SetActive(enabled);
        for (int i = 0; i < secondaries.Length; i++)
        {
            secondaries[i].SetWorldSpaceParent(enabled);
        }
    }

    protected virtual void ResetWorldSpaceParent()
    {
        if (worldSpaceParent != null)
        {
            Destroy(worldSpaceParent.gameObject);
        }
        worldSpaceParent = new GameObject("World Space Parent").transform;
        worldSpaceParent.SetParent(transform);
    }

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
            case UIState.ENABLED:
                SetInteractable(true);
                break;
            case UIState.DISABLING:
                SetInteractable(false);
                break;
        }

        for (int i = 0; i < graphicFaders.Length; i++)
        {
            graphicFaders[i].SetTargetColor(state == UIState.ENABLING ? Color.black : Color.clear, new Vector4(1, 1, 1, 0));
        }

        if (state == UIState.ENABLING || state == UIState.DISABLING)
        {
            for (int i = 0; i < secondaries.Length; i++)
            {
                secondaries[i].State = state;
            }
        }
    }

    public void SwitchToInterface(BattleUIType switchTo)
    {
        State = UIState.DISABLING;
        BattleUserInterface_Master.EnableUI(switchTo);
    }
}
