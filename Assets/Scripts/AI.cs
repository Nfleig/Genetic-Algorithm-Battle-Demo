using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI : MonoBehaviour
{

    public enum SelectionMethod
    {
        Roulette,
        Random,
        BestFit
    }

    public static SelectionMethod IntToSelectionMethod(int value)
    {
        return value switch
        {
            0 => SelectionMethod.Roulette,
            1 => SelectionMethod.Random,
            2 => SelectionMethod.BestFit,
            _ => throw new Exception("Value " + value + " does not correspond to a selection method.")
        };
    }

    public enum ReplacementMethod
    {
        WeakestParent,
        BothParents,
        WeakestGene
    }

    public static ReplacementMethod IntToReplacementMethod(int value)
    {
        return value switch
        {
            0 => ReplacementMethod.WeakestParent,
            1 => ReplacementMethod.BothParents,
            2 => ReplacementMethod.WeakestGene,
            _ => throw new Exception("Value " + value + " does not correspond to a replacement method.")
        };
    }

    public abstract class Attackable
    {
        public float health;
        public int armor;
        public bool isOrange;
        public Vector3 position;
        public Attackable(float health, int armor, bool isOrange, Vector3 position)
        {
            this.health = health;
            this.armor = armor;
            this.isOrange = isOrange;
            this.position = position;
        }
    }

    public class Fighter : Attackable
    {
        public int id;
        public int damage;

        public Fighter(float health, int damage, int armor, bool isOrange, Vector3 position, int id) : base(health, armor, isOrange, position)
        {
            this.damage = damage;
            this.id = id;
        }

        public override string ToString()
        {
            return "{ " + (isOrange ? "Orange Fighter " : "Blue Fighter ") + id + " }";
        }
    }

    public class NexusData : Attackable
    {
        public NexusData(float health, bool isOrange, Vector3 position) : base(health, 1, isOrange, position)
        {

        }

        public void SetHealth(float health)
        {
            this.health = health;
        }

        public override string ToString()
        {
            return "{ " + (isOrange ? "Orange Nexus" : "Blue Nexus") + " }";
        }
    }

    public struct AttackPair
    {
        public Attackable fighter;
        public Attackable target;

        public AttackPair(Attackable fighter, Attackable target)
        {
            this.fighter = fighter;
            this.target = target;
        }

        public override string ToString()
        {
            return "{ " + fighter.ToString() + " -> " + target.ToString() + " }";
        }
    }

    /*
        This class represents one chromosome and includes all of the data and methods that it needs
    */
    public class Gene
    {
        //These two are just lists of all of the units on each side that the Gene will use to create it's genotype later
        public static List<Fighter> blueFighters;
        public static List<Fighter> orangeFighters;
        public static NexusData blueNexus;
        public static NexusData orangeNexus;
        public List<AttackPair> genotype;
        public double fitness;

        //These values help the AI execute the genotype's instructions if this gene is chosen
        public int matchups;
        private bool MoreAttackers;
        private AI ai;

        //Initializer assigns all of the gene's data and then generates a random genotype
        public Gene(AI ai)
        {
            this.ai = ai;
            genotype = new List<AttackPair>();
            fitness = 0;
            Randomize();
            CalculateFitness();
        }

        //Generates a new random genotype
        public void Reset()
        {
            genotype = new List<AttackPair>();
            fitness = 0;
            Randomize();
            CalculateFitness();
        }

        /*
            This method intitializes a random genotype
            Genotypes are structured as pairs of units that will attack eachother
            If you were to look at one it would look like this:

            {fighter, defender, fighter, defender, ...}

            where the first fighter would fight the first defender, and so on
        */
        public void Randomize()
        {
            List<Attackable> tempEnemies = new List<Attackable>();
            List<Fighter> playerList = ai.isOrange ? orangeFighters : blueFighters;
            List<Fighter> enemyList = ai.isOrange ? blueFighters : orangeFighters;

            foreach (Fighter fighter in enemyList)
            {
                tempEnemies.Add(fighter);
            }

            foreach (Fighter fighter in playerList)
            {
                if (tempEnemies.Count > 0)
                {
                    int randomID = (int)(UnityEngine.Random.value * (tempEnemies.Count - 1));
                    genotype.Add(new AttackPair(fighter, tempEnemies[randomID]));
                    tempEnemies.RemoveAt(randomID);
                }
                else
                {
                    genotype.Add(new AttackPair(fighter, ai.isOrange ? blueNexus : orangeNexus));
                }
            }

            foreach (Fighter enemyFighter in tempEnemies)
            {
                genotype.Add(new AttackPair(ai.isOrange ? orangeNexus : blueNexus, enemyFighter));
            }
        }

        //This method mutates a genotype by swiching two units (making sure that they are the same type so as not to destroy the genotype structure)
        public void Mutate()
        {
            int randomID1 = (int)(UnityEngine.Random.value * genotype.Count);
            int randomID2 = 0;
            while (randomID1 != randomID2)
            {
                randomID2 = (int)(UnityEngine.Random.value * genotype.Count);
            }
            Attackable tempTarget = genotype[randomID1].target;
            genotype[randomID1] = new AttackPair(genotype[randomID1].fighter, genotype[randomID2].target);
            genotype[randomID2] = new AttackPair(genotype[randomID2].fighter, tempTarget);
        }

        /*
            This is the most important part of the genetic algorithm: the Fitness function.
            This function determines how effective a gene is at achieving this algorithm's goal (which at it's core is to maximize damage output while minimizing damage intake)
            A majority of the settings that affect how the AI behaves do it by modifying this fitness function to give certain variables (like the distance or damage output) more weight
        */
        public void CalculateFitness()
        {
            double attackDamage = 0;
            double defenceDamage = 0;
            float totalDistance = 0;

            //Calculates attack and defence damage done by pairs of units fighting eachother as well as the distance each unit has to travel
            foreach (AttackPair pair in genotype)
            {
                if (pair.fighter is Fighter)
                {
                    double fightDamage = CalculateAttackDamage(pair.fighter as Fighter, pair.target);
                    attackDamage += fightDamage * (pair.target is NexusData ? ai.aiSettings.extraDamageWeight : 1);
                }

                if (pair.target is Fighter)
                {
                    double fightDamage = CalculateAttackDamage(pair.target as Fighter, pair.fighter);
                    defenceDamage += fightDamage * (pair.fighter is NexusData ? ai.aiSettings.enemyExtraDamageWeight : 1);
                }

                totalDistance += Vector3.Distance(pair.fighter.position, pair.target.position);
            }

            //The actual fitness equation
            fitness = (ai.aiSettings.damageDealtWeight * attackDamage) - (ai.aiSettings.damageTakenWeight * defenceDamage) - (ai.aiSettings.distanceWeight * totalDistance);
        }

        public double CalculateAttackDamage(Fighter attacker, Attackable defender)
        {
            //This is the equation for the damage that a unit takes. Each armor value offers a 5% damage reduction
            double damage = attacker.damage - (attacker.damage * (defender.armor / 20));

            //This increases the value of the damage if it kills the unit
            if (damage >= defender.health)
            {
                damage *= ai.aiSettings.lethalDamageWeight;
            }
            return damage;
        }

        //This method prints out the contents and fitness of a gene
        public string toString()
        {
            string text = "{\n";
            foreach (AttackPair pair in genotype)
            {
                text += pair.ToString() + "\n";
            }
            return text + "}";
        }
    }

    [System.Serializable]
    public class AISettings
    {
        public int populationSize;
        public int generations;
        public int GPF;
        public int families;
        public float mutationChance;
        public SelectionMethod selectionMethod;
        public ReplacementMethod replacementMethod;
        public float lethalDamageWeight = 1;
        public float damageDealtWeight = 1;
        public float damageTakenWeight = 1;
        public float extraDamageWeight = 2;
        public float enemyExtraDamageWeight = 2;
        public float distanceWeight = 1;
        public AISettings(
            int populationSize,
            int generations,
            int GPF,
            int families,
            float mutationChance,
            SelectionMethod selectionMethod,
            ReplacementMethod replacementMethod,
            float lethalDamageWeight,
            float damageDealtWeight,
            float damageTakenWeight,
            float extraDamageWeight,
            float enemyExtraDamageWeight,
            float distanceWeight
        )
        {
            this.populationSize = populationSize;
            this.generations = generations;
            this.GPF = GPF;
            this.families = families;
            this.mutationChance = mutationChance;
            this.selectionMethod = selectionMethod;
            this.replacementMethod = replacementMethod;
            this.lethalDamageWeight = lethalDamageWeight;
            this.damageDealtWeight = damageDealtWeight;
            this.damageTakenWeight = damageTakenWeight;
            this.extraDamageWeight = extraDamageWeight;
            this.enemyExtraDamageWeight = enemyExtraDamageWeight;
            this.distanceWeight = distanceWeight;
        }
        public AISettings(AISettings other)
        {
            populationSize = other.populationSize;
            generations = other.generations;
            GPF = other.GPF;
            families = other.families;
            mutationChance = other.mutationChance;
            selectionMethod = other.selectionMethod;
            replacementMethod = other.replacementMethod;
            lethalDamageWeight = other.lethalDamageWeight;
            damageDealtWeight = other.damageDealtWeight;
            damageTakenWeight = other.damageTakenWeight;
            extraDamageWeight = other.extraDamageWeight;
            enemyExtraDamageWeight = other.enemyExtraDamageWeight;
            distanceWeight = other.distanceWeight;
        }
    }
    //These are the lists of the units in each team
    public List<Gene> genes;

    public bool isOrange;

    //These are all of the different settings for the AI
    public AISettings aiSettings;

    private bool isActive = false;
    private bool isStarting = true;
    private int currentGeneration = 0;
    private GameController gameController;

    void Awake() 
    {
        //Initializes all of the lists
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        genes = new List<Gene>();
        print("Should be initialized now");
    }

    //This special comparison function helps sort the list of genes by fitness
    int CompareFitnesses(Gene x, Gene y){
        if(x.fitness == y.fitness){
            return 0;
        }
        else if(x.fitness > y.fitness){
            return -1;
        }
        else{
            return 1;
        }
    }

    public void runAI(){
        isActive = true;
    }

    

    void Update()
    {
        if(isStarting){
            //Initializes a randomized initial population of genes
            while (genes.Count < aiSettings.populationSize)
            {
                Gene.blueNexus = new NexusData(gameController.blueNexus.health, false, gameController.blueNexus.transform.position);
                Gene.orangeNexus = new NexusData(gameController.orangeNexus.health, true, gameController.orangeNexus.transform.position);
                Gene.blueFighters = gameController.blueFighters.Select(fighter => new Fighter(fighter.health, fighter.damage, fighter.armor, false, fighter.transform.position, fighter.id)).ToList();
                Gene.orangeFighters = gameController.orangeFighters.Select(fighter => new Fighter(fighter.health, fighter.damage, fighter.armor, true, fighter.transform.position, fighter.id)).ToList();
                genes.Add(new Gene(this));
            }
            isStarting = false;
        }
        if(isActive){
            if (currentGeneration == 0)
            {
                Gene.blueNexus.SetHealth(gameController.blueNexus.health);
                Gene.orangeNexus.SetHealth(gameController.orangeNexus.health);
                Gene.blueFighters = gameController.blueFighters.Select(fighter => new Fighter(fighter.health, fighter.damage, fighter.armor, false, fighter.transform.position, fighter.id)).ToList();
                Gene.orangeFighters = gameController.orangeFighters.Select(fighter => new Fighter(fighter.health, fighter.damage, fighter.armor, true, fighter.transform.position, fighter.id)).ToList();
                foreach (Gene gene in genes)
                {
                    //Reset all of the genes each round
                    gene.Reset();
                    gene.CalculateFitness();
                }
            }
            Gene bestGene;
            bool firstGen = true;
            //Runs each generation
            if(currentGeneration < aiSettings.generations){
                //Limits the amount of generations that will be run per frame according to the GPF value
                while(currentGeneration < aiSettings.generations && (currentGeneration % aiSettings.GPF != 0 || firstGen)){
                    //print("Generation " + (index + 1) + ":");
                    NewGeneration();
                    currentGeneration++;
                    firstGen = false;
                }
            }
            else{
                //Sorts the list of genes and finds the one with the greatest fitness
                genes.Sort(CompareFitnesses);
                bestGene = genes[genes.Count - 1];

                //print("Best Gene: " + bestGene.toString());

                foreach (AttackPair pair in bestGene.genotype)
                {
                    if (pair.fighter is Fighter)
                    {
                        FighterController fighterController = GetFighterController(pair.fighter as Fighter);
                        if (fighterController == null)
                        {
                            continue;
                        }
                        if (pair.target is Fighter)
                        {
                            FighterController enemyController = GetFighterController(pair.target as Fighter);
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
                currentGeneration = 0;
                isActive = false;
            }
        }
    }

    FighterController GetFighterController(Fighter fighter)
    {
        return fighter.isOrange ?
            gameController.orangeFighters.Find(fighterController => fighterController.id == fighter.id) :
            gameController.blueFighters.Find(fighterController => fighterController.id == fighter.id);
    }

    void NewGeneration()
    {
        /*
            Uses Roulette selection to choose parents to breed.
            This selection method takes the sum of all of the fitnesses and multiplies it by a random value between 0 and 1 to get a target value. 
            It then starts at the gene with the highest fitness and goes down the list adding each fitness to a partial sum until the target value is reached. 
            The gene who's fitness reached that target value is then chosen to be a parent.

            This selection method is much more likely to pick genes with higher fitness, but it still gives the weaker genes a chance to be a parent which helps bring diversity to the genepool
        */
        double sumFitness = 0;
        genes.Sort(CompareFitnesses);
        List<Gene> newGenes = new List<Gene>();

        foreach (Gene gene in genes)
        {
            sumFitness += gene.fitness;
            newGenes.Add(gene);
        }

        int[] parentGeneIDs = new int[aiSettings.families * 2];

        for (int i = 0; i < parentGeneIDs.Length; i++)
        {
            if (aiSettings.selectionMethod == SelectionMethod.Roulette)
            {
                double target = sumFitness * UnityEngine.Random.value;
                int index = genes.Count - 1;
                double partialsum = 0;
                while ((target > 0 && partialsum < target) || target < 0 && partialsum > target && index >= 0)
                {
                    partialsum += genes[index].fitness;
                    if ((target > 0 && partialsum >= target) || (target <= 0 && partialsum <= target))
                    {
                        parentGeneIDs[i] = index;
                    }
                    index--;
                }
            }
            else if (aiSettings.selectionMethod == SelectionMethod.Random)
            {
                int randomID = (int)(UnityEngine.Random.value * (genes.Count - 1));
                parentGeneIDs[i] = randomID;
            }
            else
            {
                parentGeneIDs[i] = genes.Count - (i + 1);
            }
        }
        for (int i = 0; i < parentGeneIDs.Length; i += 2)
        {
            //print(parentGenes[i] + " " + parentGenes[i + 1]);
            breedParents(parentGeneIDs[i], parentGeneIDs[i + 1], i, newGenes);
        }
        genes = newGenes;
    }

    void breedParents(int parent1ID, int parent2ID, int familyNumber, List<Gene> newGenes){
        /*
            This method creates two offspring from two parents by selecting two points (A and B) in the genotypes of the parents and copying their genotypes to their children as follows:

            The first parent will give their genotype from the beginning to point A to the first child, from point A to point B to the second child, and from point B to the end to the first child.

            The second parent will give their genotype from the beginning to point A to the second child, from point A to point B to the first child, and from point B to the end to the second child.
        */

        Gene parent1 = genes[parent1ID];
        Gene parent2 = genes[parent2ID];
        if (parent1.genotype.Count > 3)
        {
            int pointA = 0;
            int pointB = 0;
            while (pointA == pointB)
            {
                pointA = (int)(UnityEngine.Random.value * (parent1.genotype.Count - 1));
                pointB = (int)(UnityEngine.Random.value * (parent1.genotype.Count - pointA - 1)) + pointA;
            }
            Gene child1 = new Gene(this);
            Gene child2 = new Gene(this);

            child1.genotype = new List<AttackPair>();
            child2.genotype = new List<AttackPair>();

            for (int i = 0; i < pointA; i++)
            {
                child1.genotype.Add(parent1.genotype[i]);
                child2.genotype.Add(parent2.genotype[i]);
            }
            for (int i = pointA; i < pointB; i++)
            {
                child1.genotype.Add(parent2.genotype[i]);
                child2.genotype.Add(parent1.genotype[i]);
            }
            for (int i = pointB; i < parent1.genotype.Count; i++)
            {
                child1.genotype.Add(parent1.genotype[i]);
                child2.genotype.Add(parent2.genotype[i]);
            }

            //This conditional statement will then potentially mutate one of the children as described earlier.
            if (UnityEngine.Random.value <= aiSettings.mutationChance)
            {
                if (UnityEngine.Random.value <= 0.5)
                {
                    child1.Mutate();
                }
                else
                {
                    child2.Mutate();
                }
            }

            //Then if any of the children are stronger than any of the parents, the stronger children will replace the weaker parents.
            child1.CalculateFitness();
            child2.CalculateFitness();
            if (aiSettings.replacementMethod == ReplacementMethod.WeakestParent)
            {
                if (child1.fitness > parent1.fitness)
                {
                    newGenes[parent1ID] = child1;
                }
                else if (child1.fitness > parent2.fitness)
                {
                    newGenes[parent2ID] = child1;
                }

                if (child2.fitness > parent1.fitness && parent1 != child2)
                {
                    newGenes[parent1ID] = child2;
                }

                if (child2.fitness > parent2.fitness && parent2 != child2)
                {
                    newGenes[parent2ID] = child2;
                }
            }
            else if (aiSettings.replacementMethod == ReplacementMethod.BothParents)
            {
                newGenes[parent1ID] = child2;
                newGenes[parent2ID] = child2;
            }
            else
            {

                if (child1.fitness > genes[familyNumber].fitness)
                {
                    newGenes[familyNumber] = child1;
                }
                else if (child1.fitness > genes[familyNumber + 1].fitness)
                {
                    newGenes[familyNumber + 1] = child1;
                }

                if (child1.fitness > genes[familyNumber].fitness && genes[familyNumber] != child1)
                {
                    newGenes[familyNumber] = child1;
                }
                else if (child1.fitness > genes[familyNumber + 1].fitness)
                {
                    newGenes[familyNumber + 1] = child1;
                }
            }
        }
    }

    //This method is called once the game has ended and it will make all remaining units return to their original position
    public void Stop(){
        isActive = false;
        
    }

    //This method compiles data for the UI
    public string getData(){
        string state = "Idle";
        double bestFitness = 0;
        if(genes.Count > 1){
            bestFitness = genes[genes.Count - 1].fitness;
        }
        if(isActive){
            state = "Thinking";
        }
        string data = "State: " + state + "\nGeneration: " + currentGeneration + "\nBestFitness: " + bestFitness + "\nNexus Health: " + (isOrange ? gameController.orangeNexus : gameController.blueNexus).health;
        return data;
    }
}
