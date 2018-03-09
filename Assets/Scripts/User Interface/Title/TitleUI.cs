using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleUI : InputEnabledUI
{
    void Start()
    {
        State = state;
    }
    public TitleSlaveUI[] slaves;
    protected override void SetState(UIState state)
    {
        base.SetState(state);
        for (int i = 0; i < slaves.Length; i++)
        {
            slaves[i].OnTitleSetState(state);
        }

        SetInteractable((int)state >= 2);
    }
}
