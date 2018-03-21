using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleUIAgents;
using Gameplay;

namespace BattleUIAgents.Base
{
    public class BattleUIAgent : InputEnabledUI
    {
        [Header("Battle Agent Configuration")]
        public Player player;
        public BattleUIAgent hookedTo;
        public BattleAgentDehooker dehooker;
        public List<BattleUIAgent> hookedAgents;

        protected delegate bool BattleAgentFilterPredicate<T>(BattleUIAgent agent);
        public delegate void BattleAgentDehooker();
        Dictionary<BattleUIAgent, BattleAgentDehooker> dehookers = new Dictionary<BattleUIAgent, BattleAgentDehooker>();
        protected Vector3 relativeWorldInputPosition;

        void OnEnable()
        {
            GatherRequiredAgents();
        }

        void OnDisable()
        {
            foreach (KeyValuePair<BattleUIAgent, BattleAgentDehooker> item in dehookers)
            {
                item.Value();
            }

            dehookers = new Dictionary<BattleUIAgent, BattleAgentDehooker>();
        }

        protected override void ProcessInput()
        {
            base.ProcessInput();
            relativeWorldInputPosition = transform.InverseTransformPoint(currentInputPosition.world);
        }

        protected virtual void GatherRequiredAgents()
        {

        }
        protected BattleUIAgent[] HookToThis<T>(string nameFilter, Player owner, bool creationMode, int limit)
        {
            return HookToThis<T>(nameFilter, owner, creationMode, limit, () => { });
        }
        protected BattleUIAgent[] HookToThis<T>(string nameFilter, Player owner, bool creationMode, int limit, BattleAgentDehooker dehookingExtension)
        {
            BattleUIAgent[] examinedArray = owner == null ? MiscellaneousVariables.it.generalBattleAgents.ToArray() : owner.uiAgents;

            int cycles = creationMode ? limit : Mathf.Min(examinedArray.Length, limit);
            List<BattleUIAgent> matches = new List<BattleUIAgent>();

            UIAgent candidate = creationMode ? RetrieveDynamicAgentPrefab<T>(nameFilter) : examinedArray[0];
            bool typeMatch = candidate is T && candidate is BattleUIAgent;
            bool nameMatch = candidate.name.Contains(nameFilter);

            for (int i = 0; i < cycles; i++)
            {
                if (!creationMode)
                {
                    candidate = examinedArray[i];
                    typeMatch = candidate is T && candidate is BattleUIAgent;
                    nameMatch = candidate.name.Contains(nameFilter);
                }

                if (typeMatch && nameMatch)
                {
                    BattleUIAgent confirmedCandidate = (BattleUIAgent)candidate;
                    if (creationMode) confirmedCandidate = Instantiate(confirmedCandidate).GetComponent<BattleUIAgent>();

                    confirmedCandidate.hookedTo = this;
                    confirmedCandidate.OnHook();

                    BattleAgentDehooker dehooker = () => { confirmedCandidate.hookedTo = null; confirmedCandidate.OnUnhook(); };
                    if (creationMode) dehooker += () => { Destroy(confirmedCandidate.gameObject, 10.0f); };

                    dehooker += dehookingExtension;

                    dehookers.Add(confirmedCandidate, dehooker);

                    matches.Add(confirmedCandidate);
                }
            }

            return matches.ToArray();
        }

        protected void DehookFromThis(BattleUIAgent agent)
        {
            BattleAgentDehooker dehooker = dehookers[agent];
            if (dehooker != null) dehooker();
        }

        protected void DehookFromThis<T>(BattleAgentFilterPredicate<T> condition, int limit)
        {
            List<BattleUIAgent> dehookedAgents = new List<BattleUIAgent>();
            foreach (KeyValuePair<BattleUIAgent, BattleAgentDehooker> item in dehookers)
            {
                if (dehookedAgents.Count >= limit) break;
                if (condition(item.Key))
                {
                    item.Value();
                    dehookedAgents.Add(item.Key);
                }
            }

            dehookedAgents.ForEach((x) => { dehookers.Remove(x); });
        }

        public virtual void OnHook()
        {
            SetInteractable(true);
            GatherRequiredAgents();
        }

        public virtual void OnUnhook()
        {
            SetInteractable(false);
        }
    }
}