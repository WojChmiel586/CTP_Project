using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class PlayerControls : MovingEntities
{
    public enum PlayerArchetypes
    {
        MELEE,
        RANGED,
        MAGE,
        ERROR
    }
    public PlayerArchetypes playerArchetype;
    public float restartLevelDelay = 1f;
    public Slider healthBar;
    public int MaxHealth = 30;
    public int currentHealth = 30;
    public Weapon currentWeapon;
    private bool targetingAttack;
    public PlayerInventory inventory = new PlayerInventory();
    public Weapon melee;
    public Weapon ranged;
    public SpriteRenderer playerSprite;
    public Sprite meleeSprite;
    public Sprite rangedSprite;
    public TMP_Text currentWeaponPrompt;

    private List<Enemy> enemiesInRange;

    //targeting attacks variables
    private int targetingIndex = 0;
    private Enemy currentlySelectedEnemy = null;


    // Start is called before the first frame update
    protected override void Start()
    {
        healthBar = GameObject.Find("HealthBar").GetComponent<Slider>();
        currentHealth = GameManager.instance.playersCurrentHealth;
        healthBar.maxValue = MaxHealth;
        healthBar.value = currentHealth;
        enemiesInRange = new List<Enemy>();
        base.Start();
        switch (playerArchetype)
        {
            case PlayerArchetypes.MELEE:

                inventory.ArrowCount = 4;
                playerSprite.sprite = meleeSprite;

                break;
            case PlayerArchetypes.RANGED:
                playerSprite.sprite = rangedSprite;
                Weapon dagger = Weapon.CreateInstance("Weapon") as Weapon;
                dagger.weaponName = "Dagger";
                dagger.weaponType = Weapon.WeaponType.MELEE;
                dagger.meleeDamage = 2;
                dagger.numberOfAttacks = 1;
                dagger.range = 0;
                melee = dagger;

                Weapon crossbow = Weapon.CreateInstance("Weapon") as Weapon;
                crossbow.weaponName = "Crossbow";
                crossbow.weaponType = Weapon.WeaponType.RANGED;
                crossbow.meleeDamage = 1;
                crossbow.numberOfAttacks = 1;
                crossbow.range = 8;
                crossbow.rangedDamage = 10;
                ranged = crossbow;
                currentWeapon = melee;
                inventory.ArrowCount = 20;

                break;
            case PlayerArchetypes.MAGE:
                //not working
                break;
            case PlayerArchetypes.ERROR:
                break;
            default:
                break;
        }
    }

    private void OnDisable()
    {
        GameManager.instance.playersCurrentHealth = currentHealth;

    }

    private void CheckIfGameOver()
    {
        if (currentHealth <= 0)
        {
            GameManager.instance.GameOver();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.playersTurn)
            return;

        if (currentNode == null)
        {
            currentNode = pathfinding.worldToNode(transform.position);
        }
        CheckIfGameOver();
        if (currentWeaponPrompt == null)
        {
            currentWeaponPrompt = GameObject.FindGameObjectWithTag("CurrentWeapon").GetComponent<TMP_Text>();
        }
        currentWeaponPrompt.text = currentWeapon.weaponName;
        //Targeting logic
        if (targetingAttack)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                currentlySelectedEnemy.isBeingTargeted(false);
                targetingIndex--;
                if (targetingIndex < 0)
                {
                    targetingIndex = enemiesInRange.Count - 1;
                }
            }

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                currentlySelectedEnemy.isBeingTargeted(false);
                targetingIndex++;
                if (targetingIndex > enemiesInRange.Count - 1)
                {
                    targetingIndex = 0;
                }
            }
            currentlySelectedEnemy = enemiesInRange[targetingIndex];
            currentlySelectedEnemy.isBeingTargeted(true);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Attack();
            }
        }


        //Movement logic
        else
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                //currentWeapon = melee;
                currentWeapon = currentWeapon == melee ? ranged : melee;
                GameManager.instance.playersTurn = false;
            }
            //if (Input.GetKeyDown(KeyCode.Alpha2))
            //{
            //    currentWeapon = ranged;
            //    GameManager.instance.playersTurn = false;
            //}
            if (Input.GetKeyDown(KeyCode.X))
            {
                AttemptMove(0, 0);
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                GetTargetsInRange();
            }

            int moveX = (int)Input.GetAxisRaw("Horizontal");
            int moveY = (int)Input.GetAxisRaw("Vertical");

            if (moveX != 0)
            {
                moveY = 0;
            }

            if (moveX != 0 || moveY != 0)
            {
                AttemptMove(moveX, moveY);
            }
        }

    }

    private void Attack()
    {
        switch (currentWeapon.weaponType)
        {
            case Weapon.WeaponType.MELEE:
                for (int i = 0; i < currentWeapon.numberOfAttacks; i++)
                {
                    currentlySelectedEnemy.TakeDamage(currentWeapon.meleeDamage);
                }

                break;


            case Weapon.WeaponType.RANGED:
                for (int i = 0; i < currentWeapon.numberOfAttacks; i++)
                {
                    currentlySelectedEnemy.TakeDamage(currentWeapon.rangedDamage);
                    LoseArrows(1);
                }

                break;


            default:
                for (int i = 0; i < currentWeapon.numberOfAttacks; i++)
                {
                    currentlySelectedEnemy.TakeDamage(currentWeapon.meleeDamage);
                }
                break;
        }
        targetingAttack = false;
        currentlySelectedEnemy.isBeingTargeted(false);
        GameManager.instance.playersTurn = false;
    }

    protected override void AttemptMove(int dirX, int dirY)
    {
        CheckIfGameOver();
        if (!pathfinding.CheckIfValidNode(currentNode, new Vector2(dirX, dirY)))
            return;
        if (pathfinding.CheckIfWall(currentNode, new Vector2Int(dirX, dirY)))
            return;
        if (pathfinding.CheckIfEnemy(currentNode, new Vector2(dirX, dirY)))
        {
            Enemy hitEnemy = GameManager.instance.GetEnemyOnNode(levelGrid.grid[currentNode.gridX + dirX, currentNode.gridY + dirY]);
            MeleeAttack(hitEnemy);
            return;
        }

        base.AttemptMove(dirX, dirY);

        GameManager.instance.playersTurn = false;

    }

    //protected override void OnCantMove<T>(T component)
    //{
    //    Debug.Log("Player Can't move");
    //    if (component is Enemy)
    //    {
    //        Enemy hitEnemy = component as Enemy;
    //        MeleeAttack(hitEnemy);
    //    }
    //}

    private void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoseHealth(int loss)
    {
        currentHealth -= loss;
        healthBar.value = currentHealth;
        CheckIfGameOver();
    }
    public void GainHealth(int gain)
    {
        currentHealth += gain;
        healthBar.value = currentHealth;
    }

    public void GainArrows(int gain)
    {
        inventory.ArrowCount += gain;
    }

    public void LoseArrows(int loss)
    {
        inventory.ArrowCount -= loss;
    }
    public void GainCoins (int gain)
    {
        inventory.CoinCount += gain;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Exit"))
        {
            Invoke("Restart", restartLevelDelay);
            enabled = false;
        }
        if (other.CompareTag("Pickup"))
        {
            other.GetComponent<Pickup>().OnPickup(gameObject);
        }
    }

    private void MeleeAttack(Enemy enemy)
    {
        for (int i = 0; i < currentWeapon.numberOfAttacks; i++)
        {
            enemy.TakeDamage(currentWeapon.meleeDamage);
        }
        GameManager.instance.playersTurn = false;
    }

    private void GetTargetsInRange()
    {
        targetingIndex = 0;
        enemiesInRange.Clear();

        foreach (var enemy in GameManager.instance.enemies)
        {
            if (Mathf.Abs(transform.position.x - enemy.transform.position.x) + Mathf.Abs(transform.position.y - enemy.transform.position.y) <= currentWeapon.range)
            {
                enemiesInRange.Add(enemy);
            }
        }
        if (enemiesInRange.Count == 0 || (currentWeapon.weaponType == Weapon.WeaponType.RANGED && inventory.ArrowCount < 1))
        {
            targetingAttack = false;
            return;
        }
        targetingAttack = true;
    }
}
