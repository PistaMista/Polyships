﻿using System;
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

                result.affectingAttacker = effect.targetedPlayer == Battle.main.attacker;
                result.affectingAll = effect.targetedPlayer == null;

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
            targetedPlayer = data.affectingAll ? null : data.affectingAttacker ? Battle.main.attacker : Battle.main.defender;
            visibleTo = data.visibleToAll ? null : data.visibleToAttacker ? Battle.main.attacker : Battle.main.defender;

            transform.SetParent(targetedPlayer != null ? targetedPlayer.transform : Battle.main.transform);
        }

        protected virtual int[] GetMetadata()
        {
            return new int[0];
        }

        public Player targetedPlayer;
        public Player visibleTo;
        public int duration; //The amount of turns this effect lasts
        public string FormattedDuration
        {
            get
            {
                int displayDuration = Mathf.CeilToInt(duration / 2.0f);
                return displayDuration + " " + (displayDuration > 1 ? "turns" : "turn");
            }
        }
        public bool expired = false;
        public int priority; //The priority this effect takes over others
        public bool editable;
        public int prefabIndex;

        /// <summary>
        /// Gets the description for what the effect does.
        /// </summary>
        /// <returns>Short detailed description for player's eyes.</returns>
        public virtual string GetDescription()
        {
            return "";
        }

        /// <summary>
        /// Executes every time a new turn starts.
        /// </summary>
        public override void OnTurnStart()
        {
            base.OnTurnStart();
            if (expired) return;
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
            if (duration == 0 && !expired)
            {
                Expire(false, false);
                return;
            }

            if (expired) return;
        }

        /// <summary>
        /// Executes when any effect is added.
        /// </summary>
        /// <param name="addedEffect">Effect that was added.</param>
        public virtual void OnEffectAdd(Effect addedEffect)
        {

        }

        /// <summary>
        /// Executes when any effect is removed.
        /// </summary>
        /// <param name="removedEffect">Effect that was removed.</param>
        public virtual void OnEffectRemove(Effect removedEffect)
        {

        }

        /// <summary>
        /// Executes when this effect expires.
        /// </summary>
        /// <param name="forced">Whether this effect was forced to expire before timing out normally.</param>
        protected virtual void Expire(bool forced, bool removeAtStart)
        {
            expired = true;
            if (removeAtStart) turnStartAction += () => RemoveFromStack(this);
            else turnEndAction += () => RemoveFromStack(this);
        }

        /// <summary>
        /// Checks if this effect is legal to add to the stack.
        /// </summary>
        /// <returns></returns>
        protected virtual bool Legal()
        {
            throw new Exception("No rule check for " + name + ". Please add rule check.");
        }

        /// <summary>
        /// Gets how many effects of this type are possible to add. Effects themselves may not be added if they conflict with present ones. Cannot use case data and must use all available other data.
        /// </summary>
        /// <returns>Default addable amount of this effect type.</returns>
        public virtual int Max()
        {
            throw new Exception("No max addition calculations for " + name + ". Please add.");
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
        /// If this effect can be added to the queue, adds it, if not the effect is destroyed.
        /// </summary>
        /// <param name="effect">The effect to be added.</param>
        /// <returns>Whether the effect was added.</returns>
        public static bool AddToStack(Effect effect)
        {
            if (!effect.Legal())
            {
                Debug.LogError("Tried to add illegal effect " + effect.name);
                Destroy(effect.gameObject);
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
                affected.OnEffectAdd(effect);
            }

            effect.transform.SetParent(effect.targetedPlayer != null ? effect.targetedPlayer.transform : Battle.main.transform);
            return true;
        }

        /// <summary>
        /// Removes an effect from the queue.
        /// </summary>
        /// <param name="effect">Effect to remove.</param>
        public static void RemoveFromStack(Effect effect)
        {
            foreach (Effect affected in Battle.main.effects)
            {
                affected.OnEffectRemove(effect);
            }

            Battle.main.effects.Remove(effect);

            Destroy(effect.gameObject);
        }
        public static Action turnStartAction;
        public static Action turnEndAction;
    }
}