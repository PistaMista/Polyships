using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class Effect : MonoBehaviour
    {
        [Serializable]
        public struct EffectData
        {
            public bool affectingAll;
            public bool affectingAttacker;
            public int duration;
            public int priority;
            public int prefabIndex;
            public int[] metadata;
            public static implicit operator EffectData(Effect effect)
            {
                EffectData result;

                result.affectingAttacker = effect.affectedPlayer == Battle.main.attacker;
                result.affectingAll = effect.affectedPlayer == null;

                result.duration = effect.duration;
                result.priority = effect.priority;
                result.prefabIndex = effect.prefabIndex;

                result.metadata = effect.GetMetadata();

                return result;
            }
        }

        public virtual void Initialize(EffectData data)
        {
            duration = data.duration;
            priority = data.priority;
            prefabIndex = data.prefabIndex;
        }

        public virtual void AssignReferences(EffectData data)
        {
            affectedPlayer = data.affectingAll ? null : data.affectingAttacker ? Battle.main.attacker : Battle.main.defender;
        }

        protected virtual int[] GetMetadata()
        {
            return new int[0];
        }

        public Effect[] conflictingEffects;
        public Player affectedPlayer;
        public int duration; //The amount of turns this effect lasts
        public int priority; //The priority this effect takes over others
        public int prefabIndex;

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

        public static Effect CreateEffect(Type type)
        {
            Effect result = null;
            Effect candidate = RetrieveEffectPrefab(type);
            if (candidate.GetAdditionalAllowed() > 0)
            {
                result = Instantiate(candidate.gameObject).GetComponent<Effect>();
            }

            return result;
        }

        public static Effect RetrieveEffectPrefab(Type type)
        {
            Effect result = null;
            for (int i = 0; i < MiscellaneousVariables.it.effectPrefabs.Length; i++)
            {
                Effect candidate = MiscellaneousVariables.it.effectPrefabs[i];
                if (ReferenceEquals(candidate.GetType(), type))
                {
                    result = candidate;
                    result.prefabIndex = i;
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

            expiredEffects = new List<Effect>();
        }

        public static Effect[] GetEffectsInQueue<T>()
        {
            List<Effect> result = new List<Effect>();
            foreach (Effect effect in Battle.main.effects)
            {
                if (effect is T) result.Add(effect);
            }

            return result.ToArray();
        }

        public virtual string GetDescription()
        {
            return "";
        }
    }
}