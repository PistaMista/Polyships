using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UIState
{
    DISABLED,
    DISABLING,
    ENABLING,
    ENABLED
}

public class BasicUI : UIAgent
{
    public RectTransform rect;
    public bool enableOnStart;

    protected virtual void Start()
    {
        if (enableOnStart)
        {
            State = UIState.ENABLED;
        }
    }
}
