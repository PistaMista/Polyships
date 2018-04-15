using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class Effect : BattleBehaviour
    {
        [Serializable]
        public struct EffectData
        {
            public bool affectingAll;
            public bool affectingAttacker;
            public bool visibleToAll;
            public bool visibleToAttacker;
            public int duration;
            public int priority;
            public bool editable;
            public int prefabIndex;
            public int[] metadata;
            public static implicit operator EffectData(Effect effect)
            {
                EffectData result;

                result.affectingAttacker = effect.affectedPlayer == Battle.main.attacker;
                result.affectingAll = effect.affectedPlayer == null;

                result.visibleToAttacker = effect.visibleTo == Battle.main.attacker;
                result.visibleToAll = effect.visibleTo == null;

                result.duration = effect.duration;
                result.priority = effect.priority;
                result.editable = effect.editable;
                result.prefabIndex = effect.prefabIndex;

                result.metadata = effect.GetMetadata();

                return result;
            }
        }

        public virtual void Initialize(EffectData data)
        {
            duration = data.duration;
            priority = data.priority;
            editable = data.editable;
            prefabIndex = data.prefabIndex;
        }

        public virtual void AssignReferences(EffectData data)
        {
            affectedPlayer = data.affectingAll ? null : data.affectingAttacker ? Battle.main.attacker : Battle.main.defender;
            visibleTo = data.visibleToAll ? null : data.visibleToAttacker ? Battle.main.attacker : Battle.main.defender;
        }

        protected virtual int[] GetMetadata()
        {
            return new int[0];
        }

        public Effect[] conflictingEffects;
        public Player affectedPlayer;
        public Player visibleTo;
        public int duration; //The amount of turns this effect lasts
        public int priority; //The priority this effect takes over others
        public bool editable;
        public int prefabIndex;

        /// <summary>
        /// Executes every time a new turn starts.
        /// </summary>
        public override void OnTurnStart()
        {
            base.OnTurnStart();
        }

        /// <summary>
        /// Executes every time a game is loaded and the current turn is therefore resumed.
        /// </summary>
        public override void OnTurnResume()
        {
            base.OnTurnResume();
        }

        /// <summary>
        /// Executes every time a turn ends.
        /// </summary>
        public override void OnTurnEnd()
        {
            base.OnTurnEnd();
            duration--;
            if (duration == 0)
            {
                expiredEffects.Add(this);
            }
        }

        /// <summary>
        /// Gets the amount of this effect that can be applied.
        /// </summary>
        /// <param name="ignoreObjectValues">Ignores any data this object carries and uses the type as a whole instead.</param>
        /// <returns>How many of these effects can we add.</returns>
        public virtual int GetAdditionalAllowed(bool ignoreObjectValues)
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


        /// <summary>
        /// Checks if this effect conflicts with another.
        /// </summary>
        /// <param name="effect">Potential conflictor effect.</param>
        /// <returns>Whether the effect conflicts.</returns>
        protected virtual bool ConflictsWith(Effect effect)
        {
            return ConflictsWithType(effect.GetType());
        }

        /// <summary>
        /// Checks if this effect type conflicts with another.
        /// </summary>
        /// <param name="type">Potential conflictor effect type.</param>
        /// <returns>Whether the effect type conflicts.</returns>
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

        /// <summary>
        /// Executes when another effect is added.
        /// </summary>
        /// <param name="addedEffect">Effect that was added.</param>
        public virtual void OnOtherEffectAdd(Effect addedEffect)
        {

        }

        /// <summary>
        /// Executes when another effect is removed.
        /// </summary>
        /// <param name="removedEffect">Effect that was removed.</param>
        public virtual void OnOtherEffectRemove(Effect removedEffect)
        {

        }

        /// <summary>
        /// Creates an effect object.
        /// </summary>
        /// <param name="type">The type of effect to create.</param>
        /// <returns>The created effect.</returns>
        public static Effect CreateEffect(Type type)
        {
            Effect result = null;
            Effect candidate = RetrieveEffectPrefab(type);
            result = Instantiate(candidate.gameObject).GetComponent<Effect>();


            return result;
        }

        /// <summary>
        /// Retrieves the prefab for an effect.
        /// </summary>
        /// <param name="type">Type of effect prefab.</param>
        /// <returns>The prefab of the effect.</returns>
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

        /// <summary>
        /// If this effect can be added to the queue, adds it.
        /// </summary>
        /// <param name="effect">The effect to be added.</param>
        /// <returns>Whether the effect was added.</returns>
        public static bool AddToQueue(Effect effect)
        {
            if (effect.GetAdditionalAllowed(false) <= 0)
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

        /// <summary>
        /// Removes an effect from the queue.
        /// </summary>
        /// <param name="effect">Effect to remove.</param>
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
        /// <summary>
        /// Removes expired effects from the queue.
        /// </summary>
        public static void RemoveExpiredEffectsFromQueue()
        {
            foreach (Effect effect in expiredEffects)
            {
                RemoveFromQueue(effect);
            }

            expiredEffects = new List<Effect>();
        }

        /// <summary>
        /// Finds effects of one type in the queue.
        /// </summary>
        /// <returns>Found effects.</returns>
        public static Effect[] GetEffectsInQueue(Predicate<Effect> predicate, Type type, uint limit)
        {
            if (predicate == null)
            {
                predicate = x => { return true; };
            }

            List<Effect> matches = new List<Effect>();


            foreach (Effect candidate in Battle.main.effects)
            {
                if (predicate(candidate) && candidate.GetType() == type) matches.Add(candidate);
                if (matches.Count == limit) break;
            }

            return matches.ToArray();
        }

        /// <summary>
        /// Gets the description for what the effect does.
        /// </summary>
        /// <returns>Short detailed description for player's eyes.</returns>
        public virtual string GetDescription()
        {
            return "";
        }
    }
}