using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class Event : Effect
    {
        [Range(0.00f, 1.00f)]
        public float baseSummoningChance = 0.5f;
        protected virtual bool IsTriggered()
        {
            return Random.Range(0.00f, 1.00f) <= baseSummoningChance;
        }

        protected virtual void SetupEvent()
        {

        }

        public static void ConsiderEvents()
        {
            foreach (Effect effectPrefab in MiscellaneousVariables.it.effectPrefabs)
            {
                if (effectPrefab is Event)
                {
                    Event eventPrefab = effectPrefab as Event;
                    if (eventPrefab.IsTriggered())
                    {
                        Event createdEvent = Effect.CreateEffect(eventPrefab.GetType()) as Event;
                        createdEvent.SetupEvent();

                        if (createdEvent.CanBeAddedIntoQueue())
                        {
                            Effect.AddToQueue(createdEvent);
                        }
                        else
                        {
                            Destroy(createdEvent.gameObject);
                        }
                    }
                }
            }
        }
    }
}
