using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameplay;
using Gameplay.Ships;
using BattleUIAgents.Base;
using BattleUIAgents.Agents;


namespace BattleUIAgents.UI
{
    public class FleetPlacer : ScreenBattleUIAgent
    {
        Agents.Grid grid;
        Shipbox shipbox;
        [Header("Ship Animation Configuration")]
        public float shipAnimationTravelTime;
        public float shipAnimationMaxSpeed;

        protected override void PerformLinkageOperations()
        {
            player = Battle.main.attacker;
            base.PerformLinkageOperations();

            grid = (Agents.Grid)LinkAgent(FindAgent(x => { return x.player == player; }, typeof(Agents.Grid)), true);
            grid.Delinker += () => { grid = null; };

            shipbox = (Shipbox)LinkAgent(FindAgent(x => { return true; }, typeof(Shipbox)), true);
            shipbox.Delinker += () => { shipbox = null; };

            shipbox.hookedPosition = player.transform.position + Vector3.left * player.board.tiles.GetLength(0) + Vector3.up * MiscellaneousVariables.it.boardUIRenderHeight;

            Delinker += () =>
            {
                for (int i = 0; i < player.board.ships.Length; i++)
                {
                    player.board.ships[i].Invoke("Hide", 3.0f);
                }
            };

            player.board.SpawnShips();
            shipbox.Populate(player.board.placementInfo.allShips);
        }

        void HidePlacedShips()
        {
            for (int i = 0; i < player.board.ships.Length; i++)
            {
                player.board.ships[i].gameObject.SetActive(false);
            }
        }
        protected override void ProcessInput()
        {
            base.ProcessInput();
            if (tap)
            {
                bool newShipSelected = false;
                foreach (Ship ship in player.board.placementInfo.allShips)
                {
                    Vector3 localInputPosition = ship.transform.InverseTransformPoint(currentInputPosition.world);
                    if (Mathf.Abs(localInputPosition.x) < 0.5f && Mathf.Abs(localInputPosition.z) < ship.maxHealth / 2.0f)
                    {
                        if (player.board.placementInfo.selectedShip != null)
                        {
                            player.board.placementInfo.selectedShip.Place(player.board.placementInfo.selectedShip.placementInfo.lastLocation);
                        }

                        ship.Pickup();
                        newShipSelected = true;

                        // foreach (Gameplay.Tile tile in managedBoard.placementInfo.validTiles)
                        // {
                        //     RemoveTileAgent(tile.coordinates);
                        // }
                        UpdateMarkers();
                        break;
                    }
                }

                if (!newShipSelected) //If no other ship was selected
                {
                    bool clickedOnBoard = grid.GetTileAtPosition(currentInputPosition.world) != null;
                    bool clickedOnDrawer = shipbox.IsPressed();
                    if (player.board.placementInfo.selectedShip != null) //If a ship is already selected
                    {
                        if (!clickedOnBoard) //If the user clicked outside of the board
                        {
                            if (clickedOnDrawer) //And the player clicks on the drawer
                            {
                                //Put the ship into the drawer
                                player.board.placementInfo.selectedShip.Place(null);
                            }
                            else
                            {
                                //Place the ship back where it was
                                player.board.placementInfo.selectedShip.Place(player.board.placementInfo.selectedShip.placementInfo.lastLocation);
                            }

                            UpdateMarkers();
                        }
                    }
                    else
                    {
                        if (clickedOnDrawer && player.board.placementInfo.notplacedShips.Count == 0)
                        {
                            Battle.main.NextTurn();
                            gameObject.SetActive(false);
                            FindAgent(x => { return true; }, typeof(TurnNotifier)).gameObject.SetActive(true);
                        }
                    }
                }
            }

            if (pressed && player.board.placementInfo.selectedShip != null)
            {
                Gameplay.Tile candidateTile = grid.GetTileAtPosition(currentInputPosition.world);
                if (candidateTile != null)
                {
                    if (player.board.placementInfo.selectableTiles.Contains(candidateTile))
                    {
                        player.board.SelectTileForPlacement(candidateTile);
                        UpdateMarkers();
                    }
                }
            }

            if (endPress)
            {
                if (player.board.placementInfo.selectedShip)
                {
                    player.board.placementInfo.selectedTiles = new List<Gameplay.Tile>();
                    player.board.ReevaluateTiles();
                    UpdateMarkers();
                }
            }
        }

        protected override float CalculateConversionDistance()
        {
            return Camera.main.transform.position.y - MiscellaneousVariables.it.boardUIRenderHeight;
        }

