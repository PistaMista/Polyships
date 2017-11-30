using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAgent : MonoBehaviour
{
    public UIState state;
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

    protected virtual void Update()
    {

    }

    protected virtual void ChangeState(UIState state)
    {
        this.state = state;
        gameObject.SetActive(state != UIState.DISABLED);
    }
}
