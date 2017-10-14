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
        groups = new ShipRectangleGroup[5];

        int groupIndex = 0;

        List<Ship> toAdd = new List<Ship>();
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
                groups[groupIndex].ships = toAdd.ToArray();
                toAdd = new List<Ship>();
                toAdd.Add(ship);
                groupIndex++;
            }
        }
        groups[groupIndex].ships = toAdd.ToArray();

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
        public Vector4 sizeLimits;

        public AttachmentPoint(Vector2 position, Vector4 sizeLimits)
        {
            this.position = position;
            this.sizeLimits = sizeLimits;
        }
    }

    struct NextStepCandidate
    {
        public int groupID;
        public Vector2 position;
        public bool vertical;
        public Vector2 wholeFootprint;
        public float balanceDisruption;

        public NextStepCandidate(int groupID, Vector2 position, bool vertical, Vector2 wholeFootprint, float balanceDisruption)
        {
            this.groupID = groupID;
            this.position = position;
            this.vertical = vertical;
            this.wholeFootprint = wholeFootprint;
            this.balanceDisruption = balanceDisruption;
        }
    }

    void ArrangeShipGroupsOnSquarePlane()
    {
        float currentBalance = 1;
        Vector2 footprint = Vector2.zero;
        List<int> addedGroupIDs = new List<int>();

        List<AttachmentPoint> attachmentPoints = new List<AttachmentPoint>();
        attachmentPoints.Add(new AttachmentPoint(Vector2.zero, Vector4.one * Mathf.Infinity));

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
                                        sizeLimitation = new Vector2(attachmentPoint.sizeLimits.x, attachmentPoint.sizeLimits.w);
                                        break;
                                    case 1:
                                        sizeLimitation = new Vector2(attachmentPoint.sizeLimits.x, attachmentPoint.sizeLimits.y);
                                        break;
                                    case 2:
                                        sizeLimitation = new Vector2(attachmentPoint.sizeLimits.z, attachmentPoint.sizeLimits.w);
                                        break;
                                    case 3:
                                        sizeLimitation = new Vector2(attachmentPoint.sizeLimits.z, attachmentPoint.sizeLimits.y);
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
                                    newCandidate.balanceDisruption = Mathf.Abs(currentBalance - newCandidate.wholeFootprint.x / newCandidate.wholeFootprint.y);

                                    if (newCandidate.balanceDisruption < bestCandidate.balanceDisruption)
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
            currentBalance = bestCandidate.wholeFootprint.x / bestCandidate.wholeFootprint.y;
            footprint = bestCandidate.wholeFootprint;

            //RECALCULATE ATTACHMENT POINTS
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
