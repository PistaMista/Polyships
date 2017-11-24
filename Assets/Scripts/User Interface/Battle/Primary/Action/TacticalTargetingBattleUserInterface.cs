using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticalTargetingBattleUserInterface : BoardViewUserInterface
{
    public bool isPrimary;
    Player managedAttacker;
    public AttackViewUserInterface attackViewUserInterface;


    public GameObject tokenPrefab;
    public GameObject stackPedestal;
    public float stackPedestalHeight;
    public float stackMaximumDeviation;
    public Vector3 defaultPedestalPosition;
    public Vector3 stowedPedestalPosition;
    public Vector3 targetPedestalPosition;
    protected List<ActionToken> placedTokens;
    protected List<ActionToken> stackTokens;
    protected ActionToken[] allTokens;
    protected ActionToken heldToken;

    public void ResetTargeting() //Resets the targeting completely
    {
        stackPedestal.SetActive(true);
        stackPedestal.transform.position = defaultPedestalPosition;
        if (allTokens != null)
        {
            for (int i = 0; i < allTokens.Length; i++)
            {
                Destroy(allTokens[i].gameObject);
            }
        }
        allTokens = null;
        heldToken = null;
        AddTokens();
        stackTokens = new List<ActionToken>(allTokens);
        placedTokens = new List<ActionToken>();
    }

    protected void AddTokens() //Spawns in the tokens
    {
        int tokenCount = GetInitialTokenCount();
        allTokens = new ActionToken[tokenCount];

        Vector3 startingPosition = defaultPedestalPosition + Vector3.up * (stackPedestalHeight / 2.0f + tokenPrefab.GetComponent<ActionToken>().stackHeight / 2.0f);
        for (int i = 0; i < tokenCount; i++)
        {
            ActionToken token = Instantiate(tokenPrefab).GetComponent<ActionToken>();
            token.owner = this;
            token.OnPedestal = true;


            Vector3 globalPosition = startingPosition + Vector3.up * token.stackHeight * i + new Vector3(Random.Range(-stackMaximumDeviation, stackMaximumDeviation), 0, Random.Range(-stackMaximumDeviation, stackMaximumDeviation));
            token.defaultPositionRelativeToPedestal = stackPedestal.transform.InverseTransformPoint(globalPosition);
            token.transform.localPosition = token.defaultPositionRelativeToPedestal;
            allTokens[i] = token;
        }
    }

    protected virtual int GetInitialTokenCount()
    {
        return 0;
    }

    protected virtual void PickupToken(ActionToken token)
    {
        token.value = null;
        if (token.OnPedestal)
        {
            Debug.Log("Picked up token from pedestal");
            stackTokens.Remove(token);
        }
        else
        {
            Debug.Log("Picked up token from the field");
            placedTokens.Remove(token);
        }
        attackViewUserInterface.selectedTargeter = this;

        token.OnPedestal = false;
        heldToken = token;
    }

    protected virtual void DropHeldToken()
    {
        CalculateTokenValue(heldToken);
        if (heldToken.value == null)
        {
            stackTokens.Add(heldToken);
            Debug.Log("Dropped token onto pedestal");
            heldToken.OnPedestal = true;
        }
        else
        {
            placedTokens.Add(heldToken);
            Debug.Log("Dropped token onto the field");
            heldToken.OnPedestal = false;
        }

        attackViewUserInterface.selectedTargeter = null;
    }

    protected virtual Vector3 CalculateHeldTokenTargetPosition(Vector3 inputPosition)
    {
        return inputPosition;
    }

    protected virtual void CalculateTokenValue(ActionToken token)
    {

    }

    protected override void Update()
    {
        base.Update();
        if (heldToken)
        {
            Vector3 pressedPosition = ConvertToWorldInputPosition(currentInputPosition.screen);
            heldToken.targetPosition = CalculateHeldTokenTargetPosition(pressedPosition);

            if (endPress)
            {
                DropHeldToken();
            }
        }
        else
        {
            if (beginPress && (attackViewUserInterface.selectedTargeter == null || attackViewUserInterface.selectedTargeter == this))
            {
                CheckTokensForPickup();
            }
        }
    }

    protected virtual void CheckTokensForPickup()
    {
        Vector3 pressedPosition = ConvertToWorldInputPosition(currentInputPosition.screen);
        if (Vector3.Distance(pressedPosition, defaultPedestalPosition) < 3.0f)
        {
            PickupToken(stackTokens[stackTokens.Count - 1]);
        }
        else
        {
            ActionToken closestValidToken = null;
            float closestDistance = Mathf.Infinity;
            foreach (ActionToken token in placedTokens)
            {
                float distance = Vector3.Distance(token.transform.position, pressedPosition);
                if (distance < 3.0f && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestValidToken = token;
                }
            }

            if (closestValidToken)
            {
                PickupToken(closestValidToken);
            }
        }
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
                managedBoard = Battle.main.defender.board;
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
