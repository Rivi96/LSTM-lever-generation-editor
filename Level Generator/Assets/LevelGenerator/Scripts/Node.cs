using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Node parentNode;
    public Vector2 position;
    public int costF;
    public int costH;
    public int costG;
    public bool isPassable;

    public void ClearValues()
    {
        position = gameObject.transform.position;

        parentNode = null;
        costF = 0;
        costG = 0;
        costH = 0;
    }
}
