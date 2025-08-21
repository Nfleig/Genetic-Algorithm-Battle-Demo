using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    // Public Properties
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
    public GameObject menu;
    public GameObject UI;
    public GameObject Obstacle;
    public int ObstacleCount;
    public float ObstacleRange;
    public Text winLabel;

    // Private Properties
    private float _turnTimer = 900;
    public List<FighterController> blueFighters;
    public List<FighterController> orangeFighters;
    private List<GameObject> Obstacles = new List<GameObject>();
    private bool _gameOver;

    /*
     * This function initializes the game
     */
    void Awake(){
        CreateBattlefield();
        Reset();
    }

    /*
     * This is the main gameplay loop
     */
    void Update(){

        /*
         * This is how I tried to fix my framerate issues before I knew
         * how to run code asynchronously
         *
         * TODO: Make the genetic algorithm asynchronous and get rid of this
         */

        // Calculate the fps

        float fps = 1.0f / Time.deltaTime;
        
        // If the framerate is too low then decrease the generations per second

        if(fps < 10 && playerAI.GPF > 5){
            playerAI.GPF--;
            enemyAI.GPF--;
        }

        // If the framerate is high enough then we can start increasing the generations per second

        else if(fps > 12 && fps < 50 && playerAI.GPF < 50){
            playerAI.GPF++;
            enemyAI.GPF++;
        }

        // Get the number of fighters

        int numFighters = 0;
        foreach(FighterController fighter in blueFighters){
            if(!fighter.dead){
                numFighters++;
            }
        }
        int numDefenders = 0;
        foreach(FighterController fighter in orangeFighters){
            if(!fighter.dead){
                numDefenders++;
            }
        }

        // See if either player has lost or the game has ended

        if ((numFighters == 0 || playerNexus.health <= 0) && !_gameOver)
        {
            winLabel.text = "Orange Team Wins";
            _gameOver = true;
            playerAI.Stop();
            enemyAI.Stop();
            foreach(FighterController fighter in blueFighters.Concat(orangeFighters)){
                fighter.Reset();
            }
        }
        if ((numDefenders == 0 || enemyNexus.health <= 0) && !_gameOver)
        {
            winLabel.text = "Blue Team Wins";
            _gameOver = true;
            playerAI.Stop();
            enemyAI.Stop();
            foreach(FighterController fighter in blueFighters.Concat(orangeFighters)){
                fighter.Reset();
            }
        }

        // If the game hasn't ended, then rerun the genetic algorithms at regular intervals

        if(!_gameOver){
            _turnTimer -= Time.fixedDeltaTime;
            if(_turnTimer <= 0){
                //print(fighters.Count);
                playerAI.runAI();
                enemyAI.runAI();
                _turnTimer = turnDelay;
            }
        }

        // Toggle the menu if the player presses escape

        if(Input.GetKeyDown("space")){
            menu.SetActive(!menu.activeSelf);
            UI.SetActive(!UI.activeSelf);
        }
    }

    /*
     * This function resets and restarts the battle
     */

    public void Reset(){

        // Disable the AIs

        playerAI.gameObject.SetActive(false);
        enemyAI.gameObject.SetActive(false);
        Debug.Log("AIs disabled!");
        
        // Kill all of the fighters and defenders

        if (blueFighters != null && orangeFighters != null)
        {
            foreach (FighterController fighter in blueFighters)
            {
                if (fighter)
                {
                    Destroy(fighter.gameObject);
                }
            }
            foreach (FighterController fighter in orangeFighters)
            {
                if (fighter)
                {
                    Destroy(fighter.gameObject);
                }
            }
        }

        // Reset all of the important variables

        blueFighters = new List<FighterController>();
        orangeFighters = new List<FighterController>();
        _gameOver = false;
        playerNexus.health = nexusHealth;
        enemyNexus.health = nexusHealth;
        
        // Reactivate the AIs
        
        playerAI.gameObject.SetActive(true);
        enemyAI.gameObject.SetActive(true);

        // Get the length of the line of fighters

        float fighterLength = fighterCount * 1.25f;

        _turnTimer = 0;

        GameObject newFighter;

        // Respawn all of the fighters on both sides

        for(int i = 0; i < fighterCount; i++){
            newFighter = Instantiate(fighter, new Vector3(-(fighterLength/2) + ((i + 1) * 1.25f), 0, -armyDistance), Quaternion.identity);
            FighterController newFighterController = newFighter.GetComponent<FighterController>();
            newFighterController.SetID(i);
            blueFighters.Add(newFighterController);
        }

        float defenderLength = defenderCount * 1.25f;
        for(int i = 0; i < defenderCount; i++){
            newFighter = Instantiate(defender, new Vector3(-(defenderLength/2) + ((i + 1) * 1.25f), 0, armyDistance), Quaternion.identity);
            FighterController newFighterController = newFighter.GetComponent<FighterController>();
            newFighterController.SetID(i);
            orangeFighters.Add(newFighterController);
        }

        // Hide the win label

        winLabel.text = "";
    }

    public void RemoveFighter(FighterController fighter)
    {
        if (fighter.defender)
        {
            orangeFighters.Remove(fighter);
        }
        else
        {
            blueFighters.Remove(fighter);
        }
    }

    /*
     * This function fills the battlefield with obstacles for the fighters to pathfind around
     */

    public void CreateBattlefield()
    {

        // Destroy all obstacles currently on the battlefield

        if (Obstacles.Count > 0)
        {
            foreach (GameObject obstacle in Obstacles)
            {
                Destroy(obstacle);
            }
        }

        // Spawn a bunch of random cubes in the battlefield

        for (int i = 0; i < ObstacleCount; i++)
        {
            GameObject newObstacle = GameObject.Instantiate(Obstacle, new Vector3((Random.value * ObstacleRange) - (ObstacleRange / 2), -0.5f, (Random.value * ObstacleRange) - (ObstacleRange / 2)), Quaternion.Euler(Random.value * 90, Random.value * 90, Random.value * 90));
            Obstacles.Add(newObstacle);
        }
    }

    /*
     * This function closes the game
     */

    public void Quit(){
        Application.Quit();
    }
}
