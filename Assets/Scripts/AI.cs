using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngineInternal;

public class AI : MonoBehaviour
{
    //These are the lists of the units in each team
    public bool isOrange;

    //These are all of the different settings for the AI
    public GeneticAlgorithm.AISettings aiSettings;

    private bool isActive = false;
    private bool isStarting = true;
    private int currentGeneration = 0;
    private GameController gameController;
    private GeneticAlgorithm geneticAlgorithm;
    private Task<GeneticAlgorithm.Gene> evolutionTask;
    private CancellationTokenSource evolutionTaskCancellationTokenSource;

    void Awake()
    {
        //Initializes all of the lists
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        geneticAlgorithm = new GeneticAlgorithm(aiSettings, gameController, isOrange);
        evolutionTaskCancellationTokenSource = new CancellationTokenSource();
        print("Should be initialized now");
    }



    public void runAI()
    {
        geneticAlgorithm.Reset(gameController);
        if (gameController.multithreading)
        {
            CancellationToken evolutionTaskCancellationToken = evolutionTaskCancellationTokenSource.Token;
            evolutionTask = new Task<GeneticAlgorithm.Gene>((evolutionTaskCancellationToken) => geneticAlgorithm.SimulateEvolution(aiSettings.generations, (CancellationToken)evolutionTaskCancellationToken), evolutionTaskCancellationTokenSource.Token);
            evolutionTask.Start();
        }
        currentGeneration = 0;
        isActive = true;
        isStarting = true;
    }

    void Update()
    {
        if (isActive)
        {
            if (gameController.multithreading)
            {
                if (evolutionTask != null && evolutionTask.IsCompleted)
                {
                    //Sorts the list of genes and finds the one with the greatest fitness

                    GeneticAlgorithm.Gene bestGene = evolutionTask.Result;

                    ExecuteGene(bestGene);

                    isActive = false;
                }
            }
            else
            {
                if (currentGeneration >= aiSettings.generations)
                {
                    GeneticAlgorithm.Gene bestGene = geneticAlgorithm.GetBestGene();

                    ExecuteGene(bestGene);

                    isActive = false;
                }
                else
                {
                    geneticAlgorithm.SimulateEvolution(aiSettings.GPF);
                    currentGeneration += aiSettings.GPF;
                }
            }
        }
    }

    private void ExecuteGene(GeneticAlgorithm.Gene bestGene)
    {
        foreach (GeneticAlgorithm.AttackPair pair in bestGene.genotype)
        {
            if (pair.fighter is GeneticAlgorithm.Fighter)
            {
                FighterController fighterController = GetFighterController(pair.fighter as GeneticAlgorithm.Fighter);
                if (fighterController == null)
                {
                    continue;
                }
                if (pair.target is GeneticAlgorithm.Fighter)
                {
                    FighterController enemyController = GetFighterController(pair.target as GeneticAlgorithm.Fighter);
                    if (enemyController != null)
                    {
                        fighterController.Fight(enemyController);
                    }
                    else
                    {
                        //print("Couldn't find " + (pair.target.isOrange ? "orange" : "blue") + " fighter with ID: " + (pair.target as Fighter).id);
                        fighterController.Fight(fighterController);
                    }
                }
                else
                {
                    fighterController.Fight(isOrange ? gameController.blueNexus : gameController.orangeNexus);
                }
            }
        }
    }

    FighterController GetFighterController(GeneticAlgorithm.Fighter fighter)
    {
        return fighter.isOrange ?
            gameController.orangeFighters.Find(fighterController => fighterController.id == fighter.id) :
            gameController.blueFighters.Find(fighterController => fighterController.id == fighter.id);
    }



    //This method is called once the game has ended and it will make all remaining units return to their original position
    public void Stop()
    {
        evolutionTaskCancellationTokenSource.Cancel();
        isActive = false;

    }

    //This method compiles data for the UI
    public string getData()
    {
        string state = "Idle";
        double bestFitness = geneticAlgorithm.GetBestGene().fitness;
        if (isActive)
        {
            state = "Thinking";
        }
        string data = "State: " + state + "\nGeneration: " + geneticAlgorithm.GetCurrentGeneration() + "\nBestFitness: " + bestFitness + "\nNexus Health: " + (isOrange ? gameController.orangeNexus : gameController.blueNexus).health;
        return data;
    }
}
