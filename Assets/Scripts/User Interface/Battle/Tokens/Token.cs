using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BattleUIAgents.Base;
using BattleUIAgents.Agents;

using Gameplay;

namespace BattleUIAgents.Tokens
{
    public class Token : WorldBattleUIAgent
    {
        public static Token heldToken;
        public Effect effectType;
        public Effect effect;
        public float pickupRadius;
        public float height;
        public Vector3 initialSpot;
        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            Delinker += () => { if (effect != null) { Effect.RemoveFromQueue(effect); effect = null; }; initialSpot = Vector3.zero; };

            hookedPosition = GetPositionWhenEffectless();
        }

        protected Vector3 GetPositionWhenEffectless()
        {
            float highestPosition = Mathf.NegativeInfinity;
            BattleUIAgent[] highestBlockerCandidates = FindAgents(x =>
            {
                if (x is Token && x.linked)
                {
                    Token token = (Token)x;
                    if (token.effect == null && token.effectType == effectType && token.transform.position.y > highestPosition)
                    {
                        highestPosition = token.transform.position.y;
                        return true;
                    }
                }
                return false;
            }, int.MaxValue
            );

            if (highestBlockerCandidates != null && highestBlockerCandidates.Length > 0)
            {
                Token highestBlocker = (Token)highestBlockerCandidates[highestBlockerCandidates.Length - 1];
                return highestBlocker.transform.position + Vector3.up * height + Vector3.right * 0.5f;
            }
            else
            {
                return initialSpot;
            }
        }

        // protected virtual Effect GetInitialEffect()
        // {
        //     return Battle.main.effects.Find(x =>
        //     {
        //         return FindAgent(y =>
        //         {
        //             if (y is Token)
        //             {
        //                 Token candidate = (Token)y;
        //                 return candidate.effect == x;
        //             }
        //             return false;
        //         }
        //         ) == null;
        //     }
        //     );
        // }

        public bool TryPickup(Vector3 position)
        {
            Vector2 planarPosition = new Vector2(transform.position.x, transform.position.z);
            Vector2 planarInput = new Vector2(position.x, position.z);
            float planarDistance = Vector2.Distance(planarPosition, planarInput);

            if (heldToken == null && planarDistance < pickupRadius)
            {
                if (FindAgent(x =>
                {
                    if (x is Token)
                    {
                        Token c = (Token)x;
                        float planarCandidateDistance = Vector2.Distance(planarInput, new Vector2(c.transform.position.x, c.transform.position.z));
                        return planarCandidateDistance < c.pickupRadius && ((c.transform.position.y > transform.position.y) || (Mathf.Approximately(c.transform.position.y, transform.position.y) && planarCandidateDistance < planarDistance));
                    }

                    return false;
                }) == null)
                {
                    Pickup();
                    return true;
                }
            }

            return false;
        }

        protected virtual void Pickup()
        {
            heldToken = this;
            if (effect != null)
            {
                Effect.RemoveFromQueue(effect);
                effect = null;
            }
        }

        public virtual void ProcessExternalInputWhileHeld(Vector3 inputPosition)
        {

        }

        public virtual void Drop()
        {
            if (effect != null)
            {
                transform.SetAsLastSibling();
                if (!Battle.main.effects.Contains(effect))
                {
                    Effect.AddToQueue(effect);
                }
            }
            else
            {
                transform.SetAsFirstSibling();
                hookedPosition = GetPositionWhenEffectless();
            }
        }

        protected virtual Effect CalculateEffect()
        {
            return null;
        }
    }
}