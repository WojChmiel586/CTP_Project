using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public DungeonController dungeonController;
    public bool playersTurn = true;
    public int playersCurrentHealth = 30;

    private float turnDelay = 0.05f;
    public List<Enemy> enemies;
    public bool enemiesMoving;
    public bool doingSetup = false;
    public PlayerControls player;
    public GameObject LoadingScreen;
    public TMP_Text arrowCount;
    public TMP_Text coinCount;
    PlayerInventory playerInventory;
    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        enemies = new List<Enemy>();
        InitGame();
    }

    private void InitGame()
    {
       doingSetup = true;
        
        enemies.Clear();
    }
    // Update is called once per frame
    void Update()
    {
        if (playersTurn || enemiesMoving || doingSetup)
        {
            return;
        }
        if (player == null)
        {
            player = FindObjectOfType<PlayerControls>();
            playerInventory = player.inventory;
        }
        StartCoroutine(MoveEnemies());
        TrackPlayerInventory();
    }

    private void TrackPlayerInventory()
    {
        coinCount.text = playerInventory.CoinCount.ToString();
        arrowCount.text = playerInventory.ArrowCount.ToString();
    }

    public void AddEnemyToList(Enemy enemy)
    {
        enemies.Add(enemy);
    }
    public void RemoveEnemyFromList(Enemy enemy)
    {
        enemies.Remove(enemy);
    }
    public void GameOver()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        playersTurn = false;
        enabled = false;
    }
    IEnumerator MoveEnemies()
    {
        enemiesMoving = true;


        yield return new WaitForSeconds(turnDelay);


        if (enemies.Count == 0)
        {
            yield return new WaitForSeconds(turnDelay);
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].MoveEnemy();

            yield return new WaitForSeconds(enemies[i].moveTime);
        }
        playersTurn = true;

        enemiesMoving = false;
    }

    public Enemy GetEnemyOnNode(GridNode node)
    {
        foreach (var enemy in enemies)
        {
            if (enemy.currentNode == node)
            {
                return enemy;
            }
        }
        return null;
    }
}
