using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AISettings : MonoBehaviour
{
    public enum SettingID
    {
        Agression,
        LethalDamageWeight,
        ExtraDamageWeight,
        EnemyExtraDamageWeight,
        DistanceWeight,
        UsePathfinding,
        Generations,
        PopulationSize,
        MutationChance,
        Families,
        SelectionMethod,
        FighterCount,
        DefenderCount,
        ObstacleCount,
        TurnDelay,
        ReplacementMethod
    }
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
                case SettingID.Agression:
                    ai.damageDealtWeight = setting.value;
                    ai.damageTakenWeight = 1 / setting.value;
                    break;
                case SettingID.LethalDamageWeight:
                    ai.lethalDamageWeight = setting.value;
                    break;
                case SettingID.ExtraDamageWeight:
                    ai.extraDamageWeight = setting.value;
                    break;
                case SettingID.EnemyExtraDamageWeight:
                    ai.enemyExtraDamageWeight = setting.value;
                    break;
                case SettingID.DistanceWeight:
                    ai.distanceWeight = setting.value;
                    break;
                case SettingID.UsePathfinding:
                    ai.usePathfinding = setting.boolValue;
                    break;
                case SettingID.Generations:
                    ai.generations = setting.numValue;
                    break;
                case SettingID.PopulationSize:
                    ai.populationSize = setting.numValue;
                    break;
                case SettingID.MutationChance:
                    ai.mutationChance = setting.value;
                    break;
                case SettingID.Families:
                    ai.families = setting.numValue;
                    break;
                case SettingID.SelectionMethod:
                    ai.selectionMethod = (AI.SelectionMethod) setting.numValue;
                    break;
                case SettingID.FighterCount:
                    game.fighterCount = setting.numValue;
                    break;
                case SettingID.DefenderCount:
                    game.defenderCount = setting.numValue;
                    break;
                case SettingID.ObstacleCount:
                    game.ObstacleCount = setting.numValue;
                    break;
                case SettingID.TurnDelay:
                    game.turnDelay = setting.numValue;
                    break;
                case SettingID.ReplacementMethod:
                    ai.replacementMethod = (AI.ReplacementMethod) setting.numValue;
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
