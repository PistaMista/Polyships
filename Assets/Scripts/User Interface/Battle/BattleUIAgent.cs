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

    protected BattleUIAgent[] HookToThis<T>(string nameFilter, Player owner, bool creationMode)
    {
        return HookToThis<T>(nameFilter, owner, creationMode, creationMode ? 1200 : int.MaxValue);
    }
    protected BattleUIAgent[] HookToThis<T>(string nameFilter, Player owner, bool creationMode, int limit)
    {
        BattleUIAgent[] examinedArray = owner == null ? MiscellaneousVariables.it.generalBattleAgents.ToArray() : owner.uiAgents;

        int cycles = creationMode ? limit : Mathf.Min(examinedArray.Length, limit);
        List<BattleUIAgent> matches = new List<BattleUIAgent>();
        for (int i = 0; i < cycles; i++)
        {
            UIAgent candidate = creationMode ? RetrieveDynamicAgentPrefab<T>(nameFilter) : examinedArray[i];

            bool typeMatch = candidate is T && candidate is BattleUIAgent;
            bool nameMatch = candidate.name.Contains(nameFilter);

            if (typeMatch && nameMatch)
            {
                BattleUIAgent confirmedCandidate = (BattleUIAgent)candidate;
                if (creationMode) confirmedCandidate = Instantiate(confirmedCandidate).GetComponent<BattleUIAgent>();

                confirmedCandidate.hookedTo = this;
                dehooker += () => { confirmedCandidate.hookedTo = null; };
                if (creationMode) dehooker += () => { Destroy(confirmedCandidate.gameObject, 10.0f); };
                matches.Add(confirmedCandidate);
            }
        }

        return matches.ToArray();
    }

}