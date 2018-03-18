using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUIAgent : InputEnabledUI
{
    public Player player;
    public BattleUIAgent hookedTo;

    public delegate void BattleAgentDehooker();
    public BattleAgentDehooker dehooker;

    public void HookAllToThis<T>(string nameFilter)
    {
        HookAllToThis<T>(nameFilter);
    }
    public void HookAllToThis<T>(string nameFilter, Player owner)
    {
        BattleUIAgent[] examinedArray = owner == null ? MiscellaneousVariables.it.generalBattleAgents : owner.uiAgents;


    }
}