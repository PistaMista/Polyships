using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BattleUserInterface : InputEnabledUserInterface
{
    public BattleUserInterface[] secondaries;
    public UIAgent[] staticUIAgents = new UIAgent[0];
    public List<UIAgent> dynamicUIAgents = new List<UIAgent>();
    public GameObject[] dynamicUIAgentPalette;
    public Transform UIAgentParent;

    public void SetWorldRendering(bool enabled)
    {
        for (int i = 0; i < secondaries.Length; i++)
        {
            secondaries[i].SetWorldRendering(enabled);
        }
    }

    protected void DestroyDynamicAgents()
    {
        foreach (UIAgent agent in dynamicUIAgents)
        {
            Destroy(agent.gameObject);
        }

        dynamicUIAgents = new List<UIAgent>();
    }

    protected UIAgent CreateDynamicAgent(int paletteNumber)
    {
        UIAgent agent = Instantiate(dynamicUIAgentPalette[paletteNumber]).GetComponent<UIAgent>();
        agent.transform.SetParent(UIAgentParent);
        return agent;
    }

    protected override void Update()
    {
        base.Update();

    }

    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.ENABLING:
                SetInteractable(true);
                break;
            case UIState.ENABLED:
                SetInteractable(true);
                break;
            case UIState.DISABLING:
                SetInteractable(false);
                break;
            case UIState.DISABLED:
                SetInteractable(false);
                break;
        }

        for (int i = 0; i < secondaries.Length; i++)
        {
            secondaries[i].State = state;
        }
    }
}
