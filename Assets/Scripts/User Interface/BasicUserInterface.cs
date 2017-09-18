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
    public RectTransform rect;
    public bool enableOnStart;
    public static Vector2 referenceResolution;

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
    protected virtual void Start()
    {
        if (enableOnStart)
        {
            State = UIState.ENABLED;
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
