using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BattleUIAgents.Base;

namespace BattleUIAgents.Agents
{
    public class Firebutton : WorldBattleUIAgent
    {
        public WorldBattleUIAgent buttonPart;
        public float buttonPartDepression;
        public float pushRadius;
        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            LinkAgent(buttonPart, false);
        }
        public bool TryPush(Vector3 position)
        {
            if (Vector3.Distance(position, transform.position) < pushRadius)
            {
                buttonPart.hookedPosition = buttonPart.unhookedPosition - Vector3.up * buttonPartDepression;
                return true;
            }
            else
            {
                buttonPart.hookedPosition = buttonPart.unhookedPosition;
                return false;
            }
        }
    }
}
