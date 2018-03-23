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
        /// Terminates all references this object holds to others and all references others hold to it. 
        /// </summary>
        private AgentDelinker delinker;
        public AgentDelinker Delinker
        {
            get
            {
                return delinker + (() =>
                {
                    delinker = null;
                    Reset();
                    Debug.Log(name);
                });
            }
            set
            {
                delinker = value;
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

                agent.Delinker += () => { Delinker -= () => { agent.Delinker(); }; }; //Add the following to the its delinker - 1. Delink the agent's delinker from the delinker of the agent linking it 2. Reset the agent's delinker 3. Reset the agent
                Delinker += () => { agent.Delinker(); };
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
                instantiatedAgent.Delinker += () => { Destroy(instantiatedAgent.gameObject, 10.0f); }; //Make its delinker destroy the object
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

        // public void Delink()
        // {
        //     if (delinker != null)
        //     {
        //         delinker();
        //         delinker = null;
        //         Reset();
        //         Debug.Log("Delinked " + name);
        //     }
        // }

        // public void RegisterAdditionalDelinker(AgentDelinker delinker)
        // {
        //     this.delinker += delinker;
        // }
    }
}