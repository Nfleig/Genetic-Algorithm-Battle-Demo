using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AISettings : MonoBehaviour
{
    // Public Properties
    public AI ai;
    public GameController game;

    // Private Properties
    private List<Setting> settings;

    /*
     * This function will initialize the settings handler by finding all settings objects
     */

    void Start()
    {
        settings = new List<Setting>();
        foreach(GameObject setting in GameObject.FindGameObjectsWithTag("Setting")){
            settings.Add(setting.GetComponent<Setting>());
        }
    }
    
    /*
     * This function will set all of the settings in the AI when the battle starts
     */

    public void StartBattle(){
        int oldDebris = game.ObstacleCount;
        foreach(Setting setting in settings){
            switch(setting.ID){
                case 1: // Agressiveness
                    ai.agressiveness = setting.value;
                    break;
                case 2: // Lethal Damage Weight
                    ai.LethalDamageWeight = setting.value;
                    break;
                case 3: // Extra Damage Weight
                    ai.ExtraDamageWeight = setting.value;
                    break;
                case 4: // Enemy Extra Damage Weight
                    ai.EnemyExtraDamageWeight = setting.value;
                    break;
                case 5: // Distance Weight
                    ai.distanceWeight = setting.value;
                    break;
                case 6: // Use Pathfinding
                    ai.usePathfinding = setting.boolValue;
                    break;
                case 7: // Generations
                    ai.generations = setting.numValue;
                    break;
                case 8: // Population Size
                    ai.populationSize = setting.numValue;
                    break;
                case 9: // Mutate chance
                    ai.mutateChance = setting.value;
                    break;
                case 10: // Families
                    ai.families = setting.numValue;
                    break;
                case 11: // Selection Method
                    ai.selectionMethod = setting.numValue;
                    break;
                case 12: // Fighter Count
                    game.fighterCount = setting.numValue;
                    break;
                case 13: // Defender Count
                    game.defenderCount = setting.numValue;
                    break;
                case 14: // Obstacle Count
                    game.ObstacleCount = setting.numValue;
                    break;
                case 15: // Turn Delay
                    game.turnDelay = setting.numValue;
                    break;
                case 16: // Replacement Method
                    ai.replacementMethod = setting.numValue;
                    break;
            }
        }

        // If the obstacle count has been updated then regenerate the battlefield

        if(oldDebris != game.ObstacleCount){
            game.CreateBattlefield();
        }
        game.Reset();
    }
}
