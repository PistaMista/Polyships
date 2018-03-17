using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class UIAgent : MonoBehaviour
{
    public UIState state;
    public bool parentStateIndependent = false;
    public UIAgent[] remoteAgents = new UIAgent[0]; //These will be informed when the state of this ui changes, but will not be waited for when disabling/enabling this UI.
    public UIAgent[] staticAgents = new UIAgent[0];
    public List<UIAgent> dynamicAgents = new List<UIAgent>();
    public GameObject[] dynamicAgentPrefabs;
    public Transform dynamicAgentParent;
    public List<UIAgent> allAgents
    {
        get
        {
            List<UIAgent> result = new List<UIAgent>();
            result.AddRange(staticAgents);
            result.AddRange(dynamicAgents);
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
        foreach (UIAgent agent in dynamicAgents)
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

        removedAgents.ForEach(x => dynamicAgents.Remove(x));
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

        dynamicAgents.Remove(agent);
    }

    protected UIAgent CreateDynamicAgent<T>(string nameFilter)
    {
        UIAgent result = null;
        UIAgent candidate = RetrieveDynamicAgentPrefab<T>(nameFilter);

        if (candidate != null)
        {
            result = Instantiate(candidate.gameObject).GetComponent<UIAgent>();
        }

        return result;
    }

    protected UIAgent RetrieveDynamicAgentPrefab<T>(string nameFilter)
    {
        UIAgent prefab = null;
        for (int i = 0; i < dynamicAgentPrefabs.Length; i++)
        {
            UIAgent candidate = dynamicAgentPrefabs[i].GetComponent<UIAgent>();
            bool typeMatch = candidate is T;
            bool nameMatch = candidate.name.Contains(nameFilter);

            if (typeMatch && nameMatch)
            {
                prefab = candidate;
                break;
            }
        }

        return prefab;
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
            for (int i = 0; i < remoteAgents.Length; i++)
            {
                UIAgent x = remoteAgents[i];
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
