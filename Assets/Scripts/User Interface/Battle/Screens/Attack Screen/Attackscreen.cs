using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BattleUIAgents.Base;
using BattleUIAgents.Agents;
using BattleUIAgents.Tokens;


namespace BattleUIAgents.UI
{
    public class Attackscreen : ScreenBattleUIAgent
    {
        Agents.Grid grid;
        public Token[] tokenTypes;
        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            LinkAgent(FindAgent(x => { return x is Flag && x.player != player; }));
            grid = (Agents.Grid)LinkAgent(FindAgent(x => { return x is Agents.Grid && x.player == player; }));
            grid.Delinker += () => { grid = null; };

            Delinker += () => { Token.heldToken = null; };

            for (int i = 0; i < tokenTypes.Length; i++)
            {
                LinkAgents(FindAgents(x =>
                {
                    if (x.GetType() == tokenTypes[i].GetType())
                    {
                        x.player = player;
                        return true;
                    }

                    return false;
                }, tokenTypes[i].effectPrefab.GetAdditionalAllowed()));
            }
        }

        protected override void ProcessInput()
        {
            base.ProcessInput();
            if (Token.heldToken == null)
            {
                if (beginPress)
                {
                    BattleUIAgent[] allTokens = FindAgents(x => { return x is Token && x.linked; }, int.MaxValue);
                    for (int i = 0; i < allTokens.Length; i++)
                    {
                        if (((Token)allTokens[i]).TryPickup(currentInputPosition.world)) break;
                    }
                }
                else if (tap)
                {
                    bool tapOutsideOfBoardArea = grid.GetTileAtPosition(currentInputPosition.world) == null;
                    if (tapOutsideOfBoardArea)
                    {
                        gameObject.SetActive(false);
                        FindAgent(x => { return x is Overview; }).gameObject.SetActive(true);
                    }
                }
            }
            if (Token.heldToken != null)
            {
                if (pressed) Token.heldToken.ProcessExternalInputWhileHeld(currentInputPosition.world);
                else if (endPress)
                {
                    Token.heldToken.Drop();

                    List<BattleUIAgent> toDelink = new List<BattleUIAgent>();
                    for (int i = 0; i < tokenTypes.Length; i++)
                    {
                        toDelink.AddRange(FindAgents(x => { return x.GetType() == tokenTypes[i].GetType() && x.linked && ((Token)x).boundEffect == null; }, FindAgents(x => { return x.GetType() == tokenTypes[i].GetType() && x.linked; }, int.MaxValue).Length - tokenTypes[i].effectPrefab.GetAdditionalAllowed()));
                    }

                    toDelink.ForEach(x => { x.Delinker(); });
                }
            }
        }

        protected override Vector2 GetFrameSize()
        {
            return base.GetFrameSize() + new Vector2(player.board.tiles.GetLength(0), player.board.tiles.GetLength(1));
        }

        protected override Vector3 GetPosition()
        {
            return base.GetPosition() + player.transform.position + Vector3.left * (player.board.tiles.GetLength(0) / 4.0f);
        }

        protected override float CalculateConversionDistance()
        {
            return Camera.main.transform.position.y - MiscellaneousVariables.it.boardUIRenderHeight;
        }
    }
}
