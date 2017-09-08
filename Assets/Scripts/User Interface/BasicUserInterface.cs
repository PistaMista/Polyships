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

    public virtual void AEnable()
    {
        gameObject.SetActive(true);
        state = UIState.ENABLING;
    }

    public virtual void ADisable()
    {
        //gameObject.SetActive(false);
        state = UIState.DISABLING;
    }

    protected virtual void Update()
    {

    }
}
