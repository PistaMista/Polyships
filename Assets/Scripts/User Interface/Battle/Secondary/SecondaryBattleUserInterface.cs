using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondaryBattleUserInterface : BasicUserInterface
{
    public Transform worldSpaceParent;

    void Awake()
    {
        ResetWorldSpaceParent();
    }

    public void SetWorldSpaceParent(bool enabled)
    {
        worldSpaceParent.gameObject.SetActive(enabled);
    }

    protected virtual void ResetWorldSpaceParent()
    {
        if (worldSpaceParent != null)
        {
            Destroy(worldSpaceParent.gameObject);
        }
        worldSpaceParent = new GameObject("World Space Parent").transform;
        worldSpaceParent.SetParent(transform);
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
    }
}
