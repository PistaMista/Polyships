using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticalTargetingBattleUserInterface : PrimaryBattleUserInterface
{
    public bool isPrimary;
    Player managedAttacker;
    public AttackViewUserInterface attackViewUserInterface;


    public GameObject tokenPrefab;
    public GameObject stackPedestal;
    public Vector3 defaultPedestalPosition;
    public Vector3 targetPedestalPosition;
    protected List<ActionToken> placedTokens;
    protected List<ActionToken> stackTokens;
    protected ActionToken[] allTokens;
    protected ActionToken heldToken;

    public void ResetTargeting() //Resets the targeting completely
    {
        stackPedestal.SetActive(true);
        stackPedestal.transform.position = defaultPedestalPosition;
        for (int i = 0; i < allTokens.Length; i++)
        {
            Destroy(allTokens[i].gameObject);
        }
        allTokens = null;
        heldToken = null;
        AddTokens();
        stackTokens = new List<ActionToken>(allTokens);
        placedTokens = new List<ActionToken>();
    }

    protected virtual void AddTokens() //Spawns in the tokens
    {

    }

    protected override void Update()
    {
        base.Update();

    }

    protected override void ResetWorldSpaceParent() //World space parent is completely negated
    {

    }

    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.ENABLING:
                if (managedAttacker != Battle.main.attacker) //If the last attacker played his turn reset the targeter
                {
                    ResetTargeting();
                    managedAttacker = Battle.main.attacker;
                }
                break;
        }

        stackPedestal.SetActive(state != UIState.DISABLED);
        for (int i = 0; i < allTokens.Length; i++)
        {
            allTokens[i].gameObject.SetActive(state != UIState.DISABLED);
        }
    }
}
