using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AISettingsManager : MonoBehaviour
{
    public enum SettingID
    {
        DamageDealtWeight,
        DamageTakenWeight,
        LethalDamageWeight,
        ExtraDamageWeight,
        EnemyExtraDamageWeight,
        DistanceWeight,
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
    public AI blueAI;
    public AI orangeAI;
    public GeneticAlgorithm.AISettings blueAISettings;
    public GeneticAlgorithm.AISettings orangeAISettings;
    public bool isOrange;
    public GameObject changeText;
    public Text aiSettingsText;
    public Color blueColor;
    public Color orangeColor;

    // Private Properties
    private GameController gameController;
    private List<Setting> settings;
    private Setting familySetting;
    private int oldDebrisCount;

    /*
     * This function will initialize the settings handler by finding all settings objects
     */

    void Start()
    {
        settings = new List<Setting>();
        blueAISettings = new GeneticAlgorithm.AISettings(blueAI.aiSettings);
        orangeAISettings = new GeneticAlgorithm.AISettings(orangeAI.aiSettings);
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        oldDebrisCount = gameController.obstacleCount;
        foreach (GameObject settingObject in GameObject.FindGameObjectsWithTag("Setting"))
        {
            Setting setting = settingObject.GetComponent<Setting>();
            settings.Add(setting);
            switch (setting.ID)
            {
                case SettingID.DamageDealtWeight:
                    setting.SetValueWithoutNotify(blueAI.aiSettings.damageDealtWeight);
                    break;
                case SettingID.DamageTakenWeight:
                    setting.SetValueWithoutNotify(blueAI.aiSettings.damageTakenWeight);
                    break;
                case SettingID.LethalDamageWeight:
                    setting.SetValueWithoutNotify(blueAI.aiSettings.lethalDamageWeight);
                    break;
                case SettingID.ExtraDamageWeight:
                    setting.SetValueWithoutNotify(blueAI.aiSettings.extraDamageWeight);
                    break;
                case SettingID.EnemyExtraDamageWeight:
                    setting.SetValueWithoutNotify(blueAI.aiSettings.enemyExtraDamageWeight);
                    break;
                case SettingID.DistanceWeight:
                    setting.SetValueWithoutNotify(blueAI.aiSettings.distanceWeight);
                    break;
                case SettingID.Generations:
                    setting.SetValueWithoutNotify(blueAI.aiSettings.generations);
                    break;
                case SettingID.PopulationSize:
                    setting.SetValueWithoutNotify(blueAI.aiSettings.populationSize);
                    break;
                case SettingID.MutationChance:
                    setting.SetValueWithoutNotify(blueAI.aiSettings.mutationChance);
                    break;
                case SettingID.Families:
                    familySetting = setting;
                    setting.SetValueWithoutNotify(blueAI.aiSettings.families);
                    break;
                case SettingID.SelectionMethod:
                    setting.SetValueWithoutNotify((int)blueAI.aiSettings.selectionMethod);
                    break;
                case SettingID.FighterCount:
                    setting.SetValueWithoutNotify(gameController.blueFighterCount);
                    break;
                case SettingID.DefenderCount:
                    setting.SetValueWithoutNotify(gameController.orangeFighterCount);
                    break;
                case SettingID.ObstacleCount:
                    setting.SetValueWithoutNotify(gameController.obstacleCount);
                    break;
                case SettingID.TurnDelay:
                    setting.SetValueWithoutNotify(gameController.turnDelay);
                    break;
                case SettingID.ReplacementMethod:
                    setting.SetValueWithoutNotify((int)blueAI.aiSettings.replacementMethod);
                    break;
            }
        }
    }

    public void UpdateSetting(Setting setting)
    {
        GeneticAlgorithm.AISettings aiSettings = isOrange ? orangeAISettings : blueAISettings;
        changeText.SetActive(true);
        switch (setting.ID)
        {
            case SettingID.DamageDealtWeight:
                aiSettings.damageDealtWeight = setting.floatValue;
                break;
            case SettingID.DamageTakenWeight:
                aiSettings.damageTakenWeight = setting.floatValue;
                break;
            case SettingID.LethalDamageWeight:
                aiSettings.lethalDamageWeight = setting.floatValue;
                break;
            case SettingID.ExtraDamageWeight:
                aiSettings.extraDamageWeight = setting.floatValue;
                break;
            case SettingID.EnemyExtraDamageWeight:
                aiSettings.enemyExtraDamageWeight = setting.floatValue;
                break;
            case SettingID.DistanceWeight:
                aiSettings.distanceWeight = setting.floatValue;
                break;
            case SettingID.Generations:
                if (setting.numValue < 0)
                {
                    setting.SetValueWithoutNotify(1);
                }
                aiSettings.generations = setting.numValue;
                break;
            case SettingID.PopulationSize:
                if (setting.numValue < 1)
                {
                    setting.SetValueWithoutNotify(1);
                }
                if (aiSettings.families > setting.numValue / 2)
                {
                    aiSettings.families = setting.numValue / 2;
                    familySetting.SetValueWithoutNotify(setting.numValue / 2);
                }
                aiSettings.populationSize = setting.numValue;
                break;
            case SettingID.MutationChance:
                aiSettings.mutationChance = setting.floatValue;
                break;
            case SettingID.Families:
                if (setting.numValue < 0)
                {
                    setting.SetValueWithoutNotify(0);
                }
                if (setting.numValue > aiSettings.populationSize / 2)
                {
                    setting.SetValueWithoutNotify(aiSettings.populationSize / 2);
                }
                aiSettings.families = setting.numValue;
                break;
            case SettingID.SelectionMethod:
                aiSettings.selectionMethod = GeneticAlgorithm.IntToSelectionMethod(setting.numValue);
                break;
            case SettingID.FighterCount:
                if (setting.numValue < 0)
                {
                    setting.SetValueWithoutNotify(0);
                }
                gameController.blueFighterCount = setting.numValue;
                break;
            case SettingID.DefenderCount:
                if (setting.numValue < 0)
                {
                    setting.SetValueWithoutNotify(0);
                }
                gameController.orangeFighterCount = setting.numValue;
                break;
            case SettingID.ObstacleCount:
                if (setting.numValue < 0)
                {
                    setting.SetValueWithoutNotify(0);
                }
                gameController.obstacleCount = setting.numValue;
                break;
            case SettingID.TurnDelay:
                if (setting.numValue < 0)
                {
                    setting.SetValueWithoutNotify(0);
                }
                gameController.turnDelay = setting.numValue;
                break;
            case SettingID.ReplacementMethod:
                aiSettings.replacementMethod = GeneticAlgorithm.IntToReplacementMethod(setting.numValue);
                break;
        }
    }

    /*
     * This function will set all of the settings in the AI when the battle starts
     */

    public void StartBattle()
    {
        blueAI.aiSettings = blueAISettings;
        orangeAI.aiSettings = orangeAISettings;
        changeText.SetActive(false);

        // If the obstacle count has been updated then regenerate the battlefield

        if (oldDebrisCount != gameController.obstacleCount)
        {
            oldDebrisCount = gameController.obstacleCount;
            gameController.CreateBattlefield();
        }
        gameController.Reset();
        changeText.SetActive(false);
    }

    public void SwitchAI(Button button)
    {
        isOrange = !isOrange;
        button.image.color = isOrange ? orangeColor : blueColor;
        button.GetComponentInChildren<TextMeshProUGUI>().color = isOrange ? Color.black : Color.white;
        GeneticAlgorithm.AISettings aiSettings = isOrange ? orangeAISettings : blueAISettings;
        aiSettingsText.text = (isOrange ? "Orange" : "Blue") + " AI Settings";
        foreach (Setting setting in settings)
        {
            switch (setting.ID)
            {
                case SettingID.DamageDealtWeight:
                    setting.SetValueWithoutNotify(aiSettings.damageDealtWeight);
                    break;
                case SettingID.DamageTakenWeight:
                    setting.SetValueWithoutNotify(aiSettings.damageTakenWeight);
                    break;
                case SettingID.LethalDamageWeight:
                    setting.SetValueWithoutNotify(aiSettings.lethalDamageWeight);
                    break;
                case SettingID.ExtraDamageWeight:
                    setting.SetValueWithoutNotify(aiSettings.extraDamageWeight);
                    break;
                case SettingID.EnemyExtraDamageWeight:
                    setting.SetValueWithoutNotify(aiSettings.enemyExtraDamageWeight);
                    break;
                case SettingID.DistanceWeight:
                    setting.SetValueWithoutNotify(aiSettings.distanceWeight);
                    break;
                case SettingID.Generations:
                    setting.SetValueWithoutNotify(aiSettings.generations);
                    break;
                case SettingID.PopulationSize:
                    setting.SetValueWithoutNotify(aiSettings.populationSize);
                    break;
                case SettingID.MutationChance:
                    setting.SetValueWithoutNotify(aiSettings.mutationChance);
                    break;
                case SettingID.Families:
                    familySetting = setting;
                    setting.SetValueWithoutNotify(aiSettings.families);
                    break;
                case SettingID.SelectionMethod:
                    setting.SetValueWithoutNotify((int)aiSettings.selectionMethod);
                    break;
                case SettingID.FighterCount:
                    setting.SetValueWithoutNotify(gameController.blueFighterCount);
                    break;
                case SettingID.DefenderCount:
                    setting.SetValueWithoutNotify(gameController.orangeFighterCount);
                    break;
                case SettingID.ObstacleCount:
                    setting.SetValueWithoutNotify(gameController.obstacleCount);
                    break;
                case SettingID.TurnDelay:
                    setting.SetValueWithoutNotify(gameController.turnDelay);
                    break;
                case SettingID.ReplacementMethod:
                    setting.SetValueWithoutNotify((int)aiSettings.replacementMethod);
                    break;
            }
        }
    }
}