        void UpdateMarkers()
        {
            List<Gameplay.Tile> setTiles = new List<Gameplay.Tile>();
            //CONCEALMENT
            foreach (Ship ship in player.board.placementInfo.placedShips)
            {
                if (ship.type == ShipType.CRUISER)
                {
                    Cruiser cruiser = (Cruiser)ship;
                    foreach (Gameplay.Tile tile in cruiser.concealmentArea)
                    {
                        if (!player.board.placementInfo.occupiedTiles.Contains(tile) && !player.board.placementInfo.invalidTiles.Contains(tile) && !player.board.placementInfo.selectedTiles.Contains(tile))
                        {
                            if (cruiser.concealing == null)
                            {
                                grid.SetTileGraphic(tile.coordinates, Agents.Grid.TileGraphicMaterial.TILE_CONCEALMENT_AREA, Color.white);
                                setTiles.Add(tile);
                            }
                        }
                    }
                }
            }
            //INVALID
            foreach (Gameplay.Tile tile in player.board.placementInfo.invalidTiles)
            {
                if (!player.board.placementInfo.occupiedTiles.Contains(tile) && !player.board.placementInfo.selectedTiles.Contains(tile))
                {
                    grid.SetTileGraphic(tile.coordinates, Agents.Grid.TileGraphicMaterial.TILE_RESTRICTED, Color.white);
                    setTiles.Add(tile);
                }
            }
            //SELECTED
            foreach (Gameplay.Tile tile in player.board.placementInfo.selectedTiles)
            {
                if (!player.board.placementInfo.occupiedTiles.Contains(tile))
                {
                    grid.SetTileGraphic(tile.coordinates, Agents.Grid.TileGraphicMaterial.TILE_SELECTED_FOR_PLACEMENT, Color.white);
                    setTiles.Add(tile);
                }
            }
            //OCCUPIED
            foreach (Ship ship in player.board.placementInfo.placedShips)
            {
                for (int i = 0; i < ship.tiles.Length; i++)
                {
                    Gameplay.Tile tile = ship.tiles[i];
                    if (ship.concealedBy)
                    {
                        grid.SetTileGraphic(tile.coordinates, Agents.Grid.TileGraphicMaterial.SHIP_CONCEALED, Color.white);
                    }
                    else
                    {
                        grid.SetTileGraphic(tile.coordinates, Agents.Grid.TileGraphicMaterial.SHIP_INTACT, Color.white);
                    }
                    setTiles.Add(tile);
                }
            }

            for (int x = 0; x < player.board.tiles.GetLength(0); x++)
            {
                for (int y = 0; y < player.board.tiles.GetLength(1); y++)
                {
                    Gameplay.Tile candidate = player.board.tiles[x, y];
                    if (!setTiles.Contains(candidate))
                    {
                        grid.SetTileGraphic(candidate.coordinates, Agents.Grid.TileGraphicMaterial.NONE, Color.clear);
                    }
                }
            }

            //SetDestroyerFiringAreaMarkers(true, player.board.placementInfo.placedShips.ToArray());
        }

        protected override void Update()
        {
            base.Update();
            Ship[] ships = player.board.placementInfo.allShips.ToArray();

            foreach (Ship ship in ships)
            {

                if (ship.placementInfo.waypoints != null)
                {
                    ship.transform.position = Vector3.SmoothDamp(ship.transform.position, ship.placementInfo.waypoints[0], ref ship.placementInfo.animationVelocity, shipAnimationTravelTime, shipAnimationMaxSpeed);

                    if (Vector3.Distance(ship.transform.position, ship.placementInfo.waypoints[0]) < 0.1f)
                    {
                        ship.placementInfo.waypoints.RemoveAt(0);
                    }

                    if (ship.placementInfo.waypoints.Count == 0)
                    {
                        ship.placementInfo.waypoints = null;
                    }
                }

                Quaternion targetRotation = ship.tiles == null ? ship.placementInfo.localShipboxRotation : ship.placementInfo.boardRotation;
                ship.transform.rotation = Quaternion.RotateTowards(ship.transform.rotation, targetRotation, Mathf.Pow(Quaternion.Angle(ship.transform.rotation, targetRotation) * Time.deltaTime * 10.0f, 0.5f));
            }
        }

        protected override Vector2 GetFrameSize()
        {
            return base.GetFrameSize() + new Vector2(player.board.tiles.GetLength(0), player.board.tiles.GetLength(1));
        }

        protected override Vector3 GetPosition()
        {
            return base.GetPosition() + player.transform.position + Vector3.left * (player.board.tiles.GetLength(0) / 2.0f);
        }
    }
}