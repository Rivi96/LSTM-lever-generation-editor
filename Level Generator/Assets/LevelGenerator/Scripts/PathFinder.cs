using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public static Node startingNode;
    public static Node targetNode;
    private static List<Node> openNodes = new List<Node>();
    private static List<Node> closedNodes = new List<Node>();
    private static List<GameObject> pathDots = new List<GameObject>();
    private static Color pathColor;

    public static bool StartPathFinding(GameObject startingGameobject, GameObject targetGameobject, Color color)
    {
        openNodes = new List<Node>();
        closedNodes = new List<Node>();
        pathDots = new List<GameObject>();

        startingNode = startingGameobject.GetComponent<Node>();
        targetNode = targetGameobject.GetComponent<Node>();
        pathColor = color;
        foreach (Node node in FindObjectsOfType<Node>())
        {
            node.ClearValues();
        }
        bool result = FindNextPathNode();
        return result;
     }

    private static bool FindNextPathNode()
    {
        openNodes.Add(startingNode);

        while (openNodes.Count > 0)
        {
            Node currentNode = openNodes[0];
            for (int i = 1; i < openNodes.Count; i++)
            {
                if (openNodes[i].costF < currentNode.costF)
                {
                    currentNode = openNodes[i];
                }
            }

            openNodes.Remove(currentNode);
            closedNodes.Add(currentNode);

            if (currentNode.position == targetNode.position)
            {
                FindFinalPath(currentNode);
                return true;
            }

            foreach (Node node in FindNeighbours(currentNode))
            {
                if (!closedNodes.Contains(node))
                {
                    int nextNodeCost = currentNode.costG + 1;
                    if (nextNodeCost < node.costG || node.costF == 0)
                    {
                        node.costG = nextNodeCost;
                        node.costH = CalculateHCost(node);
                        node.costF = node.costG + node.costH;
                        node.parentNode = currentNode;

                        if (!openNodes.Contains(node))
                        {
                            openNodes.Add(node);
                        }
                    }
                }
            }
        }
        return false;
    }

    private static List<Node> FindNeighbours(Node currentNode)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(y) != 2)
                {
                    Node node = FindNode(x + currentNode.position.x, y + currentNode.position.y);
                    if (node != null && node != currentNode && node.isPassable)
                    {
                        neighbours.Add(node);
                    }
                }
            }
        }

        return neighbours;
    }

    public static Node FindNode(float x, float y)
    {  
        RaycastHit hit;
        Node node = null;
        Physics.Raycast(new Vector3(x, y, 1), new Vector3(0, 0, -1), out hit, 1);
        if (hit.collider != null)
        {
            node = hit.collider.transform.gameObject.GetComponent<Node>();
        }

        return node;
    }

    private static int CalculateHCost(Node node)
    {
        int costH = (int)Mathf.Abs(targetNode.position.x - node.position.x) + (int)Mathf.Abs(targetNode.position.y - node.position.y);

        return costH;
    }

    private static void FindFinalPath(Node node)
    {
        DrawPathDot(node.position);
        while (node.parentNode != null)
        {
            node = node.parentNode;
            DrawPathDot(node.position);
        }
    }

    private static void DrawPathDot(Vector2 nodePosition)
    {
        GameObject pathDot = AssetDatabase.LoadAssetAtPath("Assets/LevelGenerator/Prefabs/PathDot.prefab", typeof(GameObject)) as GameObject;
        pathDot = PrefabUtility.InstantiatePrefab(pathDot) as GameObject;
        pathDot.hideFlags = HideFlags.HideInHierarchy;
        pathDot.GetComponent<SpriteRenderer>().color = pathColor;
        pathDot.transform.position = new Vector2(nodePosition.x, nodePosition.y);
        pathDots.Add(pathDot);
    }

    public static void DestroyPathDots()
    {
        foreach (GameObject pathDot in pathDots)
        {
            DestroyImmediate(pathDot);
        }
    }
}