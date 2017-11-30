using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BattleUserInterface : InputEnabledUserInterface
{
    public UIAgent[] staticUIAgents = new UIAgent[0];
    public List<UIAgent> dynamicUIAgents = new List<UIAgent>();
    public GameObject[] dynamicUIAgentPaletteObjects;
    public string[] dynamicUIAgentPaletteNames;
    public Transform UIAgentParent;
    public List<UIAgent> allAgents
    {
        get
        {
            List<UIAgent> result = new List<UIAgent>();
            result.AddRange(staticUIAgents);
            result.AddRange(dynamicUIAgents);
            return result;
        }
    }


    protected void DestroyDynamicAgents()
    {
        dynamicUIAgents.ForEach(x => Destroy(x.gameObject));
        dynamicUIAgents = new List<UIAgent>();
    }

    protected UIAgent CreateDynamicAgent(string name)
    {
        UIAgent agent = null;
        for (int i = 0; i < dynamicUIAgentPaletteNames.Length; i++)
        {
            if (name == dynamicUIAgentPaletteNames[i])
            {
                agent = Instantiate(dynamicUIAgentPaletteObjects[i]).GetComponent<UIAgent>();
                if (UIAgentParent != null)
                {
                    agent.transform.SetParent(UIAgentParent);
                    agent.State = State;
                }

                break;
            }
        }
        return agent;
    }

    protected override void Update()
    {
        base.Update();
        if (State == UIState.ENABLING || State == UIState.DISABLING)
        {
            if (!allAgents.Find(x => x.State == State))
            {
                State = State == UIState.ENABLING ? UIState.ENABLED : UIState.DISABLED;
            }
        }
    }

    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        SetInteractable((int)state >= 2);
        allAgents.ForEach(x => { if (x.State != state) { x.State = state; } });
    }
}
