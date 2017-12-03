using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAgent : MonoBehaviour
{
    public UIState state;
    public UIAgent[] staticUIAgents = new UIAgent[0];
    public List<UIAgent> dynamicUIAgents = new List<UIAgent>();
    public GameObject[] dynamicUIAgentPaletteObjects;
    public string[] dynamicUIAgentPaletteNames;
    public Transform childAgentDefaultParent;
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
    public UIState State
    {
        get
        {
            return state;
        }
        set
        {
            ChangeState(value);
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
                if (childAgentDefaultParent != null)
                {
                    agent.transform.SetParent(childAgentDefaultParent);
                    agent.State = State;
                }

                dynamicUIAgents.Add(agent);
                break;
            }
        }
        return agent;
    }

    bool changeStateCausedByUpdate;
    protected virtual void Update()
    {
        if (State == UIState.ENABLING || State == UIState.DISABLING)
        {
            if (!allAgents.Exists(x => x.State == State))
            {
                changeStateCausedByUpdate = true;
                State = State == UIState.ENABLING ? UIState.ENABLED : UIState.DISABLED;
                changeStateCausedByUpdate = false;
            }
        }
    }

    protected virtual void ChangeState(UIState state)
    {
        this.state = state;
        if (!changeStateCausedByUpdate)
        {
            allAgents.ForEach(x => { if (x.State != state) { x.State = state; } });
        }
        gameObject.SetActive(state != UIState.DISABLED);
    }
}
