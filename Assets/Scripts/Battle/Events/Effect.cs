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
            expiredEffects.Add(this);
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

    public static Effect CreateEffect<T>()
    {
        Effect result = null;
        Effect candidate = RetrieveEffectPrefab<T>();
        if (candidate.GetAdditionalAllowed() > 0)
        {
            result = Instantiate(candidate.gameObject).GetComponent<Effect>();
        }

        return result;
    }

    public static Effect RetrieveEffectPrefab<T>()
    {
        Effect result = null;
        for (int i = 0; i < MiscellaneousVariables.it.effectPrefabs.Length; i++)
        {
            Effect candidate = MiscellaneousVariables.it.effectPrefabs[i].GetComponent<Effect>();
            if (candidate is T)
            {
                result = candidate;
                break;
            }
        }

        return result;
    }

    public static bool AddToQueue(Effect effect)
    {
        if (effect.GetAdditionalAllowed() <= 0)
        {
            return false;
        }

        int insertionIndex = 0;
        foreach (Effect measure in Battle.main.effects)
        {
            if (measure.priority >= effect.priority)
            {
                insertionIndex++;
            }
            else
            {
                break;
            }
        }

        Battle.main.effects.Insert(insertionIndex, effect);
        foreach (Effect affected in Battle.main.effects)
        {
            if (affected != effect)
            {
                affected.OnOtherEffectAdd(effect);
            }
        }

        effect.transform.SetParent(Battle.main.transform);
        return true;
    }

    public static void RemoveFromQueue(Effect effect)
    {
        Battle.main.effects.Remove(effect);
        foreach (Effect affected in Battle.main.effects)
        {
            affected.OnOtherEffectRemove(effect);
        }

        Destroy(effect.gameObject);
    }

    static List<Effect> expiredEffects = new List<Effect>();
    public static void RemoveExpiredEffectsFromQueue()
    {
        foreach (Effect effect in expiredEffects)
        {
            RemoveFromQueue(effect);
        }
    }

    public static int GetAmountInQueue<T>()
    {
        int result = 0;
        foreach (Effect effect in Battle.main.effects)
        {
            if (effect is T) result++;
        }

        return result;
    }
}
