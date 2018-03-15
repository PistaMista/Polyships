using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    public int playerAbilityIndex; //The token ID used by the player to do this effect - < 0 means none
    public Effect[] conflictingEffects;
    public int duration; //The amount of turns this effect lasts
    public int priority; //The priority this effect takes over others

    public virtual void OnTurnStart()
    {

    }

    public virtual void OnTurnEnd()
    {
        duration--;
        if (duration == 0)
        {
            Battle.main.RemoveEffect(this);
        }
    }

    //Checks how many of these effects can be added to the battle effect queue
    public virtual int GetAdditionalAllowed()
    {
        foreach (Effect potentialConflictor in Battle.main.effects)
        {
            if (ConflictsWith(potentialConflictor))
            {
                return 0;
            }
        }

        return 8;
    }

    //Checks if the effect conflicts with another
    protected virtual bool ConflictsWith(Effect effect)
    {
        return ConflictsWithType(effect.GetType());
    }

    protected bool ConflictsWithType(Type type)
    {
        for (int i = 0; i < conflictingEffects.Length; i++)
        {
            if (conflictingEffects[i].GetType() == type)
            {
                return true;
            }
        }

        return false;
    }

    public virtual void OnOtherEffectAdd(Effect addedEffect)
    {

    }

    public virtual void OnOtherEffectRemove(Effect removedEffect)
    {

    }

    public static Effect SummonEffect<T>()
    {
        Effect result = null;
        for (int i = 0; i < MiscellaneousVariables.it.effectPrefabs.Length; i++)
        {
            Effect candidate = MiscellaneousVariables.it.effectPrefabs[i].GetComponent<Effect>();
            if (candidate is T && candidate.CheckUsageConditions())
            {
                result = Instantiate(candidate.gameObject).GetComponent<Effect>();
                break;
            }
        }

        return result;
    }
}
