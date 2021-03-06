using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathSpawner : Singleton<PathSpawner>
{
    private SpawnObject pathPiece;

    private List<Node> confirmedPathNodes = new List<Node>();
    private List<Node> pathNodeCandidates = new List<Node>();

    private const int closestPlacementDist = 1;

    Node prevNode = null;

    private Node GetBestNodeForPath(List<Node> nodesToCheck, Vector3 target)
    {
        Node bestNode = null;

        foreach(Node node in nodesToCheck)
        {
            if (node != null)
            {
                float distToTarget = Vector3.Distance(node.pos, target);

                if (bestNode == null || distToTarget < bestNode.distanceToDest)
                {
                    node.distanceToDest = distToTarget;
                    bestNode = node;
                }
            }
        }
        return bestNode;
    }

    private List<Node> GetPathNodeCandidates()
    {
        List<Node> nodeCandidates = new List<Node>();

        Node nodeCandidate;
        nodeCandidate = NodeManager.Instance.GetNodeAtPosition(new Vector3(PathGenerator.Instance.currentNode.pos.x, 0, PathGenerator.Instance.currentNode.pos.z + pathPiece.bounds.z));
        if (nodeCandidate != null)
        {
            nodeCandidates.Add(nodeCandidate);
        }

        nodeCandidate = NodeManager.Instance.GetNodeAtPosition(new Vector3(PathGenerator.Instance.currentNode.pos.x + pathPiece.bounds.x, 0, PathGenerator.Instance.currentNode.pos.z));
        if (nodeCandidate != null)
        {
            nodeCandidates.Add(nodeCandidate);
        }

        nodeCandidate = NodeManager.Instance.GetNodeAtPosition(new Vector3(PathGenerator.Instance.currentNode.pos.x, 0, PathGenerator.Instance.currentNode.pos.z - pathPiece.bounds.z));
        if (nodeCandidate != null)
        {
            nodeCandidates.Add(nodeCandidate);
        }

        nodeCandidate = NodeManager.Instance.GetNodeAtPosition(new Vector3(PathGenerator.Instance.currentNode.pos.x - pathPiece.bounds.x, 0, PathGenerator.Instance.currentNode.pos.z));
        if (nodeCandidate != null)
        {
            nodeCandidates.Add(nodeCandidate);
        }

        return nodeCandidates;
    }

    public bool DrawPath(Node destinationNode, SpawnObject selectedPathPiece, Transform parent)
    {
        pathPiece = selectedPathPiece;

        if (PathGenerator.Instance.currentNode.pos == new Vector3(0, 0, 0))
        {
            PathGenerator.Instance.currentNode = destinationNode;
        }

        if(PathGenerator.Instance.currentNode != null && destinationNode != null)
        {
            while (Vector3.Distance(PathGenerator.Instance.currentNode.pos, destinationNode.pos) > closestPlacementDist)
            {
                confirmedPathNodes.Clear();
                pathNodeCandidates.Clear();

                pathNodeCandidates = GetPathNodeCandidates();

                if(pathNodeCandidates == null)
                {
                    break;
                }

                List<Node> pathNodeCadidatesCopy = pathNodeCandidates;
                List<Node> goodNodes = new List<Node>();
                foreach (Node node in pathNodeCadidatesCopy)
                {
                    if (node != null && !node.used && !node.enclosure)
                    {
                        goodNodes.Add(node);
                    }
                }

                Node nextNode = null;
                if (goodNodes != null)
                {
                    nextNode = GetBestNodeForPath(goodNodes, destinationNode.pos);
                }

                if (nextNode != null && Vector3.Distance(nextNode.pos, destinationNode.pos) < Vector3.Distance(PathGenerator.Instance.currentNode.pos, destinationNode.pos))
                {
                    confirmedPathNodes.Add(nextNode);                    

                    PlacePath(confirmedPathNodes, PathGenerator.Instance.currentNode, pathPiece, parent);

                    PathGenerator.Instance.currentNode = nextNode;
                    confirmedPathNodes.Clear();                 
                }
                else
                {
                    break;
                }
            }
        }       
        return true;
    }

    private void PlacePath(List<Node> nodesToPlace, Node prevNode, SpawnObject pathPiece, Transform parent)
    {
        for (int i = 0; i < nodesToPlace.Count; i++)
        {
            // draw path on node
            if(nodesToPlace[i] != null)
            {
                if (prevNode.pos.x > nodesToPlace[i].pos.x || prevNode.pos.x < nodesToPlace[i].pos.x)
                {
                    for (float x = -pathPiece.bounds.x / 2; x < pathPiece.bounds.x / 2; x++)
                    {
                        Instantiate(pathPiece.gameObject, new Vector3(nodesToPlace[i].pos.x + x, nodesToPlace[i].pos.y, nodesToPlace[i].pos.z), Quaternion.Euler(0, 90, 0), parent);

                        for (float n = -pathPiece.bounds.x / 2; n < pathPiece.bounds.x / 2; n++)
                        {
                            NodeManager.Instance.SetNodeUsed(new Vector3(nodesToPlace[i].pos.x, nodesToPlace[i].pos.y, nodesToPlace[i].pos.z + n));
                        }                     
                    }                      
                }
                else
                {
                    Instantiate(pathPiece.gameObject, nodesToPlace[i].pos, Quaternion.Euler(0, 0, 0), parent);

                    for (float n = -pathPiece.bounds.x / 2; n < pathPiece.bounds.x / 2; n++)
                    {
                        NodeManager.Instance.SetNodeUsed(new Vector3(nodesToPlace[i].pos.x + n, nodesToPlace[i].pos.y, nodesToPlace[i].pos.z));
                    }
                }                                            
            }               
        }     
    }
}
