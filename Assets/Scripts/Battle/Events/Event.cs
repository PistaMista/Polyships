using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class Event : Effect
    {
        [Range(0.00f, 1.00f)]
        public float baseSummoningChance = 0.5f;
        public bool GetSummoningRoll()
        {
            if (GetAdditionalAllowed() > 0)
            {
                return ProcessSummoningRoll();
            }

            return false;
        }
        protected virtual bool ProcessSummoningRoll()
        {
            return Random.Range(0.00f, 1.00f) <= baseSummoningChance;
        }

        protected virtual void OnSummon()
        {

        }

        public static void RandomEventsRoll()
        {
            foreach (Effect effectPrefab in MiscellaneousVariables.it.effectPrefabs)
            {
                if (effectPrefab is Event)
                {
                    Event eventPrefab = effectPrefab as Event;
                    if (eventPrefab.GetSummoningRoll())
                    {
                        Event createdEvent = Effect.CreateEffect(eventPrefab.GetType()) as Event;
                        Effect.AddToQueue(createdEvent);
                        createdEvent.OnSummon();
                    }
                }
            }
        }
    }
}
