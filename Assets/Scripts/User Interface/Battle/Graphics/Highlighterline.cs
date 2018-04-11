using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using BattleUIAgents.Base;

namespace BattleUIAgents.Agents
{
    public class Highlighterline : WorldBattleUIAgent
    {
        public Material lineMaterial;
        public float lineWidth;
        public float extensionTime;
        public float extensionMaxSpeed;

        float extensionProgress;
        float extensionSpeed;
        protected override void Update()
        {
            base.Update();
            float targetExtension = linked ? 1.0f : 0.0f;
            if (Mathf.Abs(extensionProgress - targetExtension) > 0.01f)
            {
                extensionProgress = Mathf.SmoothDamp(extensionProgress, targetExtension, ref extensionSpeed, extensionTime, extensionMaxSpeed);

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
            }
        }

        float longestBranchLength;
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
        public void Set(Vector3[] nodes, int[][] connections, int startingNode)
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
            StartBranchingFromNode(startingNode, startingNode);
        }

        public void Set(Vector3[] nodes, int[][] connections)
        {
            Set(nodes, connections, 0);
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
}
