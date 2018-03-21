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
        public bool hooked
        {
            get { return ServiceDehooker != null; }
        }
        public BattleAgentServiceDehooker ServiceDehooker;

        protected delegate bool BattleAgentFilterPredicate<T>(BattleUIAgent agent);
        public delegate void BattleAgentServiceDehooker();
        protected Vector3 relativeWorldInputPosition;

        void OnEnable()
        {
            GatherRequiredAgents();
        }

        void OnDisable()
        {
            Unhook();
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


                    confirmedCandidate.SetInteractable(true);
                    confirmedCandidate.GatherRequiredAgents();

                    if (creationMode) confirmedCandidate.ServiceDehooker += () => { Destroy(confirmedCandidate.gameObject, 10.0f); };

                    matches.Add(confirmedCandidate);
                    ServiceDehooker += confirmedCandidate.Unhook;
                }
            }

            return matches.ToArray();
        }

        // protected void DehookFromThis(BattleUIAgent agent)
        // {
        //     BattleAgentDehooker dehooker = dehookers[agent];
        //     if (dehooker != null) dehooker();
        // }

        // protected void DehookFromThis<T>(BattleAgentFilterPredicate<T> condition, int limit)
        // {
        //     List<BattleUIAgent> dehookedAgents = new List<BattleUIAgent>();
        //     foreach (KeyValuePair<BattleUIAgent, BattleAgentDehooker> item in dehookers)
        //     {
        //         if (dehookedAgents.Count >= limit) break;
        //         if (condition(item.Key))
        //         {
        //             item.Value();
        //             dehookedAgents.Add(item.Key);
        //         }
        //     }

        //     dehookedAgents.ForEach((x) => { dehookers.Remove(x); });
        // }
        public void Unhook()
        {
            if (ServiceDehooker != null) ServiceDehooker();
            ServiceDehooker = null;
            WipeStoredValues();

            SetInteractable(false);
        }

        protected virtual void WipeStoredValues()
        {

        }
    }
}