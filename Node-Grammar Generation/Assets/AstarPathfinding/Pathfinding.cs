using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public bool DEBUG_FINDPATH = false;
    [SerializeField]
    AstarGrid grid;
    [SerializeField]
    DungeonController dungeonController;
    public Transform StartPos;
    public Transform TargetPos;
    public List<GridNode> FinalPath;
    // Start is called before the first frame update
    void Start()
    {
        dungeonController = FindObjectOfType<DungeonController>();
        grid = dungeonController.aStarGrid;
    }

    // Update is called once per frame
    void Update()
    {
        if (DEBUG_FINDPATH)
        {
            FindPath(StartPos.position, TargetPos.position);
        }

    }

    public void FindPath(Vector3 a_StartPos, Vector3 a_TargetPos)
    {
        if (grid == null)
        {
            grid = dungeonController.aStarGrid;
        }

        GridNode startNode = worldToNode(a_StartPos);
        GridNode targetNode = worldToNode(a_TargetPos);

        List<GridNode> OpenList = new List<GridNode>();
        HashSet<GridNode> ClosedList = new HashSet<GridNode>();

        OpenList.Add(startNode);

        while(OpenList.Count > 0)
        {
            GridNode CurrentNode = OpenList[0];
            for (int i = 1; i < OpenList.Count; i++)
            {
                if (OpenList[i].fCost < CurrentNode.fCost || OpenList[i].fCost == CurrentNode.fCost && OpenList[i].hCost < CurrentNode.hCost)
                {
                    CurrentNode = OpenList[i];
                }
            }
            OpenList.Remove(CurrentNode);
            ClosedList.Add(CurrentNode);

            if (CurrentNode == targetNode)
            {
                GetFinalPath(startNode, targetNode);
                break;
            }

            foreach (var neigbourNode in grid.GetNeighbouringNodes(CurrentNode))
            {
                if (neigbourNode.isWall || ClosedList.Contains(neigbourNode))
                {
                    continue;
                }
                int MoveCost = CurrentNode.gCost + GetManhattenDistance(CurrentNode, neigbourNode);

                if (MoveCost < neigbourNode.gCost || !OpenList.Contains(neigbourNode))
                {
                    neigbourNode.gCost = MoveCost;
                    neigbourNode.hCost = GetManhattenDistance(neigbourNode, targetNode);
                    neigbourNode.parent = CurrentNode;

                    if (!OpenList.Contains(neigbourNode))
                    {
                        OpenList.Add(neigbourNode);
                    }
                }
            }

        }
    }

    private int GetManhattenDistance(GridNode nodeA, GridNode nodeB)
    {

        int ix = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int iy = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        return ix + iy;
    }

    void GetFinalPath(GridNode a_StartingNode, GridNode a_EndNode)
    {
        List<GridNode> FinalPathReversed = new List<GridNode>();
        GridNode currentNode = a_EndNode;

        while (currentNode != a_StartingNode)
        {
            FinalPathReversed.Add(currentNode);
            currentNode = currentNode.parent;
        }

        FinalPathReversed.Reverse();

        //grid.FinalPath = FinalPathReversed;
        FinalPath = FinalPathReversed;
    }

    public GridNode worldToNode(Vector3 pos)
    {
        GridNode node = dungeonController.worldToNodePos(pos);
        return node;
    }

    public bool CheckIfWall(GridNode currentNode, Vector2Int direction)
    {
        if (grid.grid[currentNode.gridX + direction.x,currentNode.gridY + direction.y].isWall)
        {
            return true;
        }
        return false;
    }

    public bool CheckIfEnemy(GridNode currentNode, Vector2 direction)
    {
        foreach (var enemy in GameManager.instance.enemies)
        {
            if (enemy.currentNode.gridX == currentNode.gridX+direction.x && enemy.currentNode.gridY == currentNode.gridY+direction.y)
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckIfPlayer(GridNode currentNode, Vector2 direction)
    {
        PlayerControls player = GameManager.instance.player;
        if (player.currentNode.gridX == currentNode.gridX + direction.x && player.currentNode.gridY == currentNode.gridY + direction.y)
        {
            return true;
        }
        return false;
    }

    public bool CheckIfValidNode(GridNode currentNode, Vector2 direction)
    {
        Vector2 check = new Vector2(currentNode.gridX + direction.x, currentNode.gridY + direction.y);
        if (check.x >= 0 && check.x < grid.gridWorldSize.x)
        {
            if (check.y >= 0 && check.y < grid.gridWorldSize.y)
            {
                return true;
            }
        }
        return false;
    }
}
