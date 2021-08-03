using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstarGrid : MonoBehaviour
{
    public LayerMask WallMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public float Distance;
    public Vector3 bottomLeft;
    public GridNode[,] grid;
    public List<GridNode> FinalPath;
    public bool showGizmos = false;

    public Vector2 testOffset = Vector2.zero;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    private void Start()
    {

    }


    public void initGrid(int _gridSizeX, int _gridSizeY)
    {
        gridWorldSize = new Vector2(_gridSizeX, _gridSizeY);
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();

    }

    public GridNode NodeFromWorldPosition(Vector3 a_WorldPosition)
    {
        float xpoint = ((a_WorldPosition.x + (gridWorldSize.x) / 2) / (gridWorldSize.x));
        float ypoint = ((a_WorldPosition.y + (gridWorldSize.y) / 2) / (gridWorldSize.y));

        xpoint = Mathf.Clamp01(xpoint);
        ypoint = Mathf.Clamp01(ypoint);

        int x = Mathf.RoundToInt((gridSizeX - 1) * xpoint);
        int y = Mathf.RoundToInt((gridSizeY - 1) * ypoint);

        return grid[x, y];
    }

    void CreateGrid()
    {
        grid = new GridNode[gridSizeX, gridSizeY]; 
        //Vector3 bottomLeft = transform.position - (Vector3.right * gridWorldSize.x / 2 + Vector3.up * gridWorldSize.y / 2);
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = bottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                //bool Wall = true;

                //if (Physics.CheckSphere(worldPoint, nodeRadius, WallMask))
                //{
                //    Wall = false;
                //}

                grid[x, y] = new GridNode(false, worldPoint, x, y);
            }
        }

    }

    public List<GridNode> GetNeighbouringNodes(GridNode a_Node)
    {
        List<GridNode> neighbouringNodes = new List<GridNode>();
        int xCheck;
        int yCheck;

        //Right Side
        xCheck = a_Node.gridX + 1;
        yCheck = a_Node.gridY;
        if (xCheck >= 0 && xCheck < gridSizeX)
        {
            if (yCheck >= 0 && yCheck < gridSizeY)
            {
                neighbouringNodes.Add(grid[xCheck, yCheck]);
            }
        }

        //Left Side
        xCheck = a_Node.gridX - 1;
        yCheck = a_Node.gridY;
        if (xCheck >= 0 && xCheck < gridSizeX)
        {
            if (yCheck >= 0 && yCheck < gridSizeY)
            {
                neighbouringNodes.Add(grid[xCheck, yCheck]);
            }
        }

        //Top Side
        xCheck = a_Node.gridX;
        yCheck = a_Node.gridY + 1;
        if (xCheck >= 0 && xCheck < gridSizeX)
        {
            if (yCheck >= 0 && yCheck < gridSizeY)
            {
                neighbouringNodes.Add(grid[xCheck, yCheck]);
            }
        }

        //Bottom Side
        xCheck = a_Node.gridX;
        yCheck = a_Node.gridY - 1;
        if (xCheck >= 0 && xCheck < gridSizeX)
        {
            if (yCheck >= 0 && yCheck < gridSizeY)
            {
                neighbouringNodes.Add(grid[xCheck, yCheck]);
            }
        }

        return neighbouringNodes;
    }

    private void OnDrawGizmos()
    {
        //Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));

        if (grid != null && showGizmos)
        {
            foreach (GridNode node in grid)
            {
                if (node.isWall)
                {
                    Gizmos.color = Color.yellow;
                }
                else
                {
                    Gizmos.color = new Color(1, 1, 1, 0.25f);
                }

                if (FinalPath != null && FinalPath.Contains(node))
                {
                    Gizmos.color = Color.green;
                }

                Gizmos.DrawCube(node.position, Vector3.one * (nodeDiameter - Distance));
            }
        }
    }
}
