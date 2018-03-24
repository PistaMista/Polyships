using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BattleUIAgents.Base;
using BattleUIAgents.Agents;


namespace BattleUIAgents.UI
{
    public class Attackscreen : ScreenBattleUIAgent
    {
        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            LinkAgent(FindAgent(x => { return x is Flag && x.player != player; }));
        }
    }
}
