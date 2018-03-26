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
        public Effect effectPrefab;
        public Effect boundEffect;
        public float pickupRadius;
        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            Delinker += () => { if (boundEffect != null) { Effect.RemoveFromQueue(boundEffect); boundEffect = null; }; };
            boundEffect = GetInitialBoundEffect();
        }

        protected virtual Effect GetInitialBoundEffect()
        {
            return Battle.main.effects.Find(x =>
            {
                return FindAgent(y =>
                {
                    if (y is Token)
                    {
                        Token candidate = (Token)y;
                        return candidate.boundEffect == x;
                    }
                    return false;
                }
                ) == null;
            });
        }

        public bool TryPickup(Vector3 position)
        {
            Vector2 planarPosition = new Vector2(transform.position.x, transform.position.z);
            Vector2 planarInput = new Vector2(position.x, position.z);

            if (heldToken == null && Vector2.Distance(planarPosition, planarInput) < pickupRadius)
            {
                if (FindAgent(x =>
                {
                    if (x is Token)
                    {
                        Token c = (Token)x;
                        return c.transform.position.y > transform.position.y && Vector2.Distance(planarInput, new Vector2(c.transform.position.x, c.transform.position.z)) < c.pickupRadius;
                    }

                    return false;
                }) == null)
                {
                    Pickup();
                    return true;
                }
                return false;
            }

            return false;
        }

        protected virtual void Pickup()
        {
            heldToken = this;
            if (boundEffect != null)
            {
                Effect.RemoveFromQueue(boundEffect);
                boundEffect = null;
            }
        }

        protected override void Update()
        {
            base.Update();
            if (heldToken == this)
            {
                boundEffect = CalculateEffect();
            }
        }

        public virtual void Drop()
        {
            if (boundEffect != null)
            {
                transform.SetAsLastSibling();
                if (!Battle.main.effects.Contains(boundEffect))
                {
                    Effect.AddToQueue(boundEffect);
                }
            }
            else
            {
                transform.SetAsFirstSibling();
            }
        }

        protected virtual Effect CalculateEffect()
        {
            return null;
        }
    }
}