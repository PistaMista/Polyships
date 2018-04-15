using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BattleUIAgents.Base;
using BattleUIAgents.Agents;
using BattleUIAgents.Tokens;

using UnityEngine.UI;

namespace BattleUIAgents.UI
{
    public class TokenHinter : ScreenBattleUIAgent
    {
        public Token token;
        public Text text;
        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            LinkAgents(FindAgents(x => { return x.name.Contains(name); }, typeof(Graphicfader), 2), true);

            Delinker += () => { SetInteractable(false); };
        }

        protected override void ProcessInput()
        {
            base.ProcessInput();
            if (tap)
            {
                gameObject.SetActive(false);
            }
        }

        protected override Vector3 GetPosition()
        {
            return base.GetPosition() + token.transform.position;
        }
    }
}
