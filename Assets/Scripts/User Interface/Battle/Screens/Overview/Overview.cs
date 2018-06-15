using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using BattleUIAgents.Agents;
using BattleUIAgents.Base;
using Gameplay;

namespace BattleUIAgents.UI
{
    public class Overview : ScreenBattleUIAgent
    {
        Flag[] flags;
        public bool enterAttackScreenOnLink;
        public float autoAttackScreenTime;
        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            flags = Array.ConvertAll(LinkAgents(FindAgents(x => { return x.player != null; }, typeof(Flag), 2), true), item => { return (Flag)item; });
            Delinker += () => { enterAttackScreenOnLink = false; CancelInvoke("GoToAttack"); };

            if (enterAttackScreenOnLink) Invoke("GoToAttack", autoAttackScreenTime);
        }

        protected override void ProcessInput()
        {
            base.ProcessInput();
            if (tap)
            {
                for (int i = 0; i < flags.Length; i++)
                {
                    Flag flag = flags[i];
                    if (flag.IsPositionOnFlag(currentInputPosition.world))
                    {
                        gameObject.SetActive(false);
                        FindAgent(x => { return x.player == flag.player; }, flag.player == Battle.main.attacker ? typeof(Fleetinfo) : typeof(Attackscreen)).gameObject.SetActive(true);
                        break;
                    }
                }
            }
        }

        protected override float CalculateConversionDistance()
        {
            return Camera.main.transform.position.y - MiscellaneousVariables.it.boardUIRenderHeight;
        }

        protected override Vector2 GetFrameSize()
        {
            return base.GetFrameSize() + MiscellaneousVariables.it.boardDistanceFromCenter * Vector2.right * 2.0f;
        }

        void GoToAttack()
        {
            gameObject.SetActive(false);
            FindAgent(x => { return x.player == Battle.main.defender; }, typeof(Attackscreen)).gameObject.SetActive(true);
        }

        protected override void Reset()
        {
            flags = null;
        }
    }
}