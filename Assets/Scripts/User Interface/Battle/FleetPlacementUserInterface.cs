using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleetPlacementUserInterface : BoardViewUserInterface
{
    public Waypoint cameraWaypoint;
    public GameObject[] defaultShipLoadout;
    public float shipPaletteGroupPadding;
    protected override void ChangeState(UIState state)
    {
        base.ChangeState(state);
        switch (state)
        {
            case UIState.DISABLING:
                SetInteractable(false);
                break;
            case UIState.ENABLING:
                break;
        }
    }

    protected override void DeployWorldElements()
    {
        base.DeployWorldElements();
        cameraWaypoint.transform.position = Battle.main.attacker.boardCameraPoint.transform.position;
        cameraWaypoint.transform.rotation = Battle.main.attacker.boardCameraPoint.transform.rotation;
        CameraControl.GoToWaypoint(cameraWaypoint, MiscellaneousVariables.it.playerCameraTransitionTime);

        ResetWorldSpaceParent();
        MakeShipDrawer();
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
    void MakeShipDrawer()
    {
        List<ShipRectangleGroup> unfinishedGroups = new List<ShipRectangleGroup>();
        List<Ship> toAdd = new List<Ship>();

        ShipRectangleGroup currentGroup = new ShipRectangleGroup();
        for (int i = 0; i < defaultShipLoadout.Length; i++)
        {
            Ship ship = Instantiate(defaultShipLoadout[i]).GetComponent<Ship>();

            ship.owner = Battle.main.attacker;
            ship.transform.SetParent(worldSpaceParent);

            if (toAdd.Count == 0)
            {
                toAdd.Add(ship);
                continue;
            }

            if (toAdd[0].type == ship.type)
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
            groups[i].rect.width = (groups[i].ships[0].transform.localScale.x + shipPaletteGroupPadding) * groups[i].ships.Length + shipPaletteGroupPadding;
            groups[i].horizontalCorners = CalculateCorners(groups[i].rect, false);
            groups[i].verticalCorners = CalculateCorners(groups[i].rect, true);
        }

        ArrangeShipGroupsOnSquarePlane();

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

                                    if (Mathf.Abs(1 - newCandidate.balance) < Mathf.Abs(1 - bestCandidate.balance))
                                    {
                                        bestCandidate = newCandidate;
                                        if (i == 3 && attachmentPoint.position == new Vector2(1.4f, -2.2f))
                                        {
                                            Debug.Log("Conflicting Size Limit: " + sizeLimitation);
                                            Debug.Log("Conflicting Size: " + size);
                                            Debug.Log("Attachment Point Position: " + attachmentPoint.position);
                                            foreach (int testGroupID in addedGroupIDs)
                                            {
                                                Debug.Log(testGroupID);
                                            }
                                            Debug.Log("BEST CANDIDATE GROUP " + candidateGroupID + ": " + attachmentPoint.position + " Corner: " + examinedCorner);
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
                            if (potentialIntersectorIndex == 0 && groupID == 0 && cornerIndex == 3 && quadrantID == 3)
                            {
                                Debug.Log("BREAK");
                            }

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

        for (int groupIndex = 0; groupIndex < groups.Length; groupIndex++)
        {
            ShipRectangleGroup group = groups[groupIndex];
            // Vector3 startingPosition = new Vector3(group.rect.x, 0, group.rect.y) + (group.vertical ? Vector3.forward : Vector3.right) * (group.rect.width / 2.0f - shipPaletteGroupPadding);
            // Vector3 positionStep = (group.vertical ? Vector3.back : Vector3.left) * shipPaletteGroupPadding * 2.0f;
            // for (int shipIndex = 0; shipIndex < group.ships.Length; shipIndex++)
            // {
            //     Ship ship = group.ships[shipIndex];
            //     ship.transform.position = startingPosition + positionStep * shipIndex;
            //     ship.transform.rotation = new Quaternion(0, 1, 0, group.vertical ? 1 : 0);
            // }

            GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tmp.transform.localScale = new Vector3(group.vertical ? group.rect.height : group.rect.width, 0.1f, group.vertical ? group.rect.width : group.rect.height);
            tmp.transform.position = new Vector3(group.rect.x, groupIndex, group.rect.y);
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
                corners[groupCornerID] = new Vector2(initialState.y, initialState.x);
            }
        }

        return corners;
    }
}
