
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MovingEntities
{
    public string enemyName;
    public int playerDamage;
    public int maxHealth;
    public int detectionRange = 10;
    public HealthbarBehaviour healthbar;
    [Space]

    [Header("Loot Prefabs")]
    public GameObject arrowPrefab;
    public GameObject coinPrefab;

    private int currentHealth;
    private bool enemyAlerted = false;
    private Transform target;
    private bool skipMove;
    private bool pathFound = false;
    private int pathIndex = 0;
    private Sprite defaultSprite;
    [SerializeField] private Sprite targetedSprite;
    // Start is called before the first frame update
    protected override void Start()
    {
        currentHealth = maxHealth;
        target = GameObject.FindGameObjectWithTag("Player").transform;
        GameManager.instance.AddEnemyToList(this);
        base.Start();
        defaultSprite = spriteRenderer.sprite;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentNode == null)
        {
            currentNode = pathfinding.worldToNode(transform.position);
        }
    }


    public void TakeDamage(int loss)
    {
        currentHealth -= loss;
        healthbar.SetHealth(currentHealth, maxHealth);
        isDead();
    }

    protected override void AttemptMove(int dirX, int dirY)
    {
        if (skipMove)
        {
            skipMove = false;
            return;
        }
        if (pathfinding.CheckIfPlayer(currentNode, new Vector2(dirX,dirY)))
        {
            PlayerControls player = target.GetComponent<PlayerControls>();
            player.LoseHealth(playerDamage);
            skipMove = true;
            return;
        }
        pathIndex++;
        base.AttemptMove(dirX, dirY);

        skipMove = true;
    }

    public void MoveEnemy()
    {
        int dirX = 0;
        int dirY = 0;


        if (PlayerInRange())
        {
            enemyAlerted = true;
            if (!pathFound || pathfinding.worldToNode(transform.position) != pathfinding.FinalPath[pathfinding.FinalPath.Count - 1])
            {
                pathfinding.FindPath(transform.position, target.transform.position);
                pathFound = true;
                pathIndex = 0;
            }
            dirX = pathfinding.FinalPath[pathIndex].gridX - currentNode.gridX;
            dirY = pathfinding.FinalPath[pathIndex].gridY - currentNode.gridY;
        }
        //if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)
        //{
        //    dirY = target.position.y > transform.position.y ? 1 : -1;
        //}
        //else
        //{
        //    dirX = target.position.x > transform.position.x ? 1 : -1;
        //}

        AttemptMove(dirX, dirY);
    }


    private void isDead()
    {
        if (currentHealth <= 0)
        {
            DropLoot();
            Debug.Log("Player killed " + enemyName);
            GameManager.instance.RemoveEnemyFromList(this);
            gameObject.SetActive(false);
        }
    }

    private void DropLoot()
    {
        int lootChance = Random.Range(0, 10);
        if (lootChance >= 7)
        {
            int chance = Random.Range(0, 10);
            if (chance < 5)
            {
                Instantiate(arrowPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                Instantiate(coinPrefab, transform.position, Quaternion.identity);
            }

        }
    }

    public void isBeingTargeted(bool targeted)
    {
        if (targeted)
        {
            spriteRenderer.sprite = targetedSprite;
        }
        else
        {
            spriteRenderer.sprite = defaultSprite;
        }
    }

    private bool PlayerInRange()
    {
        int adjustedRange = enemyAlerted ? detectionRange * 2 : detectionRange;
        if (Vector3.Distance(transform.position,target.transform.position) <= adjustedRange)
        {
            return true;
        }
        return false;
    }

}
