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
    public float stackPedestalWidth;
    public float stackPedestalTransitionTime;
    public float stackMaximumDeviation;
    public Vector3 defaultPedestalPosition;
    public Vector3 scaledPedestalPosition;
    public Vector3 stowedPedestalPosition;
    public Vector3 pedestalVelocity;
    public List<ActionToken> placedTokens;
    public List<ActionToken> stackTokens;
    protected ActionToken[] allTokens;
    protected ActionToken heldToken;

    public void ResetTargeting() //Resets the targeting completely
    {
        stackPedestal.SetActive(true);

        Vector3 pedestalDirectional = defaultPedestalPosition - managedBoard.owner.boardCameraPoint.transform.position;
        scaledPedestalPosition = managedBoard.owner.boardCameraPoint.transform.position + pedestalDirectional.normalized * pedestalDirectional.magnitude / (managedBoard.tiles.GetLength(0) / (float)attackViewUserInterface.referenceBoardWidthForPedestalScaling);
        stowedPedestalPosition = scaledPedestalPosition + Vector3.right * managedBoard.tiles.GetLength(0) / 2.0f;
        stackPedestal.transform.position = scaledPedestalPosition;

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
    }

    protected void AddTokens() //Spawns in the tokens
    {
        int tokenCount = GetInitialTokenCount();

        allTokens = new ActionToken[tokenCount];
        stackTokens = new List<ActionToken>();
        placedTokens = new List<ActionToken>();

        for (int i = 0; i < tokenCount; i++)
        {
            ActionToken token = Instantiate(tokenPrefab).GetComponent<ActionToken>();
            token.owner = this;

            allTokens[i] = token;
            stackTokens.Add(token);
            token.OnPedestal = true;
            token.transform.localPosition = token.defaultPositionRelativeToPedestal;
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
            heldToken.defaultPositionRelativeToPedestal.y = (heldToken.stackHeight * stackTokens.Count + stackPedestalHeight / 2.0f - heldToken.stackHeight / 2.0f) * stackPedestal.transform.lossyScale.y;
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
        Vector3 directional = inputPosition - managedBoard.owner.boardCameraPoint.transform.position;
        return directional / directional.y * (stackPedestal.transform.TransformPoint(heldToken.defaultPositionRelativeToPedestal).y - managedBoard.owner.boardCameraPoint.transform.position.y) + managedBoard.owner.boardCameraPoint.transform.position;
    }

    protected virtual void CalculateTokenValue(ActionToken token)
    {

    }

    protected override void Update()
    {
        base.Update();
        if (State == UIState.ENABLING)
        {
            bool selectable = IsSelectable();
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
                if (beginPress && selectable)
                {
                    CheckTokensForPickup();
                }
            }

            if (selectable)
            {
                stackPedestal.transform.position = Vector3.SmoothDamp(stackPedestal.transform.position, scaledPedestalPosition, ref pedestalVelocity, stackPedestalTransitionTime);
            }
            else
            {
                stackPedestal.transform.position = Vector3.SmoothDamp(stackPedestal.transform.position, stowedPedestalPosition, ref pedestalVelocity, stackPedestalTransitionTime);
            }
        }
        else
        {
            Vector3 hideModifier = -Vector3.up * (scaledPedestalPosition.y + 10f);
            stackPedestal.transform.position = Vector3.SmoothDamp(stackPedestal.transform.position, scaledPedestalPosition + hideModifier, ref pedestalVelocity, stackPedestalTransitionTime);
        }
    }

    protected virtual bool IsSelectable()
    {
        return (attackViewUserInterface.selectedTargeter == null || attackViewUserInterface.selectedTargeter == this) && State == UIState.ENABLING;
    }

    protected void CheckTokensForPickup()
    {
        Vector3 pressedPosition = ConvertToWorldInputPosition(currentInputPosition.screen);
        if (Vector3.Distance(pressedPosition, defaultPedestalPosition) < 3.0f && stackTokens.Count > 0)
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
                    managedAttacker = Battle.main.attacker;
                    ResetTargeting();
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
