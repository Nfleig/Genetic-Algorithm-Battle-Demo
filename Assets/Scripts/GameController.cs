using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class GameController : MonoBehaviour
{
    public GameObject fighter;
    public GameObject defender;

    public Nexus playerNexus;
    public Nexus enemyNexus;
    
    public AI playerAI;
    public AI enemyAI;

    public int fighterCount;
    public int defenderCount;

    public float armyDistance = 3;
    public float turnDelay = 10;
    public int nexusHealth = 80;
    private float turnTimer = 900;

    private List<FighterController> fighters;
    private List<FighterController> defenders;
    private List<GameObject> Obstacles = new List<GameObject>();
    private bool gameOver;
    public Text winLabel;
    public GameObject menu;
    public GameObject UI;

    public GameObject Obstacle;
    public int ObstacleCount;
    public float ObstacleRange;

    void Awake(){
        CreateBattlefield();
        Reset();
    }

    List<FighterController> oldUnits;
    public float deltaTime;
    void Update(){
        //deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / Time.deltaTime;
        if(fps < 10 && playerAI.GPF > 5){
            playerAI.GPF--;
            enemyAI.GPF--;
        }
        else if(fps > 12 && fps < 50 && playerAI.GPF < 50){
            playerAI.GPF++;
            enemyAI.GPF++;

        }
        print(fps);

        int numFighters = 0;
        foreach(FighterController fighter in fighters){
            if(!fighter.dead){
                numFighters++;
            }
        }
        int numDefenders = 0;
        foreach(FighterController fighter in defenders){
            if(!fighter.dead){
                numDefenders++;
            }
        }
        if((numFighters == 0 || playerNexus.health <= 0) && !gameOver){
            winLabel.text = "Orange Team Wins";
            gameOver = true;
            playerAI.Stop();
            enemyAI.Stop();
        }
        if((numDefenders == 0 || enemyNexus.health <= 0) && !gameOver){
            winLabel.text = "Blue Team Wins";
            gameOver = true;
            playerAI.Stop();
            enemyAI.Stop();
        }

        if(!gameOver){
            turnTimer -= Time.fixedDeltaTime;
            if(turnTimer <= 0){
                //print(fighters.Count);
                playerAI.runAI();
                enemyAI.runAI();
                turnTimer = turnDelay;
            }
        }
        if(Input.GetKeyDown("escape")){
            menu.SetActive(!menu.active);
            UI.SetActive(!UI.active);
        }
    }

    public void Reset(){
        playerAI.gameObject.SetActive(false);
        enemyAI.gameObject.SetActive(false);
        if(fighters != null && defenders != null){
            foreach(FighterController fighter in fighters){
                if(fighter){
                    fighter.Die();
                    Destroy(fighter.gameObject);
                }
            }
            foreach(FighterController fighter in defenders){
                if(fighter){
                    fighter.Die();
                    Destroy(fighter.gameObject);
                }
            }
        }
        fighters = new List<FighterController>();
        defenders = new List<FighterController>();
        gameOver = false;
        playerNexus.health = nexusHealth;
        enemyNexus.health = nexusHealth;
        playerAI.gameObject.SetActive(true);
        enemyAI.gameObject.SetActive(true);


        float fighterLength = fighterCount * 1.25f;
        turnTimer = 0;

        GameObject newFighter;


        for(int i = 0; i < fighterCount; i++){
            newFighter = GameObject.Instantiate(fighter, new Vector3((-(fighterLength/2) + ((i + 1) * 1.25f)), 0, -armyDistance), Quaternion.identity);
            fighters.Add(newFighter.GetComponent<FighterController>());
        }
        float defenderLength = defenderCount * 1.25f;
        for(int i = 0; i < defenderCount; i++){
            newFighter = GameObject.Instantiate(defender, new Vector3((-(defenderLength/2) + ((i + 1) * 1.25f)), 0, armyDistance), Quaternion.identity);
            defenders.Add(newFighter.GetComponent<FighterController>());
        }
        winLabel.text = "";
    }
    
    public void CreateBattlefield(){
        if(Obstacles.Count > 0){
            foreach(GameObject obstacle in Obstacles){
                Destroy(obstacle);
            }
        }
        for(int i = 0; i < ObstacleCount; i++){
            GameObject newObstacle = GameObject.Instantiate(Obstacle, new Vector3((Random.value * ObstacleRange) - (ObstacleRange / 2), -0.5f, (Random.value * ObstacleRange) - (ObstacleRange / 2)), Quaternion.Euler(Random.value * 90, Random.value * 90, Random.value * 90));
            Obstacles.Add(newObstacle);
        }
    }

    public void Quit(){
        Application.Quit();
    }
}
