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
        public float pickupRadius
        {
            get
            {
                return effect != null ? 2.0f : 0.5f;
            }
        }
        public float occlusionRadius;
        public float height;
        bool stacked;
        public struct Stacking
        {
            public Vector3 stackStart;
            public Vector3 stackStep;
            public Stacking(Vector3 stackStart, Vector3 stackStep)
            {
                this.stackStart = stackStart;
                this.stackStep = stackStep;
            }
        }
        Stacking stacking;

        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            float initialMaxInteractableVelocity = maximumInteractableVelocity;
            Delinker += () => { stacked = false; effect = null; maximumInteractableVelocity = initialMaxInteractableVelocity; };
            if (effect == null)
            {
                PutOnStack();
            }
            else
            {
                RefreshEffectRepresentation();
            }
        }

        /// <summary>
        /// Connects the token with any effect of the same type in the battle.
        /// </summary>
        /// <returns>Connection successful.</returns>
        public virtual bool ConnectWithAnyCompatibleEffect()
        {
            foreach (Effect effect in Battle.main.effects)
            {
                if (effect.GetType() == effectType.GetType() && (effect.visibleTo == null || effect.visibleTo == Battle.main.attacker))
                {
                    if (FindAgent(x => { return (x as Token).effect == effect; }, typeof(Token)) == null)
                    {
                        this.effect = effect;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the position of this token when its stacked.
        /// </summary>
        /// <returns>Position on the stack.</returns>
        protected Vector3 GetPositionOnStack()
        {
            int blockers = FindAgents(x =>
            {
                if (x.linked)
                {
                    Token token = (Token)x;
                    return token != this && token.stacked && ((token.effect == null && token.effectType == effectType) || (this is EventToken && x is EventToken));
                }
                return false;
            }, typeof(Token), int.MaxValue
            ).Length;

            return stacking.stackStart + stacking.stackStep * blockers;
        }

        /// <summary>
        /// Checks whether this position activates the token.
        /// </summary>
        /// <param name="position">Position.</param>
        /// <returns>Will activate.</returns>
        public bool IsPositionActivating(Vector3 position)
        {
            if (!interactable && !(this is EventToken)) return false;

            Vector2 planarInput = new Vector2(position.x, position.z);

            Utilities.PerspectiveProjection scaleInfo = Utilities.GetPositionOnElevationFromPerspective(hookedPosition, Camera.main.transform.position, MiscellaneousVariables.it.boardUIRenderHeight);
            float planarDistance = Vector2.Distance(scaleInfo.planarPosition, planarInput);
            bool hasEffect = effect != null;

            if (heldToken == null && planarDistance < pickupRadius * scaleInfo.scalar)
            {
                if (FindAgent(x =>
                {
                    Token blocker = x as Token;
                    bool blockerHasEffect = blocker.effect != null;
                    Utilities.PerspectiveProjection candidateScaleInfo = Utilities.GetPositionOnElevationFromPerspective(blocker.hookedPosition, Camera.main.transform.position, MiscellaneousVariables.it.boardUIRenderHeight);
                    float blockerPlanarDistance = Vector2.Distance(planarInput, candidateScaleInfo.planarPosition);
                    //Block pickup if a token is: LINKED AND ((BOTH DONT HAVE AN EFFECT OR BOTH DO) AND (CLOSER TO INPUT OR (OF SAME TYPE AND ABOVE AND BOTH DONT HAVE AN EFFECT))) 
                    return blocker.linked && ((blockerHasEffect == hasEffect) && (blockerPlanarDistance < planarDistance || (GetType() == blocker.GetType() && blocker.hookedPosition.y > hookedPosition.y && !hasEffect)));
                }, typeof(Token)) == null)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Picks up the token.
        /// </summary>
        public virtual void Pickup()
        {
            stacked = false;
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

        protected List<Highlighterline> usageHighlighters = new List<Highlighterline>();
        /// <summary>
        /// Drops the token and applies any effect it has attached.
        /// </summary>
        public virtual void Drop()
        {
            heldToken = null;
            usageHighlighters.ForEach(x => { x.Delinker(); });
            usageHighlighters = new List<Highlighterline>();

            if (effect != null)
            {
                if (Effect.AddToQueue(effect))
                {
                    transform.SetAsLastSibling();
                    return;
                }
                else
                {
                    Destroy(effect.gameObject);
                    effect = null;
                }
            }

            transform.SetAsFirstSibling();
            PutOnStack();
        }

        /// <summary>
        /// Puts the token on the stack.
        /// </summary>
        public void PutOnStack()
        {
            hookedPosition = GetPositionOnStack();
            stacked = true;
        }


        /// <summary>
        /// Updates the graphical representation of the current effect.
        /// </summary>
        protected virtual void RefreshEffectRepresentation()
        {

        }

        /// <summary>
        /// Finds tokens based on:
        /// </summary>
        /// <param name="linked">Whether they are linked.</param>
        /// <param name="used">Whether they are used.</param>
        /// <param name="effectType">If their effect type is this.</param>
        /// <param name="limit">Limits the amount of results possible.</param>
        /// <returns></returns>
        public static Token[] FindTokens(bool linked, bool used, Type effectType, int limit)
        {
            return Array.ConvertAll(FindAgents(x =>
            {
                Token token = x as Token;
                return token.linked == linked && (token.effect != null) == used && token.effectType.GetType() == effectType;
            }, typeof(Token), limit), x => { return x as Token; });
        }

        /// <summary>
        /// Sets the stacking method for a given type.
        /// </summary>
        /// <param name="tokenEffectType">The type stack to change.</param>
        /// <param name="stackStart">Where the stack starts.</param>
        /// <param name="stackStep">Where any additional tokens get stacked. (relative to the start)</param>
        public static void SetTypeStacking(Type tokenEffectType, Vector3 stackStart, Vector3 stackStep)
        {
            Token[] tokens = Array.ConvertAll(FindAgents(x =>
            {
                Token token = x as Token;
                return token.effectType.GetType() == tokenEffectType || (tokenEffectType == typeof(Gameplay.Event) && token.effectType is Gameplay.Event);
            }, typeof(Token), int.MaxValue), x => { return x as Token; });

            Array.ForEach(tokens, x => { x.stacked = false; });
            Array.ForEach(tokens, x => { x.stacking = new Stacking(stackStart, stackStep); x.stacking.stackStep.y = x.height; });
        }
    }
}