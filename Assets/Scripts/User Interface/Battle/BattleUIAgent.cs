using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BattleUIAgents;
using Gameplay;

namespace BattleUIAgents.Base
{
    public class BattleUIAgent : InputEnabledUI
    {
        public delegate bool BattleAgentFilterPredicate(BattleUIAgent agent);
        public delegate void AgentDelinker();
        [Header("Battle Agent Configuration")]
        public Player player;
        public bool linked
        {
            get { return delinker != null; }
        }
        /// <summary>
        /// DO NOT MODIFY. Delinks everything linked to this agent.
        /// </summary>
        AgentDelinker delinker;
        /// <summary>
        /// DO NOT MODIFY. Terminates the delinker and resets the agent.
        /// </summary>
        AgentDelinker delinkerTerminator;
        /// <summary>
        /// Terminates all references this object holds to others and all references others hold to it. 
        /// </summary>
        public AgentDelinker Delinker
        {
            get
            {
                return delinker + delinkerTerminator;
            }
            set
            {
                delinker = value - delinkerTerminator;
            }
        }
        protected Vector3 relativeWorldInputPosition;

        void OnEnable()
        {
            PerformLinkageOperations();
        }

        void OnDisable()
        {
            Delinker();
        }

        protected override void ProcessInput()
        {
            base.ProcessInput();
            relativeWorldInputPosition = transform.InverseTransformPoint(currentInputPosition.world);
        }

        /// <summary>
        /// Gets the agent ready for use and links any required agents. Linking is only allowed in this method.
        /// </summary>
        protected virtual void PerformLinkageOperations()
        {
            if (delinkerTerminator == null) delinkerTerminator = (() =>
                 {
                     delinker = null;
                     Reset();
                 });
            SetInteractable(true);
        }

        public static BattleUIAgent FindAgent(BattleAgentFilterPredicate predicate)
        {
            return FindAgents(predicate, 1)[0];
        }
        public static BattleUIAgent[] FindAgents(BattleAgentFilterPredicate predicate, int limit)
        {
            List<BattleUIAgent> matches = new List<BattleUIAgent>();

            foreach (BattleUIAgent candidate in Resources.FindObjectsOfTypeAll(typeof(BattleUIAgent)))
            {
                if (predicate(candidate)) matches.Add(candidate);
                if (matches.Count == limit) break;
            }

            return matches.ToArray();
        }
        protected BattleUIAgent LinkAgent(BattleUIAgent agent)
        {
            return LinkAgents(new BattleUIAgent[] { agent })[0];
        }
        protected BattleUIAgent[] LinkAgents(BattleUIAgent[] agents)
        {
            for (int i = 0; i < agents.Length; i++)
            {
                BattleUIAgent agent = agents[i];
                agent.PerformLinkageOperations(); //Inform the agent that he is being linked

                AgentDelinker agentDelinkingSubscription = () => { agent.Delinker(); };

                agent.Delinker += () => { Delinker -= agentDelinkingSubscription; }; //Add the following to the its delinker - 1. Delink the agent's delinker from the delinker of the agent linking it 2. Reset the agent's delinker 3. Reset the agent
                Delinker += agentDelinkingSubscription;
            }

            return agents;
        }
        protected BattleUIAgent CreateAndLinkAgent<T>(string nameFilter)
        {
            return CreateAndLinkAgents<T>(nameFilter, 1)[0];
        }
        protected BattleUIAgent[] CreateAndLinkAgents<T>(string nameFilter, int limit)
        {
            GameObject battleAgentPrefab = RetrieveDynamicAgentPrefab<T>(nameFilter).gameObject; //Find the object to create
            List<BattleUIAgent> createdAgents = new List<BattleUIAgent>();

            for (int i = 0; i < limit; i++)
            {
                BattleUIAgent instantiatedAgent = Instantiate(battleAgentPrefab).GetComponent<BattleUIAgent>(); //Create the object
                instantiatedAgent.Delinker += () => { Destroy(instantiatedAgent.gameObject, 3.0f); }; //Make its delinker destroy the object
                createdAgents.Add(instantiatedAgent);
            }

            return LinkAgents(createdAgents.ToArray());
        }

        /// <summary>
        /// Resets the agent and gets it ready for another linking cycle.
        /// </summary>
        protected virtual void Reset()
        {

        }
    }
}