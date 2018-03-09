using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UIAgent : MonoBehaviour
{
    public UIState state;
    public bool parentStateIndependent = false;
    public UIAgent[] remoteUIAgents = new UIAgent[0]; //These will be informed when the state of this ui changes, but will not be waited for when disabling/enabling this UI.
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
    protected UIAgent[] RemoveDynamicAgents<T>(string nameFilter, bool instant)
    {
        List<UIAgent> removedAgents = new List<UIAgent>();
        foreach (UIAgent agent in dynamicUIAgents)
        {
            bool typeMatch = agent is T;
            bool nameMatch = agent.name.Contains(nameFilter);
            if (typeMatch && nameMatch)
            {
                if (instant)
                {
                    Destroy(agent.gameObject);
                }
                else
                {
                    agent.State = UIState.DISABLING;
                }

                removedAgents.Add(agent);
            }
        }

        removedAgents.ForEach(x => dynamicUIAgents.Remove(x));
        return removedAgents.ToArray();
    }

    protected void RemoveDynamicAgent(UIAgent agent, bool instant)
    {
        if (instant)
        {
            Destroy(agent.gameObject);
        }
        else
        {
            agent.State = UIState.DISABLING;
        }

        dynamicUIAgents.Remove(agent);
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
            allAgents.ForEach(x => { if (x.State != state && !x.parentStateIndependent) { x.State = state; } });
            for (int i = 0; i < remoteUIAgents.Length; i++)
            {
                UIAgent x = remoteUIAgents[i];
                if (x.State != state)
                {
                    x.State = state;
                }
            }
        }

        gameObject.SetActive(state != UIState.DISABLED);
    }

    public void SetState(int state)
    {
        SetState((UIState)state);
    }
}
