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

public class BasicUserInterface : MonoBehaviour
{
    public UIState state;

    protected virtual void AEnable ()
    {
        gameObject.SetActive( true );
        state = UIState.ENABLED;
    }

    protected virtual void ADisable ()
    {
        gameObject.SetActive( false );
        state = UIState.DISABLED;
    }

    protected virtual void Update ()
    {

    }
}
