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
            SetState(value);
        }
    }
    protected void DestroyDynamicAgents<T>(string nameFilter)
    {
        dynamicUIAgents.ForEach(x => Destroy(x.gameObject));
        foreach (UIAgent agent in dynamicUIAgents)
        {
            if (ReferenceEquals(agent, typeof(T)))
            {
                if (agent.name.Contains(nameFilter))
                {
                    Destroy(agent.gameObject);
                }
            }
        }
        dynamicUIAgents = new List<UIAgent>();
    }

    protected void DestroyDynamicAgent(UIAgent agent)
    {
        dynamicUIAgents.Remove(agent);
        Destroy(agent.gameObject);
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
            List<UIAgent> agents = allAgents;
            if (agents.Count > 0)
            {
                if (!allAgents.Exists(x => x.State == State))
                {
                    changeStateCausedByUpdate = true;
                    State = State == UIState.ENABLING ? UIState.ENABLED : UIState.DISABLED;
                    changeStateCausedByUpdate = false;
                }
            }
        }
    }

    protected virtual void SetState(UIState state)
    {
        this.state = state;
        if (!changeStateCausedByUpdate)
        {
            allAgents.ForEach(x => { if (x.State != state) { x.State = state; } });
        }
        gameObject.SetActive(state != UIState.DISABLED);
    }
}
