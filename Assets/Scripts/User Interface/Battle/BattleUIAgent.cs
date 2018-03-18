using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUIAgent : InputEnabledUI
{
    public Player player;
    public BattleUIAgent hookedTo;

    delegate void BattleAgentDehooker();
    BattleAgentDehooker dehooker;

    protected override void SetState(UIState state)
    {
        base.SetState(state);
        if ((int)state < 2 && dehooker != null)
        {
            dehooker();
            dehooker = null;
        }
        else if (dehooker == null)
        {
            GatherRequiredAgents();
        }
    }

    protected virtual void GatherRequiredAgents()
    {

    }
    protected BattleUIAgent HookToThis<T>(string nameFilter)
    {
        return HookToThis<T>(nameFilter, null);
    }
    protected BattleUIAgent HookToThis<T>(string nameFilter, Player owner)
    {
        return HookAllToThis<T>(nameFilter, owner, 1)[0];
    }
    protected BattleUIAgent[] HookAllToThis<T>(string nameFilter)
    {
        return HookAllToThis<T>(nameFilter, int.MaxValue);
    }
    protected BattleUIAgent[] HookAllToThis<T>(string nameFilter, int limit)
    {
        return HookAllToThis<T>(nameFilter, null, limit);
    }
    protected BattleUIAgent[] HookAllToThis<T>(string nameFilter, Player owner, int limit)
    {
        BattleUIAgent[] examinedArray = owner == null ? MiscellaneousVariables.it.generalBattleAgents.ToArray() : owner.uiAgents;

        List<BattleUIAgent> matches = new List<BattleUIAgent>();
        for (int i = 0; i < examinedArray.Length && matches.Count < limit; i++)
        {
            BattleUIAgent examined = examinedArray[i];

            bool typeMatch = examined is T;
            bool nameMatch = examined.name.Contains(nameFilter);

            if (typeMatch && nameMatch)
            {
                examined.hookedTo = this;
                dehooker += () => { examined.hookedTo = null; };
                matches.Add(examined);
            }
        }

        return matches.ToArray();
    }

}