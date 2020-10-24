using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AISettings : MonoBehaviour
{
    public AI ai;
    public GameController game;
    private List<Setting> settings;
    // Start is called before the first frame update
    void Start()
    {
        settings = new List<Setting>();
        foreach(GameObject setting in GameObject.FindGameObjectsWithTag("Setting")){
            settings.Add(setting.GetComponent<Setting>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void StartBattle(){
        int oldDebris = game.ObstacleCount;
        foreach(Setting setting in settings){
            switch(setting.ID){
                case 1:
                    ai.agressiveness = setting.value;
                    break;
                case 2:
                    ai.LethalDamageWeight = setting.value;
                    break;
                case 3:
                    ai.ExtraDamageWeight = setting.value;
                    break;
                case 4:
                    ai.EnemyExtraDamageWeight = setting.value;
                    break;
                case 5:
                    ai.distanceWeight = setting.value;
                    break;
                case 6:
                    ai.usePathfinding = setting.boolValue;
                    break;
                case 7:
                    ai.generations = setting.numValue;
                    break;
                case 8:
                    ai.populationSize = setting.numValue;
                    break;
                case 9:
                    ai.mutateChance = setting.value;
                    break;
                case 10:
                    ai.families = setting.numValue;
                    break;
                case 11:
                    ai.selectionMethod = setting.numValue;
                    break;
                case 12:
                    game.fighterCount = setting.numValue;
                    break;
                case 13:
                    game.defenderCount = setting.numValue;
                    break;
                case 14:
                    game.ObstacleCount = setting.numValue;
                    break;
                case 15:
                    game.turnDelay = setting.numValue;
                    break;
                case 16:
                    ai.replacementMethod = setting.numValue;
                    break;
            }
        }
        if(oldDebris != game.ObstacleCount){
            game.CreateBattlefield();
        }
        game.Reset();
    }
}
