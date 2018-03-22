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

        public static BattleUIAgent[] FindAgents<T>(string nameFilter, Player owner, int limit)
        {
            BattleUIAgent[] examinedArray = owner == null ? MiscellaneousVariables.it.generalBattleAgents.ToArray() : owner.uiAgents;
            List<BattleUIAgent> matches = new List<BattleUIAgent>();

            for (int i = 0; i < examinedArray.Length && matches.Count < limit; i++)
            {
                BattleUIAgent candidate = examinedArray[i];
                bool typeMatch = candidate is T;
                bool nameMatch = candidate.name.Contains(nameFilter);

                if (typeMatch && nameMatch) matches.Add(candidate);
            }

            return matches.ToArray();
        }

        protected BattleUIAgent[] HookToThis<T>(string nameFilter, Player owner, int limit, bool creationMode)
        {
            List<BattleUIAgent> linkingAgents = new List<BattleUIAgent>();

            if (creationMode)
            {
                GameObject battleAgentPrefab = RetrieveDynamicAgentPrefab<T>(nameFilter).gameObject;

                for (int i = 0; i < limit; i++)
                {
                    BattleUIAgent instantiatedAgent = Instantiate(battleAgentPrefab).GetComponent<BattleUIAgent>();
                    instantiatedAgent.ServiceDehooker += () => { Destroy(instantiatedAgent, 10.0f); };
                    linkingAgents.Add(instantiatedAgent);
                }
            }
            else
            {
                linkingAgents.AddRange(FindAgents<T>(nameFilter, owner, limit));
            }

            foreach (BattleUIAgent agent in linkingAgents)
            {
                agent.SetInteractable(true);
                agent.GatherRequiredAgents();

                agent.ServiceDehooker += () => { Debug.Log("Dehooked Battle Agent: " + agent.name); };
                ServiceDehooker += agent.Unhook;
            }

            return linkingAgents.ToArray();
        }

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