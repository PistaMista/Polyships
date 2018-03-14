using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    public int playerAbilityIndex; //The token ID used by the player to do this effect - < 0 means none
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
            if (candidate is T)
            {
                result = Instantiate(candidate.gameObject).GetComponent<Effect>();
                break;
            }
        }

        return result;
    }
}
