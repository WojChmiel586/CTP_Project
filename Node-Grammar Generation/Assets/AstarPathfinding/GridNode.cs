using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridNode
{
    public int gridX;
    public int gridY;

    public bool isWall;
    public Vector3 position;

    public GridNode parent;

    public int gCost;
    public int hCost;

    public int fCost { get { return gCost + hCost; } }

    public GridNode(bool _isWall, Vector3 a_Pos, int a_gridX, int a_gridY)
    {
        isWall = _isWall;
        position = a_Pos;
        gridX = a_gridX;
        gridY = a_gridY;
    }
}
