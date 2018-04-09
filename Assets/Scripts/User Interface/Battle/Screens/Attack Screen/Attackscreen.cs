using System.Collections;
using System.Collections.Generic;
using System;
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
        public Effect[] abilityTokenTypes;
        public float tokenStartingPositionPadding;
        public float abilityTokenSpacing;
        public float abilityTokenSpace;
        Firebutton firebutton;
        protected override void PerformLinkageOperations()
        {
            base.PerformLinkageOperations();
            LinkAgent(FindAgent(x => { return x.player != player; }, typeof(Flag)), true);
            grid = (Agents.Grid)LinkAgent(FindAgent(x => { return x.player == player; }, typeof(Agents.Grid)), true);
            grid.Delinker += () => { grid = null; };

            grid.ShowInformation(false, false, true, true);

            firebutton = (Firebutton)LinkAgent(FindAgent(x => { return true; }, typeof(Firebutton)), true);
            firebutton.Delinker += () => { firebutton = null; };

            firebutton.hookedPosition = player.transform.position + Vector3.left * (player.board.tiles.GetLength(0) / 1.5f) + Vector3.up * MiscellaneousVariables.it.boardUIRenderHeight + Vector3.back * player.board.tiles.GetLength(1) / 3.0f;

            Delinker += () => { Token.heldToken = null; };

            Vector3 startingPositionRelativeToCamera = player.transform.position + Vector3.right * (player.board.tiles.GetLength(0) / 1.5f) + Vector3.up * MiscellaneousVariables.it.boardUIRenderHeight - cameraWaypoint.transform.position;
            Vector3 boardEdgeRelativeToCamera = new Vector3(player.board.tiles[player.board.tiles.GetLength(0) - 1, 0].transform.position.x, MiscellaneousVariables.it.boardUIRenderHeight, 0) - cameraWaypoint.transform.position;
            float originalDistance = Vector3.Distance(startingPositionRelativeToCamera, boardEdgeRelativeToCamera);

            float startPosNormalAngle = Vector3.Angle(Vector3.down, startingPositionRelativeToCamera.normalized);
            float boardEdgeNormalAngle = Vector3.Angle(Vector3.down, boardEdgeRelativeToCamera.normalized);
            float angleBetweenNormals = Mathf.Abs(startPosNormalAngle - boardEdgeNormalAngle);

            float axisNormalPadding = Mathf.Cos((startPosNormalAngle + angleBetweenNormals / 2.0f) * Mathf.Deg2Rad) * tokenStartingPositionPadding;
            float positionDistance = axisNormalPadding / (2.0f * Mathf.Cos((180 - angleBetweenNormals) / 2.0f * Mathf.Deg2Rad));

            startingPositionRelativeToCamera = startingPositionRelativeToCamera.normalized * positionDistance;
            boardEdgeRelativeToCamera = boardEdgeRelativeToCamera.normalized * positionDistance;

            float scalar = Vector3.Distance(startingPositionRelativeToCamera, boardEdgeRelativeToCamera) / originalDistance;



            Vector3 abilityTokenStart = startingPositionRelativeToCamera + Vector3.forward * (player.board.tiles.GetLength(1) / abilityTokenSpacing) * scalar + cameraWaypoint.transform.position;
            Vector3 abilityTokenStep = Vector3.back * (player.board.tiles.GetLength(1) / (abilityTokenSpacing * abilityTokenTypes.Length)) * scalar;

            for (int i = 0; i < abilityTokenTypes.Length; i++)
            {
                Token.SetTypeStacking(abilityTokenTypes[i].GetType(), abilityTokenStart + abilityTokenStep * i, Vector3.right * (abilityTokenSpace * scalar / 5));
            }

            Vector3 eventTokenStart = startingPositionRelativeToCamera + Vector3.forward * (player.board.tiles.GetLength(1) / 2.0f) * scalar;
            eventTokenStart.x *= -1;
            eventTokenStart += cameraWaypoint.transform.position;

            Token.SetTypeStacking(typeof(Gameplay.Event), eventTokenStart, Vector3.back * player.board.tiles.GetLength(1) / 4.7f * scalar);

            LinkAgents(FindAgents(x =>
            {
                Token token = x as Token;
                return token.ConnectWithAnyCompatibleEffect();
            }, typeof(Token), int.MaxValue), true);

            UpdateAbilityTokens();
        }

        protected override void ProcessInput()
        {
            base.ProcessInput();
            if (Token.heldToken == null)
            {
                if (endPress && firebutton.TryPush(initialInputPosition.world))
                {
                    gameObject.SetActive(false);
                    Battle.main.NextTurn();
                    FindAgent(x => { return x.player == player; }, typeof(Damagereport)).gameObject.SetActive(true);
                }
                else if (beginPress)
                {
                    firebutton.TryPush(initialInputPosition.world);
                }
                else
                {
                    BattleUIAgent[] allTokens = FindAgents(x => { return x.linked; }, typeof(Token), int.MaxValue);
                    for (int i = 0; i < allTokens.Length; i++)
                    {
                        Token token = allTokens[i] as Token;
                        if (token.IsPositionActivating(initialInputPosition.world))
                        {
                            if (dragging && !(token is EventToken))
                            {
                                token.Pickup();
                            }
                            else if (tap && token.effect != null && token.effect.GetDescription().Length > 0)
                            {
                                SetInteractable(false);
                                TokenHinter hinter = FindAgent(x => { return true; }, typeof(TokenHinter)) as TokenHinter;

                                hinter.token = token;
                                hinter.gameObject.SetActive(true);
                                hinter.Delinker += () => { SetInteractable(true); CameraControl.GoToWaypoint(cameraWaypoint); };

                                return;
                            }
                        }
                    }

                    if (tap)
                    {
                        bool tapOutsideOfBoardArea = grid.GetTileAtPosition(currentInputPosition.world) == null;
                        if (tapOutsideOfBoardArea)
                        {
                            gameObject.SetActive(false);
                            FindAgent(x => { return true; }, typeof(Overview)).gameObject.SetActive(true);
                        }
                    }
                }
            }

            if (Token.heldToken != null)
            {
                if (pressed) Token.heldToken.ProcessExternalInputWhileHeld(currentInputPosition.world);
                else if (endPress)
                {
                    Token.heldToken.Drop();
                    UpdateAbilityTokens();
                }
            }
        }

        /// <summary>
        /// Regulates the amount of each ability token, based on the usability of their effects.
        /// </summary>
        void UpdateAbilityTokens()
        {
            for (int i = 0; i < abilityTokenTypes.Length; i++)
            {
                int currentTokenCount = Token.FindTokens(true, false, abilityTokenTypes[i].GetType(), int.MaxValue).Length;
                int extraTokens = currentTokenCount - abilityTokenTypes[i].GetAdditionalAllowed(true);
                if (extraTokens > 0)
                {
                    Array.ForEach(Token.FindTokens(true, false, abilityTokenTypes[i].GetType(), extraTokens), x => { x.Delinker(); });
                }
                else if (extraTokens < 0)
                {
                    Token[] requestedTokens = Token.FindTokens(false, false, abilityTokenTypes[i].GetType(), -extraTokens);
                    Array.ForEach(requestedTokens, requestedToken => { requestedToken.player = player; });
                    LinkAgents(requestedTokens, true);
                }
            }
        }

        protected override Vector2 GetFrameSize()
        {
            return base.GetFrameSize() + new Vector2(player.board.tiles.GetLength(0), player.board.tiles.GetLength(1));
        }

        protected override Vector3 GetPosition()
        {
            return base.GetPosition() + player.transform.position;
        }

        protected override float CalculateConversionDistance()
        {
            return Camera.main.transform.position.y - MiscellaneousVariables.it.boardUIRenderHeight;
        }
    }
}
