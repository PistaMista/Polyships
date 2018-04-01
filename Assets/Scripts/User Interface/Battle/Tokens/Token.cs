using System.Collections;
using System.Collections.Generic;
using System;
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
        public float occlusionRadius;
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
                if (x.linked)
                {
                    Token token = (Token)x;
                    if (token != this && token.effect == null && token.effectType == effectType && token.hookedPosition.y > highestPosition)
                    {
                        highestPosition = token.hookedPosition.y;
                        return true;
                    }
                }
                return false;
            }, typeof(Token), int.MaxValue
            );

            if (highestBlockerCandidates != null && highestBlockerCandidates.Length > 0)
            {
                Token highestBlocker = (Token)highestBlockerCandidates[highestBlockerCandidates.Length - 1];
                return highestBlocker.hookedPosition + Vector3.up * height + Vector3.right * 0.5f;
            }
            else
            {
                return initialSpot;
            }
        }

        public bool TryPickup(Vector3 position)
        {
            if (!interactable) return false;

            Vector2 planarInput = new Vector2(position.x, position.z);

            Utilities.PerspectiveProjection scaleInfo = Utilities.GetPositionOnElevationFromPerspective(hookedPosition, Camera.main.transform.position, MiscellaneousVariables.it.boardUIRenderHeight);
            float planarDistance = Vector2.Distance(scaleInfo.planarPosition, planarInput);

            if (heldToken == null && planarDistance < pickupRadius * scaleInfo.scalar)
            {
                if (FindAgent(x =>
                {
                    Token c = (Token)x;
                    Utilities.PerspectiveProjection candidateScaleInfo = Utilities.GetPositionOnElevationFromPerspective(c.hookedPosition, Camera.main.transform.position, MiscellaneousVariables.it.boardUIRenderHeight);
                    float planarCandidateDistance = Vector2.Distance(planarInput, candidateScaleInfo.planarPosition);
                    return planarCandidateDistance < c.occlusionRadius * scaleInfo.scalar && ((c.transform.position.y > transform.position.y) || (Mathf.Approximately(c.transform.position.y, transform.position.y) && planarCandidateDistance < planarDistance));
                }, typeof(Token)) == null)
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
            heldToken = null;

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

        public static Token[] FindTokens(bool linked, bool used, Type effectType, int limit)
        {
            return Array.ConvertAll(FindAgents(x =>
            {
                Token token = (Token)x;
                return token.linked == linked && (token.effect != null) == used && token.effectType.GetType() == effectType;
            }, typeof(Token), limit), x => { return (Token)x; });
        }
    }
}