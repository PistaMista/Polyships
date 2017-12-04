using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleetPlacementUI : BoardViewUI
{
    protected override void Start()
    {
        base.Start();
        screenToWorldInputConversionHeight = MiscellaneousVariables.it.boardUIRenderHeight;
    }
    public Material shipDrawerMaterial;
    public GameObject shipDrawerDecorator;
    public Waypoint cameraWaypoint;
    public GameObject[] defaultShipLoadout;
    public bool forceIdenticalShipTypesToGroupTogether;
    public float shipPaletteGroupPadding;
    public float shipDrawerFlatSize;
    public float cameraWaypointOffset;
    protected override void ChangeState(UIState state)
    {
        switch (state)
        {
            case UIState.ENABLING:
                managedBoard = Battle.main.attacker.board;
                break;
            case UIState.DISABLED:
                if (managedBoard.owner.ships != null)
                {
                    for (int i = 0; i < managedBoard.owner.ships.Length; i++)
                    {
                        managedBoard.owner.ships[i].gameObject.SetActive(false);
                    }
                }
                break;
        }

        base.ChangeState(state);

        switch (state)
        {
            case UIState.ENABLING:
                Vector3 finalPosition = Battle.main.attacker.boardCameraPoint.transform.position + Vector3.left * Battle.main.attacker.board.tiles.GetLength(0) * cameraWaypointOffset;
                finalPosition.y = CameraControl.CalculateCameraWaypointHeight(new Vector2((Battle.main.attacker.board.tiles.GetLength(0)) * (1 + cameraWaypointOffset * 2.0f) + 3, Battle.main.attacker.board.tiles.GetLength(1) + 3));
                cameraWaypoint.transform.position = finalPosition;
                cameraWaypoint.transform.rotation = Battle.main.attacker.boardCameraPoint.transform.rotation;
                CameraControl.GoToWaypoint(cameraWaypoint, MiscellaneousVariables.it.playerCameraTransitionTime);

                notplacedShips = new List<Ship>();
                placedShips = new List<Ship>();
                allShips = new Dictionary<Ship, ShipInfo>();

                selectedTiles = new List<Tile>();
                validTiles = new List<Tile>();
                invalidTiles = new List<Tile>();
                occupiedTiles = new List<Tile>();

                DestroyDynamicAgents();
                MakeShipDrawer();

                ChangeState(UIState.ENABLED);
                break;
        }
    }

    public float shipAnimationTravelTime;
    public float shipAnimationMaxSpeed;
    public float shipAnimationElevation;

    struct ShipInfo
    {
        public Vector3 boardPosition;
        public Quaternion boardRotation;
        public Vector3 localDrawerPosition;
        public Quaternion localDrawerRotation;
        public Tile[] occupiedTiles;

        public List<Vector3> waypoints;
        public Vector3 animationVelocity;
        public float rotationVelocity;
    }

    public Material invalidTileMaterial;
    public Material occupiedTileMaterial;
    public Material selectedTileMaterial;



    List<Tile> selectedTiles; //List of tiles selected to house the currently selected ship
    List<Tile> occupiedTiles; //List of tiles where the placed ships are placed
    List<Tile> obstructedTiles; //List of tiles where nothing can be placed
    List<Tile> validTiles; //List of tiles where the current ship can be placed
    List<Tile> invalidTiles; //List of tiles where the current ship cannot be placed

    Ship selectedShip;
    List<Ship> notplacedShips;
    List<Ship> placedShips;
    Dictionary<Ship, ShipInfo> allShips;

    void ReevaluateTiles()
    {
        Vector2 boardSize = new Vector2(managedBoard.tiles.GetLength(0), managedBoard.tiles.GetLength(1));

        obstructedTiles = new List<Tile>();

        //Determine the tiles in which a 1-tile sized ship cannot be placed
        foreach (Tile tile in occupiedTiles)
        {
            for (int x = (tile.coordinates.x == 0 ? 0 : -1); x <= ((tile.coordinates.x == boardSize.x - 1) ? 0 : 1); x++)
            {
                for (int y = (tile.coordinates.y == 0 ? 0 : -1); y <= ((tile.coordinates.y == boardSize.y - 1) ? 0 : 1); y++)
                {
                    Tile obstructedTile = managedBoard.tiles[x + (int)tile.coordinates.x, y + (int)tile.coordinates.y];
                    if (!obstructedTiles.Contains(obstructedTile))
                    {
                        obstructedTiles.Add(obstructedTile);
                    }
                }
            }
        }

        invalidTiles = new List<Tile>();
        validTiles = new List<Tile>();

        for (int x = 0; x < boardSize.x; x++)
        {
            for (int y = 0; y < boardSize.y; y++)
            {
                invalidTiles.Add(managedBoard.tiles[x, y]);
            }
        }

        //Determine where the current ship can or cannot be placed
        for (int axis = 0; axis < 2; axis++) //The axis we are sweeping across
        {
            for (int line = 0; line < (axis == 0 ? boardSize.y : boardSize.x); line++)
            {
                List<Tile> inlineValidTiles = new List<Tile>();
                List<Tile> inlineNeighbouringValidTiles = new List<Tile>();
                for (int depth = 0; depth < (axis == 0 ? boardSize.x : boardSize.y); depth++)
                {
                    Tile examined = managedBoard.tiles[axis == 0 ? depth : line, axis == 0 ? line : depth];
                    if (!obstructedTiles.Contains(examined))
                    {
                        inlineNeighbouringValidTiles.Add(examined);
                    }
                    else
                    {
                        if (inlineNeighbouringValidTiles.Count >= selectedShip.health)
                        {
                            inlineValidTiles.AddRange(inlineNeighbouringValidTiles);
                        }
                        inlineNeighbouringValidTiles = new List<Tile>();
                    }
                }

                inlineValidTiles.AddRange(inlineNeighbouringValidTiles);

                foreach (Tile tile in inlineValidTiles)
                {
                    if (!validTiles.Contains(tile))
                    {
                        validTiles.Add(tile);
                        invalidTiles.Remove(tile);
                    }
                }
            }
        }

        UpdateValidPositionMarkers();
    }

    void UpdateValidPositionMarkers()
    {
        foreach (Tile tile in validTiles)
        {
            ResetTileParent(tile.coordinates);
        }

        foreach (Tile tile in invalidTiles)
        {
            if (!occupiedTiles.Contains(tile))
            {
                MovingUIAgent parent = GetTileParent(tile.coordinates, true);

                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
                marker.transform.SetParent(parent.transform);
                marker.transform.localPosition = Vector3.up * MiscellaneousVariables.it.boardUIRenderHeight;

                marker.transform.localScale = Vector3.one * MiscellaneousVariables.it.boardTileSideLength;
                marker.transform.Rotate(90, 0, 0);
                Renderer renderer = marker.GetComponent<Renderer>();
                renderer.material = invalidTileMaterial;
            }
        }
    }

    void HideValidPositionMarkers()
    {
        foreach (Tile tile in invalidTiles)
        {
            if (!occupiedTiles.Contains(tile))
            {
                ResetTileParent(tile.coordinates);
            }
        }
    }



    void SelectShip(Ship ship)
    {
        selectedShip = ship;
        if (placedShips.Contains(ship))
        {
            RemovePlacedShip(ship);
        }

        ReevaluateTiles();

        Debug.Log("Selected ship " + ship.name);
    }

    void RemovePlacedShip(Ship ship)
    {
        foreach (Tile tile in allShips[ship].occupiedTiles)
        {
            occupiedTiles.Remove(tile);
        }

        placedShips.Remove(ship);
        notplacedShips.Add(ship);
    }

    void PlaceCurrentlySelectedShip()
    {
        foreach (Tile tile in allShips[selectedShip].occupiedTiles)
        {
            MovingUIAgent parent = GetTileParent(tile.coordinates, true);

            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            marker.transform.SetParent(parent.transform);
            marker.transform.localPosition = Vector3.up * MiscellaneousVariables.it.boardUIRenderHeight;

            marker.transform.localScale = Vector3.one * MiscellaneousVariables.it.boardTileSideLength;
            marker.transform.Rotate(90, 0, 0);
            Renderer renderer = marker.GetComponent<Renderer>();
            renderer.material = occupiedTileMaterial;
        }

        occupiedTiles.AddRange(allShips[selectedShip].occupiedTiles);

        ShipInfo info = allShips[selectedShip];
        Vector3 directional = info.occupiedTiles[0].transform.position - info.occupiedTiles[info.occupiedTiles.Length - 1].transform.position;
        info.boardPosition = (info.occupiedTiles[0].transform.position + info.occupiedTiles[info.occupiedTiles.Length - 1].transform.position) / 2.0f;
        info.boardRotation = Quaternion.Euler(0, directional.z != 0 ? 0 : 90, 0);
        allShips[selectedShip] = info;

        notplacedShips.Remove(selectedShip);
        placedShips.Add(selectedShip);

        Debug.Log("Placed ship " + selectedShip.name);
    }

    void UpdateShipWaypoints(Ship ship, bool selectedMode)
    {
        bool placed = allShips[ship].occupiedTiles != null;

        ShipInfo info = allShips[ship];
        info.waypoints = new List<Vector3>();

        Vector3 targetPosition = placed ? info.boardPosition + Vector3.up * MiscellaneousVariables.it.boardUIRenderHeight : shipDrawer.transform.TransformPoint(info.localDrawerPosition);

        info.waypoints.Add(new Vector3(ship.transform.position.x, MiscellaneousVariables.it.boardUIRenderHeight + shipAnimationElevation, ship.transform.position.z));
        info.waypoints.Add(new Vector3(targetPosition.x, MiscellaneousVariables.it.boardUIRenderHeight + shipAnimationElevation, targetPosition.z));
        if (!selectedMode)
        {
            info.waypoints.Add(targetPosition);
        }
        allShips[ship] = info;
    }

    void SelectTile(Tile tile)
    {
        selectedTiles.Add(tile);
        MovingUIAgent parent = GetTileParent(tile.coordinates, true);

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
        marker.transform.SetParent(parent.transform);
        marker.transform.localPosition = Vector3.up * MiscellaneousVariables.it.boardUIRenderHeight;

        marker.transform.localScale = Vector3.one * MiscellaneousVariables.it.boardTileSideLength;
        marker.transform.Rotate(90, 0, 0);
        Renderer renderer = marker.GetComponent<Renderer>();
        renderer.material = selectedTileMaterial;
    }

    void FinalizePlacement()
    {
        managedBoard.owner.ships = placedShips.ToArray();

        foreach (Ship ship in placedShips)
        {
            ship.transform.SetParent(managedBoard.owner.transform);
        }

        fadingDistance = (managedBoard.tiles.GetLength(0)) * (1 + cameraWaypointOffset * 2.0f) + 3;

        ChangeState(UIState.DISABLING);
        Battle.main.NextTurn();
        BattleUIMaster.EnablePrimaryBUI(BattleUIType.TURN_NOTIFIER);
    }


    float fadingDistance;
    protected override void Update()
    {
        base.Update();
        switch (State)
        {
            case UIState.ENABLED:
                if (tap)
                {
                    bool newShipSelected = false;
                    foreach (Ship ship in allShips.Keys)
                    {
                        Vector3 localInputPosition = ship.transform.InverseTransformPoint(ConvertToWorldInputPosition(currentInputPosition.screen));
                        if (Mathf.Abs(localInputPosition.x) < 0.5f && Mathf.Abs(localInputPosition.z) < ship.health / 2.0f)
                        {
                            if (selectedShip != null)
                            {
                                if (allShips[selectedShip].occupiedTiles != null)
                                {
                                    PlaceCurrentlySelectedShip();
                                }
                                UpdateShipWaypoints(selectedShip, false);
                            }

                            SelectShip(ship);
                            UpdateShipWaypoints(selectedShip, true);
                            newShipSelected = true;
                            break;
                        }
                    }

                    if (!newShipSelected) //If no other ship was selected
                    {
                        Vector3 inputPositionInDrawer = shipDrawer.transform.InverseTransformPoint(ConvertToWorldInputPosition(currentInputPosition.screen));
                        bool clickedOnBoard = GetTileAtInputPosition() != null;
                        bool clickedOnDrawer = Mathf.Abs(inputPositionInDrawer.x) < shipDrawerFlatSize / 2.0f && Mathf.Abs(inputPositionInDrawer.z) < shipDrawerFlatSize / 2.0f;
                        if (selectedShip != null) //If a ship is not selected
                        {
                            if (!clickedOnBoard) //If the user clicked outside of the board
                            {
                                if (allShips[selectedShip].occupiedTiles != null) //If the ship was already on the board
                                {
                                    if (clickedOnDrawer) //And the player clicks on the drawer
                                    {
                                        //Remove the ship from the board and put it back into the drawer
                                        ShipInfo info = allShips[selectedShip];
                                        info.occupiedTiles = null;
                                        allShips[selectedShip] = info;
                                    }
                                    else
                                    {
                                        //Place the ship back where it was
                                        PlaceCurrentlySelectedShip();
                                    }

                                    UpdateShipWaypoints(selectedShip, false);
                                    selectedShip = null;
                                    HideValidPositionMarkers();
                                }
                            }
                        }
                        else
                        {
                            if (clickedOnDrawer && notplacedShips.Count == 0)
                            {
                                FinalizePlacement();
                            }
                        }
                    }
                }

                if (pressed && selectedShip != null)
                {
                    Tile candidateTile = GetTileAtInputPosition();
                    if (candidateTile != null)
                    {
                        if (!selectedTiles.Contains(candidateTile) && validTiles.Contains(candidateTile))
                        {
                            if (selectedTiles.Count == 0)
                            {
                                SelectTile(candidateTile);
                            }
                            else
                            {
                                bool connects = false;
                                bool outOfLine = false;

                                foreach (Tile tile in selectedTiles)
                                {
                                    float distance = Vector2.Distance(tile.coordinates, candidateTile.coordinates);
                                    if ((int)distance != distance)
                                    {
                                        outOfLine = true;
                                        break;
                                    }

                                    if (distance == 1)
                                    {
                                        connects = true;
                                    }
                                }

                                if (connects && !outOfLine)
                                {
                                    SelectTile(candidateTile);
                                }
                            }

                            if (selectedTiles.Count == selectedShip.health)
                            {

                                ShipInfo info = allShips[selectedShip];
                                info.occupiedTiles = selectedTiles.ToArray();
                                allShips[selectedShip] = info;

                                PlaceCurrentlySelectedShip();
                                UpdateShipWaypoints(selectedShip, false);
                                HideValidPositionMarkers();
                                selectedShip = null;
                                invalidTiles = new List<Tile>();
                                validTiles = new List<Tile>();
                                selectedTiles = new List<Tile>();
                            }
                        }
                    }
                }

                if (endPress)
                {
                    foreach (Tile tile in selectedTiles)
                    {
                        ResetTileParent(tile.coordinates);
                    }
                    selectedTiles = new List<Tile>();
                }

                Ship[] ships = new Ship[allShips.Keys.Count];
                allShips.Keys.CopyTo(ships, 0);

                foreach (Ship ship in ships)
                {
                    ShipInfo info = allShips[ship];

                    if (info.waypoints != null)
                    {
                        ship.transform.position = Vector3.SmoothDamp(ship.transform.position, info.waypoints[0], ref info.animationVelocity, shipAnimationTravelTime, shipAnimationMaxSpeed);

                        if (Vector3.Distance(ship.transform.position, info.waypoints[0]) < 0.1f)
                        {
                            info.waypoints.RemoveAt(0);
                        }

                        if (info.waypoints.Count == 0)
                        {
                            info.waypoints = null;
                        }
                    }

                    Quaternion targetRotation = info.occupiedTiles == null ? info.localDrawerRotation : info.boardRotation;
                    ship.transform.rotation = Quaternion.RotateTowards(ship.transform.rotation, targetRotation, Mathf.Pow(Quaternion.Angle(ship.transform.rotation, targetRotation) * Time.deltaTime * 10.0f, 0.5f));

                    allShips[ship] = info;
                }
                break;
            case UIState.DISABLING:
                if (Vector3.Distance(cameraWaypoint.transform.position, Camera.main.transform.position) > fadingDistance)
                {
                    ChangeState(UIState.DISABLED);
                }
                break;
        }
    }



    struct ShipRectangleGroup
    {
        public bool vertical;
        public Ship[] ships;
        public Rect rect;
        public Vector2[] horizontalCorners;
        public Vector2[] verticalCorners;
        public Vector2[] Corners
        {
            get { return vertical ? verticalCorners : horizontalCorners; }
        }
    }



    ShipRectangleGroup[] groups;
    ShipDrawer_FleetPlacementAgent shipDrawer;
    void MakeShipDrawer()
    {
        shipDrawer = (ShipDrawer_FleetPlacementAgent)CreateDynamicAgent("ship_drawer");

        List<ShipRectangleGroup> unfinishedGroups = new List<ShipRectangleGroup>();
        List<Ship> toAdd = new List<Ship>();

        ShipRectangleGroup currentGroup = new ShipRectangleGroup();
        for (int i = 0; i < defaultShipLoadout.Length; i++)
        {
            Ship ship = Instantiate(defaultShipLoadout[i]).GetComponent<Ship>();
            notplacedShips.Add(ship);
            allShips.Add(ship, new ShipInfo());

            ship.owner = Battle.main.attacker;
            ship.transform.SetParent(shipDrawer.transform);

            if (toAdd.Count == 0)
            {
                toAdd.Add(ship);
                continue;
            }

            if (toAdd[0].type == ship.type && forceIdenticalShipTypesToGroupTogether)
            {
                toAdd.Add(ship);
            }
            else
            {
                currentGroup.ships = toAdd.ToArray();
                toAdd = new List<Ship>();
                toAdd.Add(ship);
                unfinishedGroups.Add(currentGroup);
                currentGroup = new ShipRectangleGroup();
            }
        }

        currentGroup.ships = toAdd.ToArray();
        unfinishedGroups.Add(currentGroup);

        groups = unfinishedGroups.ToArray();

        for (int i = 0; i < groups.Length; i++)
        {
            groups[i].rect.height = groups[i].ships[0].health + shipPaletteGroupPadding;
            groups[i].rect.width = (groups[i].ships[0].transform.localScale.x + 0.5f) * groups[i].ships.Length + shipPaletteGroupPadding;
            groups[i].horizontalCorners = CalculateCorners(groups[i].rect, false);
            groups[i].verticalCorners = CalculateCorners(groups[i].rect, true);
        }


        ArrangeShipGroupsOnSquarePlane();
        MoldDrawerMeshes();

        shipDrawer.enabledPositions = new Vector3[1] { Vector3.left * Battle.main.attacker.board.tiles.GetLength(0) * 1.05f + Vector3.up * MiscellaneousVariables.it.boardUIRenderHeight };
        shipDrawer.disabledPosition = shipDrawer.enabledPositions[0];
        shipDrawer.disabledPosition.y = -10;

        shipDrawer.transform.localPosition = shipDrawer.enabledPositions[0];
    }

    void MoldDrawerMeshes()
    {
        List<Dictionary<Vector3, List<Vector3>>> flatpanelHoles = new List<Dictionary<Vector3, List<Vector3>>>();
        for (int groupIndex = 0; groupIndex < groups.Length; groupIndex++)
        {
            for (int shipIndex = 0; shipIndex < groups[groupIndex].ships.Length; shipIndex++)
            {
                //Initialize an object to store the hole this mesh is going to make in the flatpanel
                Dictionary<Vector3, List<Vector3>> hole = new Dictionary<Vector3, List<Vector3>>();
                //Get the mesh we are going to be molding
                MeshFilter moldedShipMesh = groups[groupIndex].ships[shipIndex].GetComponentInChildren<MeshFilter>();

                Vector3 positionRelativeToDrawer = groups[groupIndex].ships[shipIndex].transform.position - shipDrawer.transform.position;
                Vector3 positionMod = moldedShipMesh.gameObject.transform.position - groups[groupIndex].ships[shipIndex].transform.position;
                Vector3 scale = moldedShipMesh.gameObject.transform.lossyScale;

                //Assemble the new mesh
                Vector3[] originalVertices = moldedShipMesh.mesh.vertices;

                //Position the vertices correctly
                for (int vertexID = 0; vertexID < originalVertices.Length; vertexID++)
                {
                    originalVertices[vertexID] = moldedShipMesh.gameObject.transform.rotation * Vector3.Scale(originalVertices[vertexID], scale) + positionMod + positionRelativeToDrawer;
                }

                int[] originalTriangles = moldedShipMesh.mesh.triangles;


                List<Vector3> newVerticesList = new List<Vector3>();
                List<int> newTrianglesList = new List<int>();

                //Add the triangles along with their vertices
                for (int triangle = 0; triangle <= originalTriangles.Length - 3; triangle += 3)
                {
                    int[] triangleVertices = new int[] { originalTriangles[triangle], originalTriangles[triangle + 1], originalTriangles[triangle + 2] };

                    List<int> upperVertexIDs = new List<int>();
                    List<int> lowerVertexIDs = new List<int>();
                    for (int vertexID = 0; vertexID < triangleVertices.Length; vertexID++)
                    {
                        Vector3 vertexPosition = originalVertices[triangleVertices[vertexID]];
                        if (vertexPosition.y > 0)
                        {
                            upperVertexIDs.Add(vertexID);
                        }
                        else
                        {
                            lowerVertexIDs.Add(vertexID);
                        }
                    }

                    Vector3[] surfacePair = new Vector3[2] { Vector3.up * 999, Vector3.up * 999 };

                    switch (upperVertexIDs.Count)
                    {
                        case 0:
                            for (int vertexID = 0; vertexID < triangleVertices.Length; vertexID++)
                            {
                                Vector3 finalPosition = originalVertices[triangleVertices[vertexID]];

                                newVerticesList.Add(finalPosition);
                            }

                            newTrianglesList.Add(newVerticesList.Count - 1);
                            newTrianglesList.Add(newVerticesList.Count - 2);
                            newTrianglesList.Add(newVerticesList.Count - 3);
                            break;
                        case 1:
                            Vector3 originalTipPosition = originalVertices[triangleVertices[upperVertexIDs[0]]];
                            List<Vector3> retractedPoints = new List<Vector3>();

                            //Add the points to and unordered list
                            int surfaceID = 0;
                            foreach (int vertexID in lowerVertexIDs)
                            {
                                Vector3 position = originalVertices[triangleVertices[vertexID]];

                                Vector3 relativePosition = originalTipPosition - position;

                                Vector3 normalizationAgent = relativePosition.normalized / relativePosition.normalized.y;
                                Vector3 retractedPointPosition = -normalizationAgent * position.y + position;

                                newVerticesList.Add(position);
                                retractedPoints.Add(retractedPointPosition);

                                surfacePair[surfaceID] = retractedPointPosition;
                                surfaceID++;
                            }

                            newVerticesList.AddRange(retractedPoints);

                            bool invert = lowerVertexIDs[1] - lowerVertexIDs[0] > 1;
                            //Add the first triangle of the quad
                            newTrianglesList.Add(newVerticesList.Count - (invert ? 3 : 2));
                            newTrianglesList.Add(newVerticesList.Count - (invert ? 2 : 3));
                            newTrianglesList.Add(newVerticesList.Count - 4);

                            //Add the seconds triangle of the quad
                            newTrianglesList.Add(newVerticesList.Count - (invert ? 1 : 2));
                            newTrianglesList.Add(newVerticesList.Count - (invert ? 2 : 1));
                            newTrianglesList.Add(newVerticesList.Count - 3);
                            break;
                        case 2:
                            surfaceID = 0;
                            for (int vertexID = 0; vertexID < triangleVertices.Length; vertexID++)
                            {
                                Vector3 finalPosition = originalVertices[triangleVertices[vertexID]];
                                if (upperVertexIDs.Contains(vertexID))
                                {
                                    Vector3 linkedPosition = originalVertices[triangleVertices[lowerVertexIDs[0]]];
                                    Vector3 relativePosition = finalPosition - linkedPosition;

                                    Vector3 normalizationAgent = relativePosition.normalized / relativePosition.normalized.y;
                                    finalPosition = -normalizationAgent * linkedPosition.y + linkedPosition;

                                    surfacePair[surfaceID] = finalPosition;
                                    surfaceID++;
                                }

                                newVerticesList.Add(finalPosition);
                            }

                            newTrianglesList.Add(newVerticesList.Count - 1);
                            newTrianglesList.Add(newVerticesList.Count - 2);
                            newTrianglesList.Add(newVerticesList.Count - 3);
                            break;
                    }

                    //If this triangle is on the surface add its intersection to the hole
                    if (upperVertexIDs.Count == 1 || upperVertexIDs.Count == 2)
                    {
                        for (int point = 0; point < 2; point++)
                        {
                            int connection = (point + 1) % 2;
                            if (!hole.ContainsKey(surfacePair[point]))
                            {
                                hole.Add(surfacePair[point], new List<Vector3>());
                            }

                            hole[surfacePair[point]].Add(surfacePair[connection]);
                        }
                    }
                }

                //Add the hole this mesh has made in the flatpanel
                flatpanelHoles.Add(hole);

                Mesh finalMesh = new Mesh();
                finalMesh.vertices = newVerticesList.ToArray();
                finalMesh.triangles = newTrianglesList.ToArray();
                finalMesh.RecalculateNormals();

                GameObject shipMold = new GameObject("Ship Mold");
                shipMold.transform.SetParent(shipDrawer.transform, false);
                //shipMold.transform.position = moldedShipMesh.gameObject.transform.position + Vector3.up * 10;
                //shipMold.transform.rotation = moldedShipMesh.gameObject.transform.rotation;

                MeshFilter meshFilter = shipMold.AddComponent<MeshFilter>();
                shipMold.AddComponent<MeshRenderer>();
                meshFilter.mesh = finalMesh;

                shipMold.GetComponent<Renderer>().material = shipDrawerMaterial;
            }
        }

        //Order all of the vertices of each hole in a counter-clockwise direction
        List<Vector3[]> unsortedOrderedHoles = new List<Vector3[]>();
        List<float> holeMaxXValues = new List<float>();

        foreach (Dictionary<Vector3, List<Vector3>> hole in flatpanelHoles)
        {
            Vector3[] vertices = new Vector3[hole.Keys.Count];
            hole.Keys.CopyTo(vertices, 0);

            float highestXValue = -Mathf.Infinity;
            //Get a list of vertices, where each vertex connects to the next one in the list - a connected hole
            List<Vector3> connectedHole = new List<Vector3>();
            Vector3 currentPosition = vertices[0];
            for (int i = 0; i < vertices.Length; i++)
            {
                highestXValue = currentPosition.x > highestXValue ? currentPosition.x : highestXValue;
                foreach (Vector3 connection in hole[currentPosition])
                {
                    if (!connectedHole.Contains(connection))
                    {
                        connectedHole.Add(connection);
                        currentPosition = connection;
                        break;
                    }
                }
            }

            //Determine if the connected hole is now clockwise or counter-clockwise
            float deterministicSum = 0;
            for (int i = 0; i < connectedHole.Count; i++)
            {
                Vector3 first = connectedHole[i];
                Vector3 second = connectedHole[(i + 1) % connectedHole.Count];

                deterministicSum += (second.x - first.x) * (second.z + first.z);
            }

            //If its clockwise make it counter-clockwise
            if (deterministicSum > 0)
            {
                connectedHole.Reverse();
            }

            //Determine where to insert this hole in the list such that the holes furthest to the right come first
            int insertionPoint = unsortedOrderedHoles.Count;
            for (int holeID = 0; holeID < unsortedOrderedHoles.Count; holeID++)
            {
                if (highestXValue > holeMaxXValues[holeID])
                {
                    insertionPoint = holeID;
                    break;
                }
            }

            unsortedOrderedHoles.Insert(insertionPoint, connectedHole.ToArray());
            holeMaxXValues.Insert(insertionPoint, highestXValue);
        }

        Vector3[][] orderedHoles = unsortedOrderedHoles.ToArray();

        //Assemble a simple polygon out of a plane and these ordered holes
        float halfSize = shipDrawerFlatSize / 2.0f;
        List<Vector3> processedPolygon = new List<Vector3>() { new Vector3(-halfSize, 0, halfSize), new Vector3(halfSize, 0, halfSize), new Vector3(halfSize, 0, -halfSize), new Vector3(-halfSize, 0, -halfSize) };

        for (int holeID = 0; holeID < orderedHoles.Length; holeID++)
        {
            //Find the vertex of the hole, that is furthest to the right
            int firstVertexInHoleID = 0;
            Vector3 firstVertexInHolePosition = orderedHoles[holeID][0];
            for (int holePointID = 0; holePointID < orderedHoles[holeID].Length; holePointID++)
            {
                Vector3 candidatePosition = orderedHoles[holeID][holePointID];
                if (candidatePosition.x > firstVertexInHolePosition.x)
                {
                    firstVertexInHoleID = holePointID;
                    firstVertexInHolePosition = candidatePosition;
                }
            }

            //Find a vertex on the edge of the existing polygon to connect the hole with
            int injectionPointID = 0;
            Vector3 edgeConnector = Vector3.right * Mathf.Infinity;
            for (int polygonVertexID = 0; polygonVertexID < processedPolygon.Count; polygonVertexID++)
            {
                Vector3 firstVertexRelative = processedPolygon[polygonVertexID] - firstVertexInHolePosition;
                Vector3 secondVertexRelative = processedPolygon[(polygonVertexID + 1) % processedPolygon.Count] - firstVertexInHolePosition;

                //If one point is below the line and the other above
                if (firstVertexRelative.z * secondVertexRelative.z <= 0)
                {
                    Vector3 directional = (secondVertexRelative - firstVertexRelative).normalized;
                    Vector3 normalizationAgent = directional / directional.z;

                    Vector3 potentialEdgeConnector = firstVertexRelative - normalizationAgent * firstVertexRelative.z + firstVertexInHolePosition;

                    if (potentialEdgeConnector.x >= firstVertexInHolePosition.x && potentialEdgeConnector.x < edgeConnector.x)
                    {
                        injectionPointID = polygonVertexID;
                        edgeConnector = potentialEdgeConnector;
                    }
                }
            }

            //Inject the hole with a coincident edge defined by the two vertices
            List<Vector3> toInject = new List<Vector3>();
            toInject.Add(edgeConnector);
            for (int holeIDOffset = 0; holeIDOffset < orderedHoles[holeID].Length + 1; holeIDOffset++)
            {
                int actualID = (firstVertexInHoleID + holeIDOffset) % orderedHoles[holeID].Length;
                toInject.Add(orderedHoles[holeID][actualID]);
            }
            toInject.Add(edgeConnector);

            processedPolygon.InsertRange((injectionPointID + 1) % processedPolygon.Count, toInject);
        }

        //Triangulate the resulting polygon
        Vector3[] polygon = processedPolygon.ToArray();

        List<Vector3> finalVertices = new List<Vector3>();
        List<int> finalTriangles = new List<int>();

        //Add the initial edges
        List<int> edgeIDs = new List<int>();
        for (int i = 0; i < polygon.Length; i++)
        {
            edgeIDs.Add(i);
        }


        for (int i = 0; i < polygon.Length; i++)
        {
            // //TEST
            // Vector3 tcP = polygon[i];
            // Vector3 tnP = polygon[(i + 1) % polygon.Length];

            // Debug.DrawLine(tcP + Vector3.up * 0.1f * i, tnP + Vector3.up * 0.1f * i, Color.red, Mathf.Infinity, false);
            // //TEST
            for (int edgeID = 0; edgeID < edgeIDs.Count; edgeID++)
            {
                int currentPointID = edgeIDs[edgeID];
                Vector3 currentPoint = polygon[currentPointID];

                int previousPointID = edgeIDs[(edgeID + edgeIDs.Count - 1) % edgeIDs.Count];
                Vector3 previousPointRelative = polygon[previousPointID] - currentPoint;

                int nextPointID = edgeIDs[(edgeID + 1) % edgeIDs.Count];
                Vector3 nextPointRelative = polygon[nextPointID] - currentPoint;

                Vector3 previousPointNormal = new Vector3(-previousPointRelative.z, 0, previousPointRelative.x).normalized;
                Vector3 nextPointNormal = new Vector3(nextPointRelative.z, 0, -nextPointRelative.x).normalized;


                //Determine whether this edge is convex
                if (Vector3.Distance(nextPointRelative, previousPointNormal) < Vector3.Distance(nextPointRelative, nextPointNormal))
                {
                    bool intersected = false;
                    //Determine whether this triangle has any edges intersecting into it
                    foreach (int potentialIntersector in edgeIDs)
                    {
                        //If the potential intersector point is not one of the three points of the triangle
                        if (potentialIntersector != currentPointID && potentialIntersector != previousPointID && potentialIntersector != nextPointID)
                        {
                            //Check if its inside the triangle - PLANE TEST
                            Vector3 intersectorRelativePosition = polygon[potentialIntersector] - currentPoint;

                            bool b1, b2, b3;
                            b1 = TriangleSign(intersectorRelativePosition, Vector3.zero, nextPointRelative) < 0.0f;
                            b2 = TriangleSign(intersectorRelativePosition, nextPointRelative, previousPointRelative) < 0.0f;
                            b3 = TriangleSign(intersectorRelativePosition, previousPointRelative, Vector3.zero) < 0.0f;

                            if ((b1 == b2) && (b2 == b3))
                            {
                                intersected = true;
                                break;
                            }
                        }
                    }

                    if (!intersected)
                    {
                        finalVertices.Add(previousPointRelative + currentPoint);
                        finalVertices.Add(currentPoint);
                        finalVertices.Add(nextPointRelative + currentPoint);

                        finalTriangles.Add(finalVertices.Count - 3);
                        finalTriangles.Add(finalVertices.Count - 2);
                        finalTriangles.Add(finalVertices.Count - 1);

                        edgeIDs.RemoveAt(edgeID);
                        break;
                    }
                }
            }
        }



        //Add this polygon into the drawer
        Mesh drawerFlatpanelMesh = new Mesh();
        drawerFlatpanelMesh.vertices = finalVertices.ToArray();
        drawerFlatpanelMesh.triangles = finalTriangles.ToArray();
        drawerFlatpanelMesh.RecalculateNormals();
        shipDrawer.flatpanelMesh.mesh = drawerFlatpanelMesh;
    }

    float TriangleSign(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p1.x - p3.x) * (p2.z - p3.z) - (p2.x - p3.x) * (p1.z - p3.z);
    }

    struct AttachmentPoint
    {
        public Vector2 position;
        public Vector2[] quadrantSizeLimits;

        public AttachmentPoint(Vector2 position, Vector2[] quadrantSizeLimits)
        {
            this.position = position;
            this.quadrantSizeLimits = quadrantSizeLimits;
        }
    }

    struct NextStepCandidate
    {
        public int groupID;
        public Vector2 position;
        public bool vertical;
        public Vector2 wholeFootprint;
        public float balance;

        public NextStepCandidate(int groupID, Vector2 position, bool vertical, Vector2 wholeFootprint, float balance)
        {
            this.groupID = groupID;
            this.position = position;
            this.vertical = vertical;
            this.wholeFootprint = wholeFootprint;
            this.balance = balance;
        }
    }

    void ArrangeShipGroupsOnSquarePlane()
    {
        Vector2 footprint = Vector2.zero;
        List<int> addedGroupIDs = new List<int>();

        List<AttachmentPoint> attachmentPoints = new List<AttachmentPoint>();
        attachmentPoints.Add(new AttachmentPoint(Vector2.zero, new Vector2[] { Vector2.one * Mathf.Infinity, Vector2.one * Mathf.Infinity, Vector2.one * Mathf.Infinity, Vector2.one * Mathf.Infinity }));

        NextStepCandidate bestCandidate = new NextStepCandidate(0, Vector2.zero, false, Vector2.one, Mathf.Infinity);
        for (int i = 0; i < groups.Length; i++)
        {
            //DETERMINE BEST CANDIDATE STEP
            for (int candidateGroupID = 0; candidateGroupID < groups.Length; candidateGroupID++)
            {
                if (!addedGroupIDs.Contains(candidateGroupID))
                {
                    for (int verticalIndex = 0; verticalIndex < 2; verticalIndex++)
                    {
                        //CALCULATE CORNER POSITIONS
                        Vector2[] groupCorners = verticalIndex == 1 ? groups[candidateGroupID].verticalCorners : groups[candidateGroupID].horizontalCorners;
                        //CALCULATE FOOTPRINT
                        Vector2 size = groupCorners[2] - groupCorners[1];

                        //TRY ALL POSITIONING OPTIONS
                        foreach (AttachmentPoint attachmentPoint in attachmentPoints)
                        {
                            for (int examinedCorner = 0; examinedCorner < 4; examinedCorner++)
                            {
                                Vector2 sizeLimitation = attachmentPoint.quadrantSizeLimits[examinedCorner];

                                if (size.x <= sizeLimitation.x && size.y <= sizeLimitation.y)
                                {
                                    NextStepCandidate newCandidate;
                                    newCandidate.groupID = candidateGroupID;
                                    newCandidate.position = attachmentPoint.position - groupCorners[examinedCorner];
                                    newCandidate.vertical = verticalIndex == 1;

                                    Vector4 boundaries = Vector4.zero;
                                    for (int newCandidateCornerID = 0; newCandidateCornerID < groupCorners.Length; newCandidateCornerID++)
                                    {
                                        boundaries = PushBoundaries(boundaries, newCandidate.position + groupCorners[newCandidateCornerID]);
                                    }

                                    foreach (int addedGroupID in addedGroupIDs)
                                    {
                                        Vector2[] addedGroupCorners = groups[addedGroupID].Corners;
                                        for (int cornerID = 0; cornerID < 4; cornerID++)
                                        {
                                            boundaries = PushBoundaries(boundaries, groups[addedGroupID].rect.position + addedGroupCorners[cornerID]);
                                        }
                                    }

                                    newCandidate.wholeFootprint = new Vector2(Mathf.Abs(boundaries.x - boundaries.z), Mathf.Abs(boundaries.y - boundaries.w));
                                    newCandidate.balance = newCandidate.wholeFootprint.x / newCandidate.wholeFootprint.y;

                                    if (newCandidate.wholeFootprint.x < 0.9f * shipDrawerFlatSize && newCandidate.wholeFootprint.y < 0.9f * shipDrawerFlatSize)
                                    {
                                        if (Mathf.Abs(1 - newCandidate.balance) < Mathf.Abs(1 - bestCandidate.balance))
                                        {
                                            bestCandidate = newCandidate;
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }

            //APPLY BEST STEP
            addedGroupIDs.Add(bestCandidate.groupID);
            ShipRectangleGroup positionedGroup = groups[bestCandidate.groupID];
            positionedGroup.rect.position = bestCandidate.position;
            positionedGroup.vertical = bestCandidate.vertical;
            groups[bestCandidate.groupID] = positionedGroup;
            footprint = bestCandidate.wholeFootprint;

            // Debug.Log("Cycle: " + i);
            // Debug.Log("Group ID: " + bestCandidate.groupID);
            // Debug.Log("Position: " + positionedGroup.rect.position);
            // Debug.Log("Vertical: " + positionedGroup.vertical);

            bestCandidate = new NextStepCandidate(0, Vector2.zero, false, Vector2.one, Mathf.Infinity);



            //RECALCULATE ATTACHMENT POINTS
            attachmentPoints = new List<AttachmentPoint>();
            foreach (int groupID in addedGroupIDs)
            {
                ShipRectangleGroup managedGroup = groups[groupID];
                for (int cornerIndex = 0; cornerIndex < 4; cornerIndex++)
                {
                    AttachmentPoint potentialAttachmentPoint = new AttachmentPoint(Vector2.zero, new Vector2[4]);
                    potentialAttachmentPoint.position = managedGroup.rect.position + managedGroup.Corners[cornerIndex];

                    Vector2[] calculatedQuadrants = new Vector2[4];
                    for (int quadrantID = 0; quadrantID < 4; quadrantID++)
                    {
                        Vector2 quadrantDirectional = new Vector2((quadrantID == 0 || quadrantID == 1) ? 1 : -1, (quadrantID == 1 || quadrantID == 3) ? 1 : -1);
                        Vector2 size = Vector2.one * Mathf.Infinity;

                        foreach (int potentialIntersectorIndex in addedGroupIDs)
                        {

                            ShipRectangleGroup potentialIntersector = groups[potentialIntersectorIndex];
                            for (int intersectorCornerIndex = 0; intersectorCornerIndex < 4; intersectorCornerIndex++)
                            {
                                Vector2 cornerGlobalPosition = potentialIntersector.rect.position + potentialIntersector.Corners[intersectorCornerIndex];
                                Vector2 cornerPositionRelativeToAttachmentPoint = cornerGlobalPosition - potentialAttachmentPoint.position;
                                Vector2 cornerNormalizedQuadrantPosition = Vector2.Scale(cornerPositionRelativeToAttachmentPoint, quadrantDirectional);

                                if (cornerNormalizedQuadrantPosition.x >= -0.000015f && cornerNormalizedQuadrantPosition.y >= -0.000015f && cornerNormalizedQuadrantPosition.x <= size.x && cornerNormalizedQuadrantPosition.y <= size.y)
                                {
                                    Vector2 oppositeCornerNormalizedQuadrantPosition = Vector2.Scale(potentialIntersector.rect.position - potentialIntersector.Corners[intersectorCornerIndex] - potentialAttachmentPoint.position, quadrantDirectional);
                                    Vector2 sides = oppositeCornerNormalizedQuadrantPosition - cornerNormalizedQuadrantPosition;
                                    sides = sides - Vector2.Scale(new Vector2(Mathf.Clamp(-oppositeCornerNormalizedQuadrantPosition.x, 0, Mathf.Abs(sides.x)), Mathf.Clamp(-oppositeCornerNormalizedQuadrantPosition.y, 0, Mathf.Abs(sides.y))), new Vector2(Mathf.Sign(sides.x), Mathf.Sign(sides.y)));

                                    if (sides.y != 0 && sides.x != 0)
                                    {
                                        if (sides.x > 0)
                                        {
                                            size.x = cornerNormalizedQuadrantPosition.x < size.x ? cornerNormalizedQuadrantPosition.x : size.x;
                                        }
                                        else
                                        {
                                            size.y = cornerNormalizedQuadrantPosition.y < size.y ? cornerNormalizedQuadrantPosition.y : size.y;
                                        }
                                    }
                                }
                            }
                        }

                        //TEST
                        // Debug.Log("Cycle: " + i);
                        // Debug.Log("Group: " + groupID + " Corner: " + cornerIndex + " Quadrant: " + quadrantID);
                        // Debug.Log("Size: " + size);
                        //TEST
                        // if (groupID == 3 && cornerIndex == 2 && quadrantID == 2)
                        // {
                        //     Debug.Log("Conflicting Quadrant Size: " + size);
                        // }

                        calculatedQuadrants[quadrantID] = size;
                    }

                    potentialAttachmentPoint.quadrantSizeLimits = calculatedQuadrants;
                    attachmentPoints.Add(potentialAttachmentPoint);
                }
            }
        }


        //NORMALIZE RECTANGLE POSITIONS
        Vector2 topRightCorner = Vector2.one * Mathf.NegativeInfinity;
        Vector2 bottomLeftCorner = Vector2.one * Mathf.Infinity;
        for (int i = 0; i < groups.Length; i++)
        {
            Vector2[] corners = groups[i].Corners;
            Vector2 groupTopRightCorner = corners[2] + groups[i].rect.position;
            Vector2 groupBottomLeftCorner = corners[1] + groups[i].rect.position;

            topRightCorner.x = groupTopRightCorner.x > topRightCorner.x ? groupTopRightCorner.x : topRightCorner.x;
            topRightCorner.y = groupTopRightCorner.y > topRightCorner.y ? groupTopRightCorner.y : topRightCorner.y;

            bottomLeftCorner.x = groupBottomLeftCorner.x < bottomLeftCorner.x ? groupBottomLeftCorner.x : bottomLeftCorner.x;
            bottomLeftCorner.y = groupBottomLeftCorner.y < bottomLeftCorner.y ? groupBottomLeftCorner.y : bottomLeftCorner.y;
        }

        Vector2 positionAdjustment = (bottomLeftCorner + topRightCorner) / 2.0f;

        for (int i = 0; i < groups.Length; i++)
        {
            groups[i].rect.position -= positionAdjustment;
        }


        //POSITION SHIPS ON RECTANGLE GROUPS
        for (int groupIndex = 0; groupIndex < groups.Length; groupIndex++)
        {
            ShipRectangleGroup group = groups[groupIndex];
            float shipSpacing = group.rect.width / group.ships.Length * 0.8f;
            float reservedSpace = shipSpacing * (group.ships.Length - 1);
            Vector3 startingPosition = new Vector3(group.rect.x, 0, group.rect.y) + (group.vertical ? Vector3.forward : Vector3.right) * (reservedSpace / 2.0f);
            Vector3 positionStep = (group.vertical ? Vector3.back : Vector3.left) * shipSpacing;
            for (int shipIndex = 0; shipIndex < group.ships.Length; shipIndex++)
            {
                Ship ship = group.ships[shipIndex];
                ship.transform.localPosition = startingPosition + positionStep * shipIndex;
                ship.transform.localRotation = new Quaternion(0, 1, 0, group.vertical ? 1 : 0);

                ShipInfo info = allShips[ship];
                info.localDrawerPosition = ship.transform.localPosition;
                info.localDrawerRotation = ship.transform.localRotation;
                allShips[ship] = info;
            }

            // GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // tmp.transform.localScale = new Vector3(group.vertical ? group.rect.height : group.rect.width, 0.1f, group.vertical ? group.rect.width : group.rect.height);
            // tmp.transform.position = new Vector3(group.rect.x, 0, group.rect.y);
        }
    }

    Vector4 PushBoundaries(Vector4 boundaries, Vector2 position)
    {
        if (position.x > boundaries.x)
        {
            boundaries.x = position.x;
        }
        else if (position.x < boundaries.z)
        {
            boundaries.z = position.x;
        }

        if (position.y > boundaries.y)
        {
            boundaries.y = position.y;
        }
        else if (position.y < boundaries.w)
        {
            boundaries.w = position.y;
        }

        return boundaries;
    }

    Vector2[] CalculateCorners(Rect rect, bool invert)
    {
        float CWC = rect.width / 2.0f; //CORNER WIDTH COMPONENT
        float CHC = rect.height / 2.0f; //CORNER HEIGHT COMPONENT

        Vector2[] corners = new Vector2[] { new Vector2(-CWC, CHC), new Vector2(-CWC, -CHC), new Vector2(CWC, CHC), new Vector2(CWC, -CHC) };
        if (invert)
        {
            for (int groupCornerID = 0; groupCornerID < corners.Length; groupCornerID++)
            {
                Vector2 initialState = corners[groupCornerID];
                corners[groupCornerID] = new Vector2(Mathf.Abs(initialState.y) * Mathf.Sign(initialState.x), Mathf.Abs(initialState.x) * Mathf.Sign(initialState.y));
            }
        }

        return corners;
    }
}
