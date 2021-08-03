using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovingEntities : MonoBehaviour
{
    public float moveTime = 0f;
    public LayerMask blockingLayer;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    protected SpriteRenderer spriteRenderer;
    private float inverseMoveTime;

    //A* Grid movement stuff
    protected Pathfinding pathfinding;
    public GridNode currentNode;
    protected AstarGrid levelGrid;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        inverseMoveTime = 1f / moveTime;
        levelGrid = FindObjectOfType<DungeonController>().aStarGrid;
        pathfinding = GetComponent<Pathfinding>();
        pathfinding.StartPos = transform;
        //currentNode = pathfinding.worldToNode(transform.position);
    }

    protected IEnumerator SmoothMovement(Vector3 end)
    {
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;
        while (sqrRemainingDistance > float.Epsilon)
        {
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
            rb2D.MovePosition(newPosition);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }
    }





    protected bool Move(int dirX, int DirY)
    {
        Vector2 start = new Vector2(transform.position.x, transform.position.y);
        //Vector2 start = new Vector2(transform.position.x + 0.5f, transform.position.y + 0.5f);

        Vector2 end = start + new Vector2(dirX, DirY);

        boxCollider.enabled = false;
        //Debug.DrawRay(start + new Vector2(0.5f, 0.5f), new Vector2(dirX, DirY), Color.red, 3f);
        //Debug.Log("raycast vector " + new Vector2(dirX, DirY));
        //hit = Physics2D.Raycast(start + new Vector2(0.5f, 0.5f), new Vector2(dirX, DirY), 0.75f, blockingLayer);

        //hit = Physics2D.BoxCast(start + new Vector2(0.5f, 0.5f), new Vector2(1, 1), 0, new Vector2(dirX, DirY), 0.75f, blockingLayer);
        boxCollider.enabled = true;
        StartCoroutine(SmoothMovement(end));
        currentNode = levelGrid.grid[currentNode.gridX + dirX, currentNode.gridY + DirY];


        return true;
        //Debug.Log(hit.transform.name);
        //Debug.Log("collided with something");
    }
    protected virtual void AttemptMove(int dirX, int dirY)
    {
        bool canMove = Move(dirX, dirY);
        //if (!canMove)
        //{
        //    //OnCantMove(hitComponent);
        //}
    }


    private void OnDrawGizmos()
    {
        
    }

    public void GridMove(Vector2Int moveDir)
    {
        Vector2Int newPos = new Vector2Int(currentNode.gridX + moveDir.x, currentNode.gridY);
    }

    public void UpdateCurrentNode()
    {
        currentNode = pathfinding.worldToNode(transform.position);
    }
}
