using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUserInterface : BasicUserInterface
{
    public delegate void OnTutorialComplete();
    public OnTutorialComplete onTutorialComplete;
    public int tutorialStageToSetAfterCompletion;
    protected virtual void Complete()
    {
        Battle.main.tutorialStage = tutorialStageToSetAfterCompletion;
        if (onTutorialComplete != null)
        {
            onTutorialComplete();
        }
    }
}
