using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineMarker_UIAgent : UIAgent
{
    public BoardViewUI owner;
    public Material lineMaterial;
    public float lineWidth;
    public float extensionTime;
    public float extensionMaxSpeed;
    public Vector3[] testPoints; //Intended length is 7

    // void Awake()
    // {
    //     Set(testPoints, new int[][] { new int[] { 1, 2 }, new int[] { 3, 4 }, new int[] { 5, 6 }, new int[0], new int[0], new int[0], new int[0] });
    //     SetState(UIState.ENABLING);
    // }

    protected override void SetState(UIState state)
    {
        base.SetState(state);
        if (state == UIState.DISABLED)
        {
            owner.dynamicUIAgents.Remove(this);
            Destroy(gameObject);
        }
    }

    float lastExtensionProgress;
    protected override void Update()
    {
        base.Update();
        if (state == UIState.ENABLING || state == UIState.DISABLING)
        {
            extensionProgress = Mathf.SmoothDamp(extensionProgress, (int)state < 2 ? 0.0f : 1.0f, ref extensionSpeed, extensionTime, extensionMaxSpeed);
            if (Mathf.Abs(extensionProgress - (int)state < 2 ? 0.0f : 1.0f) < 0.01f)
            {
                State = (int)state < 2 ? UIState.DISABLED : UIState.ENABLED;
            }
        }

        if (extensionProgress != lastExtensionProgress)
        {
            float extendedDistance = longestBranchLength * extensionProgress;
            foreach (Branch branch in branches)
            {
                for (int node = 0; node < branch.nodes.Length; node++)
                {
                    branch.renderer.gameObject.SetActive(true);
                    Node actualNode = nodes[branch.nodes[node]];
                    Node inputNode = nodes[actualNode.inputNode];
                    if (extendedDistance > inputNode.distanceFromFirstNode)
                    {
                        if (extendedDistance > actualNode.distanceFromFirstNode)
                        {
                            branch.renderer.SetPosition(node, actualNode.position);
                        }
                        else
                        {
                            float t = (extendedDistance - inputNode.distanceFromFirstNode) / (actualNode.distanceFromFirstNode - inputNode.distanceFromFirstNode);
                            Vector3 endPosition = Vector3.Lerp(inputNode.position, actualNode.position, t);
                            for (int i = node; i < branch.nodes.Length; i++)
                            {
                                branch.renderer.SetPosition(i, endPosition);
                            }
                            break;
                        }
                    }
                    else
                    {
                        branch.renderer.gameObject.SetActive(false);
                        break;
                    }
                }
            }

            lastExtensionProgress = extensionProgress;
        }
    }

    float longestBranchLength;
    float extensionProgress;
    float extensionSpeed;
    Node[] nodes;
    struct Node
    {
        public Vector3 position;
        public int inputNode;
        public int[] connections;
        public float distanceFromFirstNode;
        public Node(Vector3 position, int[] connections)
        {
            this.position = position;
            this.connections = connections;
            this.inputNode = 0;
            this.distanceFromFirstNode = 0;
        }
    }

    struct Branch
    {
        public int[] nodes;
        public LineRenderer renderer;
        public Branch(int[] nodes, Material renderMaterial, float lineWidth, Transform lineParent)
        {
            this.nodes = nodes;
            renderer = new GameObject("Branch Line").AddComponent<LineRenderer>();
            renderer.transform.SetParent(lineParent, false);
            renderer.material = renderMaterial;
            renderer.widthMultiplier = lineWidth;
            renderer.numCapVertices = 8;
            renderer.numCornerVertices = 8;
            renderer.useWorldSpace = false;

            renderer.positionCount = nodes.Length;
        }
    }
    List<Branch> branches;
    public void Set(Vector3[] nodes, int[][] connections)
    {
        //SET UP NODES
        this.nodes = new Node[nodes.Length];
        for (int i = 0; i < nodes.Length; i++)
        {
            this.nodes[i] = new Node(nodes[i], connections[i]);
        }

        //SET UP BRANCHES
        if (branches != null)
        {
            branches.ForEach(x => Destroy(x.renderer.gameObject));
        }
        branches = new List<Branch>();
        longestBranchLength = 0;
        StartBranchingFromNode(0, 0);

        //TEST
        // foreach (Branch branch in branches)
        // {
        //     for (int i = 0; i < branch.nodes.Length; i++)
        //     {
        //         branch.renderer.SetPosition(i, this.nodes[branch.nodes[i]].position);
        //     }
        // }
    }

    void StartBranchingFromNode(int node, int inputNode)
    {
        List<int> newBranchNodes = new List<int>();
        if (node != inputNode)
        {
            newBranchNodes.Add(inputNode);
        }
        newBranchNodes.Add(node);

        int currentNode = node;
        nodes[currentNode].inputNode = inputNode;
        int[] currentNodeConnections = nodes[currentNode].connections;

        //While the branch leads on expand it
        while (currentNodeConnections.Length > 0)
        {
            //Measure distances of connections
            for (int i = 0; i < currentNodeConnections.Length; i++)
            {
                int connectedNode = currentNodeConnections[i];

                nodes[connectedNode].distanceFromFirstNode = nodes[currentNode].distanceFromFirstNode + Vector3.Distance(nodes[connectedNode].position, nodes[currentNode].position);
            }

            //Create new branches on any additional connections
            for (int i = 1; i < currentNodeConnections.Length; i++)
            {
                StartBranchingFromNode(currentNodeConnections[i], currentNode);
            }

            //Continue this branch
            int nextBranchNode = currentNodeConnections[0];
            newBranchNodes.Add(nextBranchNode);
            nodes[nextBranchNode].inputNode = currentNode;

            if (nodes[nextBranchNode].distanceFromFirstNode > longestBranchLength)
            {
                longestBranchLength = nodes[nextBranchNode].distanceFromFirstNode;
            }

            currentNode = nextBranchNode;
            currentNodeConnections = nodes[currentNode].connections;
        }

        branches.Add(new Branch(newBranchNodes.ToArray(), lineMaterial, lineWidth, transform));
    }
}
