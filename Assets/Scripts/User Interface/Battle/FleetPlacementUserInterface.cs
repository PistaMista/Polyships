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
        }

        ArrangeShipGroupsOnSquarePlane();

    }

    struct AttachmentPoint
    {
        public Vector2 position;
        public Vector4 sizeLimitsX;
        public Vector4 sizeLimitsY;

        public AttachmentPoint(Vector2 position, Vector4 sizeLimitsX, Vector4 sizeLimitsY)
        {
            this.position = position;
            this.sizeLimitsX = sizeLimitsX;
            this.sizeLimitsY = sizeLimitsY;
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
        attachmentPoints.Add(new AttachmentPoint(Vector2.zero, Vector4.one * Mathf.Infinity, Vector4.one * Mathf.Infinity));

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
                        Vector2[] groupCorners = CalculateCorners(groups[candidateGroupID].rect, verticalIndex == 1);
                        //CALCULATE FOOTPRINT
                        Vector2 groupFootprint = verticalIndex == 1 ? new Vector2(groups[candidateGroupID].rect.height, groups[candidateGroupID].rect.width) : new Vector2(groups[candidateGroupID].rect.width, groups[candidateGroupID].rect.height);

                        //TRY ALL POSITIONING OPTIONS
                        foreach (AttachmentPoint attachmentPoint in attachmentPoints)
                        {
                            for (int examinedCorner = 0; examinedCorner < 4; examinedCorner++)
                            {
                                Vector2 sizeLimitation = Vector2.zero;
                                switch (examinedCorner)
                                {
                                    case 0:
                                        sizeLimitation = new Vector2(attachmentPoint.sizeLimitsX.x, attachmentPoint.sizeLimitsY.x);
                                        break;
                                    case 1:
                                        sizeLimitation = new Vector2(attachmentPoint.sizeLimitsX.y, attachmentPoint.sizeLimitsY.y);
                                        break;
                                    case 2:
                                        sizeLimitation = new Vector2(attachmentPoint.sizeLimitsX.z, attachmentPoint.sizeLimitsY.z);
                                        break;
                                    case 3:
                                        sizeLimitation = new Vector2(attachmentPoint.sizeLimitsX.w, attachmentPoint.sizeLimitsY.w);
                                        break;
                                }

                                if (groupFootprint.x <= sizeLimitation.x && groupFootprint.y <= sizeLimitation.y)
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
                                        Vector2[] corners = CalculateCorners(groups[addedGroupID].rect, groups[addedGroupID].vertical);
                                        for (int cornerID = 0; cornerID < 4; cornerID++)
                                        {
                                            boundaries = PushBoundaries(boundaries, groups[addedGroupID].rect.position + corners[cornerID]);
                                        }
                                    }

                                    newCandidate.wholeFootprint = new Vector2(Mathf.Abs(boundaries.x - boundaries.z), Mathf.Abs(boundaries.y - boundaries.w));
                                    newCandidate.balance = newCandidate.wholeFootprint.x / newCandidate.wholeFootprint.y;

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

            //APPLY BEST STEP
            addedGroupIDs.Add(bestCandidate.groupID);
            ShipRectangleGroup positionedGroup = groups[bestCandidate.groupID];
            positionedGroup.rect.position = bestCandidate.position;
            positionedGroup.vertical = bestCandidate.vertical;
            groups[bestCandidate.groupID] = positionedGroup;
            footprint = bestCandidate.wholeFootprint;
            bestCandidate = new NextStepCandidate(0, Vector2.zero, false, Vector2.one, Mathf.Infinity);

            //RECALCULATE ATTACHMENT POINTS
            attachmentPoints = new List<AttachmentPoint>();
            foreach (int groupID in addedGroupIDs)
            {
                ShipRectangleGroup managedGroup = groups[groupID];
                Vector2[] corners = CalculateCorners(managedGroup.rect, managedGroup.vertical);
                for (int cornerIndex = 0; cornerIndex < 4; cornerIndex++)
                {
                    AttachmentPoint potentialAttachmentPoint = new AttachmentPoint(Vector2.zero, Vector4.zero, Vector4.zero);
                    potentialAttachmentPoint.position = managedGroup.rect.position + corners[cornerIndex];

                    Vector2[] calculatedQuadrants = new Vector2[4];
                    for (int quadrantID = 0; quadrantID < 4; quadrantID++)
                    {
                        Vector2 quadrantDirectional = new Vector2((quadrantID == 0 || quadrantID == 1) ? 1 : -1, (quadrantID == 1 || quadrantID == 3) ? 1 : -1);
                        Vector2 size = Vector2.one * Mathf.Infinity;

                        foreach (int potentialIntersectorIndex in addedGroupIDs)
                        {
                            ShipRectangleGroup potentialIntersector = groups[potentialIntersectorIndex];
                            Vector2[] intersectorCorners = CalculateCorners(potentialIntersector.rect, potentialIntersector.vertical);

                            for (int intersectorCornerIndex = 0; intersectorCornerIndex < 4; intersectorCornerIndex++)
                            {
                                Vector2 cornerGlobalPosition = potentialIntersector.rect.position + intersectorCorners[intersectorCornerIndex];
                                Vector2 cornerPositionRelativeToAttachmentPoint = cornerGlobalPosition - potentialAttachmentPoint.position;
                                Vector2 cornerNormalizedQuadrantPosition = Vector2.Scale(cornerPositionRelativeToAttachmentPoint, quadrantDirectional);

                                if (cornerNormalizedQuadrantPosition.x >= 0 && cornerNormalizedQuadrantPosition.y >= 0 && cornerNormalizedQuadrantPosition.x < size.x && cornerNormalizedQuadrantPosition.y < size.y)
                                {
                                    Vector2 oppositeCornerNormalizedQuadrantPosition = Vector2.Scale(potentialIntersector.rect.position - intersectorCorners[intersectorCornerIndex] - potentialAttachmentPoint.position, quadrantDirectional);
                                    Vector2 sides = oppositeCornerNormalizedQuadrantPosition - cornerNormalizedQuadrantPosition;
                                    sides = sides + new Vector2(Mathf.Clamp(-oppositeCornerNormalizedQuadrantPosition.x, 0, Mathf.Abs(sides.x)), Mathf.Clamp(-oppositeCornerNormalizedQuadrantPosition.y, 0, Mathf.Abs(sides.y)));

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

                        calculatedQuadrants[quadrantID] = size;
                    }

                    potentialAttachmentPoint.sizeLimitsX = new Vector4(calculatedQuadrants[0].x, calculatedQuadrants[1].x, calculatedQuadrants[2].x, calculatedQuadrants[3].x);
                    potentialAttachmentPoint.sizeLimitsY = new Vector4(calculatedQuadrants[0].y, calculatedQuadrants[1].y, calculatedQuadrants[2].y, calculatedQuadrants[3].y);

                    if (potentialAttachmentPoint.sizeLimitsX != Vector4.zero && potentialAttachmentPoint.sizeLimitsY != Vector4.zero)
                    {
                        attachmentPoints.Add(potentialAttachmentPoint);
                    }
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
