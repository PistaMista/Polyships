using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSlaveUI : InputEnabledUI
{
    public UIAgent nextUI;
    public virtual void OnTitleSetState(UIState state)
    {

    }

    public void Next()
    {
        State = UIState.DISABLED;
        nextUI.State = UIState.ENABLING;
    }
}
