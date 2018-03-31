using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BattleUIAgents.Base;
using BattleUIAgents.Agents;
using BattleUIAgents.Tokens;

using Gameplay;

namespace BattleUIAgents.UI
{
    public class Attackscreen : ScreenBattleUIAgent
    {
        Agents.Grid grid;
        public Effect[] tokenEffectTypes;
        Vector3 tokenInitialSpotStart;
        Vector3 tokenInitialSpotStep;
        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            LinkAgent(FindAgent(x => { return x.player != player; }, typeof(Flag)));
            grid = (Agents.Grid)LinkAgent(FindAgent(x => { return x.player == player; }, typeof(Agents.Grid)));
            grid.Delinker += () => { grid = null; };

            Delinker += () => { Token.heldToken = null; };

            tokenInitialSpotStart = player.transform.position + Vector3.right * (player.board.tiles.GetLength(0) / 1.5f) + Vector3.forward * (player.board.tiles.GetLength(1) / 2.0f);
            tokenInitialSpotStep = Vector3.back * (player.board.tiles.GetLength(1) / (2.0f * tokenEffectTypes.Length));

            UpdateTokenSelection();
        }

        protected override void ProcessInput()
        {
            base.ProcessInput();
            if (Token.heldToken == null)
            {
                if (beginPress)
                {
                    BattleUIAgent[] allTokens = FindAgents(x => { return x.linked; }, typeof(Token), int.MaxValue);
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
                        FindAgent(x => { return true; }, typeof(Overview)).gameObject.SetActive(true);
                    }
                }
            }
            if (Token.heldToken != null)
            {
                if (pressed) Token.heldToken.ProcessExternalInputWhileHeld(currentInputPosition.world);
                else if (endPress)
                {
                    Token.heldToken.Drop();
                    UpdateTokenSelection();
                }
            }
        }

        void UpdateTokenSelection()
        {
            for (int i = 0; i < tokenEffectTypes.Length; i++)
            {
                BattleAgentFilterPredicate effectlessLinkedTokenFilter = x =>
                {
                    if (x.linked)
                    {
                        Token token = (Token)x;
                        return token.effect == null;
                    }

                    return false;
                };

                int extraTokens = FindAgents(effectlessLinkedTokenFilter, tokenEffectTypes[i].GetType(), int.MaxValue).Length - tokenEffectTypes[i].GetAdditionalAllowed();
                if (extraTokens > 0)
                {
                    BattleUIAgent[] toDelink = FindAgents(effectlessLinkedTokenFilter, tokenEffectTypes[i].GetType(), extraTokens);
                    for (int z = 0; z < toDelink.Length; z++)
                    {
                        toDelink[z].Delinker();
                    }
                }
                else if (extraTokens < 0)
                {
                    LinkAgents(FindAgents(x =>
                    {
                        if (!x.linked)
                        {
                            Token token = (Token)x;

                            token.player = player;
                            token.initialSpot = tokenInitialSpotStart + tokenInitialSpotStep * i;
                            return true;
                        }

                        return false;
                    }, tokenEffectTypes[i].GetType(), -extraTokens));
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
