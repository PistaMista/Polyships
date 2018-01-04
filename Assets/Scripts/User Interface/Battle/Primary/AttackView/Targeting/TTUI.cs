using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTUI : BoardViewUI
{
    Player managedAttacker;
    public AttackViewUI attackViewUserInterface;
    public Pedestal_TTAgent stackPedestal;
    public List<Token_TTAgent> placedTokens;
    public List<Token_TTAgent> stackTokens;
    protected Token_TTAgent[] allTokens;
    protected Token_TTAgent heldToken;

    public virtual void ResetTargeting() //Resets the targeting completely
    {
        Vector3 pedestalDirectional = stackPedestal.enabledPositions[0] - managedBoard.owner.boardCameraPoint.transform.position;
        stackPedestal.enabledPositions[1] = managedBoard.owner.boardCameraPoint.transform.position + pedestalDirectional.normalized * pedestalDirectional.magnitude / (managedBoard.tiles.GetLength(0) / (float)attackViewUserInterface.referenceBoardWidthForPedestalScaling);
        stackPedestal.enabledPositions[2] = stackPedestal.enabledPositions[1] + Vector3.right * managedBoard.tiles.GetLength(0) / 2.0f;
        stackPedestal.transform.position = stackPedestal.enabledPositions[0];

        stackPedestal.disabledPosition = stackPedestal.enabledPositions[0];
        stackPedestal.disabledPosition.y = -10;

        if (allTokens != null)
        {
            RemoveDynamicAgents<Token_TTAgent>("", true);
        }
        allTokens = null;
        heldToken = null;
        AddTokens();
    }

    protected void AddTokens() //Spawns in the tokens
    {
        int tokenCount = GetInitialTokenCount();

        allTokens = new Token_TTAgent[tokenCount];
        stackTokens = new List<Token_TTAgent>();
        placedTokens = new List<Token_TTAgent>();

        for (int i = 0; i < tokenCount; i++)
        {
            Token_TTAgent token = (Token_TTAgent)CreateDynamicAgent("token");
            token.owner = this;

            allTokens[i] = token;
            stackTokens.Add(token);
            token.OnPedestal = true;
            token.transform.localPosition = token.enabledPositions[0];
        }
    }

    protected virtual int GetInitialTokenCount()
    {
        return 0;
    }

    protected virtual void PickupToken(Token_TTAgent token)
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
        Vector3 directional = inputPosition - managedBoard.owner.boardCameraPoint.transform.position;
        return directional / directional.y * (stackPedestal.transform.TransformPoint(heldToken.enabledPositions[0]).y - managedBoard.owner.boardCameraPoint.transform.position.y) + managedBoard.owner.boardCameraPoint.transform.position;
    }

    protected virtual void CalculateTokenValue(Token_TTAgent token)
    {

    }

    protected override void Update()
    {
        base.Update();
        if ((int)State >= 2)
        {
            bool selectable = IsSelectable();
            if (heldToken)
            {
                Vector3 pressedPosition = ConvertToWorldInputPosition(currentInputPosition.screen);
                heldToken.enabledPositions[1] = CalculateHeldTokenTargetPosition(pressedPosition);

                if (endPress)
                {
                    DropHeldToken();
                    heldToken = null;
                }
            }
            else
            {
                if (beginPress && selectable)
                {
                    CheckTokensForPickup();
                }
            }
        }
    }

    public virtual bool IsSelectable()
    {
        return (attackViewUserInterface.selectedTargeter == null || attackViewUserInterface.selectedTargeter == this) && (int)State >= 2;
    }

    protected void CheckTokensForPickup()
    {
        Vector3 pressedPosition = ConvertToWorldInputPosition(currentInputPosition.screen);
        if (Vector3.Distance(pressedPosition, stackPedestal.enabledPositions[0]) < 3.0f && stackTokens.Count > 0)
        {
            PickupToken(stackTokens[stackTokens.Count - 1]);
        }
        else
        {
            Token_TTAgent closestValidToken = null;
            float closestDistance = Mathf.Infinity;
            foreach (Token_TTAgent token in placedTokens)
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

    protected override void SetState(UIState state)
    {
        switch (state)
        {
            case UIState.ENABLING:
                managedBoard = Battle.main.defender.board;
                if (managedAttacker != Battle.main.attacker) //If the last attacker played his turn reset the targeter
                {
                    managedAttacker = Battle.main.attacker;
                    ResetTargeting();
                }

                stackPedestal.transform.position = stackPedestal.disabledPosition;
                break;
        }
        base.SetState(state);
    }
}
