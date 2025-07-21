﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{

    public enum SelectionMethod
    {
        Roulette,
        Random,
        BestFit
    }

    public enum ReplacementMethod
    {
        WeakestParent,
        BothParents,
        WeakestGene
    }

    /*
        This class represents one chromosome and includes all of the data and methods that it needs
    */
    public class Gene
    {
        //These two are just lists of all of the units on each side that the Gene will use to create it's genotype later
        public List<FighterController> fighters;
        public List<FighterController> defenders;
        public List<FighterController> genotype;
        public double fitness;

        //These values help the AI execute the genotype's instructions if this gene is chosen
        public int matchups;
        private bool MoreAttackers;
        private AI ai;

        //Initializer assigns all of the gene's data and then generates a random genotype
        public Gene(List<FighterController> fighters, List<FighterController> defenders, AI ai)
        {
            this.fighters = fighters;
            this.defenders = defenders;
            this.ai = ai;
            genotype = new List<FighterController>();
            fitness = 0;
            Randomize();
            CalculateFitness();
        }

        //Generates a new random genotype
        public void Reset()
        {
            genotype = new List<FighterController>();
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
            // Create temporary lists of fighters and defenders
            List<FighterController> tempFighters = new List<FighterController>();
            foreach (FighterController fighter in fighters)
            {
                tempFighters.Add(fighter);
            }

            List<FighterController> tempDefenders = new List<FighterController>();
            foreach (FighterController defender in defenders)
            {
                tempDefenders.Add(defender);
            }

            //Determines whether there are more fighters or defenders
            MoreAttackers = fighters.Count > defenders.Count;

            matchups = 0;

            //Assigns matchups to be the amount of pairs of units that will fight eachother
            if (MoreAttackers)
            {
                matchups = defenders.Count;
            }
            else
            {
                matchups = fighters.Count;
            }

            /*
                This next section actually goes through and creates the genotype based on these rules:

                    If the index is less than matchups (meaning that the program is assigning pairs of units to fight eachother) then it will alternate between adding a fighter and a defender to the genotype

                    If the index is greater than matchups and less than the total amount of units (meaning that the program is adding on extra units that arent assigned to another unit) then the rest of the units will be added onto the end

                These rules mean that the genotype is in the structure defined above with the extra units just being tacked onto the end of the genotype
            */
            while (tempFighters.Count > 0 || tempDefenders.Count > 0)
            {
                if (tempDefenders.Count == 0 || (tempFighters.Count > 0 && genotype.Count % 2 == 0))
                {
                    int randomID = (int)(Random.value * (tempFighters.Count - 1));
                    genotype.Add(tempFighters[randomID]);
                    tempFighters.RemoveAt(randomID);
                    continue;
                }

                if (tempFighters.Count == 0 || (tempDefenders.Count > 0 && genotype.Count % 2 == 1))
                {
                    int randomID = (int)(Random.value * (tempDefenders.Count - 1));
                    genotype.Add(tempDefenders[randomID]);
                    tempDefenders.RemoveAt(randomID);
                }
            }
        }

        //This method mutates a genotype by swiching two units (making sure that they are the same type so as not to destroy the genotype structure)
        public void Mutate()
        {
            int randomID1 = (int)(Random.value * genotype.Count);
            int randomID2 = 0;
            while (randomID1 != randomID2 && (genotype[randomID1].defender = genotype[randomID2].defender))
            {
                randomID2 = (int)(Random.value * genotype.Count);
            }
            FighterController temp = genotype[randomID1];
            genotype[randomID1] = genotype[randomID2];
            genotype[randomID2] = temp;
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
            for (int i = 0; i < matchups - 1; i += 2)
            {
                attackDamage += CalculateAttackDamage(genotype[i], genotype[i + 1]);
                defenceDamage += CalculateAttackDamage(genotype[i + 1], genotype[i]);
                totalDistance += CalculateDistance(genotype[i], genotype[i + 1].gameObject, ai.usePathfinding);
            }

            //Calculates any extra damage done by units that arent assigned to fight another unit and instead will fight the enemy nexus
            for (int i = matchups; i < genotype.Count; i++)
            {
                if (MoreAttackers)
                {
                    attackDamage += genotype[i].damage * ai.extraDamageWeight;

                    //determines the distance as well
                    totalDistance += CalculateDistance(genotype[i], ai.enemyNexus.gameObject, ai.usePathfinding);
                }
                else
                {
                    defenceDamage += genotype[i].damage * ai.enemyExtraDamageWeight;
                }
            }

            //The actual fitness equation
            fitness = (ai.damageDealtWeight * attackDamage) - (ai.damageTakenWeight * defenceDamage) - (ai.distanceWeight * totalDistance);

            //The matchups value tells the AI how many sets of fighters and defenders there are (which helps when actually executing the instructions)
            matchups = 0;
            for (int i = 0; i < genotype.Count - 1; i++)
            {
                if ((!genotype[i].defender && genotype[i + 1].defender && !ai.enemy) || (genotype[i].defender && !genotype[i + 1].defender && ai.enemy))
                {
                    matchups++;
                }
            }
        }

        public float CalculateDistance(FighterController attacker, GameObject defender, bool usePathfinding)
        {
            if (usePathfinding)
            {
                //This algorithm uses the NavMeshAgents within the units to find the length of the path that it will take
                //This method was extremely inefficient so I made it optional
                NavMeshAgent agent = attacker.gameObject.GetComponent<NavMeshAgent>();
                Vector3 target = defender.transform.position;
                NavMeshPath path = new NavMeshPath();
                agent.CalculatePath(target, path);
                float distance = 0;
                for (int i = 1; i < path.corners.Length; i++)
                {
                    distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
                }
                return distance;
            }
            else
            {
                //This method just uses the distance between the two units which is way more efficient but slightly less accurate if there are obsticles that the unit needs to avoid
                return Vector3.Distance(attacker.gameObject.transform.position, defender.transform.position);
            }
        }

        public double CalculateAttackDamage(FighterController attacker, FighterController defender)
        {
            //This is the equation for the damage that a unit takes. Each armor value offers a 5% damage reduction
            double damage = (attacker.damage - (attacker.damage * (defender.armor / 20)));

            //This increases the value of the damage if it kills the unit
            if (damage >= defender.health)
            {
                damage *= ai.lethalDamageWeight;
            }
            return damage;
        }

        //This method prints out the contents and fitness of a gene
        public string toString()
        {
            string text = "";
            foreach (FighterController fighter in genotype)
            {
                text += (fighter + ", ");
            }
            text += ("\nFitness: " + fitness);
            return text;
        }
    }
    //These are the lists of the units in each team
    public List<FighterController> fighters;
    public List<FighterController> defenders;
    public List<Gene> genes;

    public Nexus enemyNexus;
    public Nexus nexus;

    public bool enemy;

    //These are all of the different settings for the AI
    public int populationSize;
    public int generations;
    public int GPF;
    public int families;
    public double mutationChance;
    public SelectionMethod selectionMethod;
    public ReplacementMethod replacementMethod;

    //And these are the different weights that affect the fitness formula
    public double lethalDamageWeight = 1;
    public double damageDealtWeight = 1;
    public double damageTakenWeight = 1;
    public double extraDamageWeight = 2;
    public double enemyExtraDamageWeight = 2;
    public double distanceWeight = 1;
    public bool usePathfinding = false;

    void Awake() 
    {
        //Initializes all of the lists
        fighters = new List<FighterController>();
        defenders = new List<FighterController>();
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
        active = true;
    }

    

    bool active = false;
    bool start = true;
    int index = 0;

    void Update()
    {
        if(start){
            //Initializes a randomized initial population of genes
            while(genes.Count < populationSize){
                genes.Add(new Gene(fighters, defenders, this));
            }
            start = false;
        }
        if(active){
            foreach(Gene gene in genes){
                //Reset all of the genes each round
                gene.fighters = fighters;
                gene.defenders = defenders;
                gene.Reset();
                gene.CalculateFitness();
            }
            Gene bestGene;
            bool firstGen = true;
            //Runs each generation
            if(index < generations){
                //Limits the amount of generations that will be run per frame according to the GPF value
                while(index < generations && (index % GPF != 0 || firstGen)){
                    //print("Generation " + (index + 1) + ":");
                    NewGeneration();
                    index++;
                    firstGen = false;
                }
            }
            else{
                //Sorts the list of genes and finds the one with the greatest fitness
                genes.Sort(CompareFitnesses);
                bestGene = genes[genes.Count - 1];

                //print("Best Gene: " + bestGene.toString());

                //Goes through and actually executes the instructions in the gene
                for(int i = 0; i < bestGene.matchups * 2; i += 2){
                    bestGene.genotype[i].Fight(bestGene.genotype[i + 1]);
                }
                foreach(FighterController fighter in fighters){
                    if(!fighter.blocked){
                        fighter.Fight(enemyNexus);
                    }
                }
                index = 0;
                active = false;
            }
        }
    }

    void NewGeneration(){
        /*
            Uses Roulette selection to choose parents to breed.
            This selection method takes the sum of all of the fitnesses and multiplies it by a random value between 0 and 1 to get a target value. 
            It then starts at the gene with the highest fitness and goes down the list adding each fitness to a partial sum until the target value is reached. 
            The gene who's fitness reached that target value is then chosen to be a parent.

            This selection method is much more likely to pick genes with higher fitness, but it still gives the weaker genes a chance to be a parent which helps bring diversity to the genepool
        */

        /*
         * Issues:
         *      - Genes can be selected multiple times at once
         *      - Genes can be replaced by children before they've reproduced, allowing the children to reproduce for free
         */
        double sumFitness = 0;
        genes.Sort(CompareFitnesses);
        
        foreach (Gene gene in genes)
        {
            sumFitness += gene.fitness;
        }

        Gene[] parentGenes = new Gene[families * 2];
        
        for (int i = 0; i < parentGenes.Length; i++)
        {
            if (selectionMethod == SelectionMethod.Roulette)
            {
                double target = sumFitness * Random.value;
                int index = genes.Count - 1;
                double partialsum = 0;
                while ((target > 0 && partialsum < target) || (target < 0 && partialsum > target) && index >= 0)
                {
                    partialsum += genes[index].fitness;
                    if ((target > 0 && partialsum >= target) || (target <= 0 && partialsum <= target))
                    {
                        parentGenes[i] = genes[index];
                    }
                    index--;
                }
            }
            else if (selectionMethod == SelectionMethod.Random)
            {
                int randomID = (int)(Random.value * (genes.Count - 1));
                parentGenes[i] = genes[randomID];
            }
            else
            {
                //If the families value is greater than the population size this conditional prevents the game from crashing
                if (i >= genes.Count - 1)
                {
                    parentGenes[i] = genes[0];
                }
                else
                {
                    parentGenes[i] = genes[genes.Count - (i + 1)];
                }
            }
        }
        for(int i = 0; i < parentGenes.Length; i += 2)
        {
            //print(parentGenes[i] + " " + parentGenes[i + 1]);
            breedParents(parentGenes[i], parentGenes[i + 1], i);
        }
    }

    void breedParents(Gene parent1, Gene parent2, int familyNumber){
        /*
            This method creates two offspring from two parents by selecting two points (A and B) in the genotypes of the parents and copying their genotypes to their children as follows:

            The first parent will give their genotype from the beginning to point A to the first child, from point A to point B to the second child, and from point B to the end to the first child.

            The second parent will give their genotype from the beginning to point A to the second child, from point A to point B to the first child, and from point B to the end to the second child.
        */
        
        /*
         * Issues:
         *      - Method of copying genetic material to children is super unsafe. Could destroy genetic structure
         *      - Parents don't actually get replaced in the WEAKEST_PARENT and BOTH_PARENTS replacement methods
         */
        if (parent1.genotype.Count > 3)
        {
            int pointA = 0;
            int pointB = 0;
            while (pointA == pointB)
            {
                pointA = (int)(Random.value * (parent1.genotype.Count - 1));
                pointB = (int)(Random.value * (parent1.genotype.Count - pointA - 1)) + pointA;
            }
            Gene child1 = new Gene(fighters, defenders, this);
            Gene child2 = new Gene(fighters, defenders, this);

            child1.genotype = new List<FighterController>();
            child2.genotype = new List<FighterController>();

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
            if (Random.value <= mutationChance)
            {
                if (Random.value <= 0.5)
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
            if (replacementMethod == ReplacementMethod.WeakestParent)
            {
                if (child1.fitness > parent1.fitness)
                {
                    parent1 = child1;
                }
                else if (child1.fitness > parent2.fitness)
                {
                    parent2 = child1;
                }

                if (child2.fitness > parent1.fitness && parent1 != child2)
                {
                    parent1 = child2;
                }

                if (child2.fitness > parent2.fitness && parent2 != child2)
                {
                    parent2 = child2;
                }
            }
            else if (replacementMethod == ReplacementMethod.BothParents)
            {
                parent1 = child2;
                parent2 = child2;
            }
            else
            {

                if (child1.fitness > genes[familyNumber].fitness)
                {
                    genes[familyNumber] = child1;
                }
                else if (child1.fitness > genes[familyNumber + 1].fitness)
                {
                    genes[familyNumber + 1] = child1;
                }

                if (child1.fitness > genes[familyNumber].fitness && genes[familyNumber] != child1)
                {
                    genes[familyNumber] = child1;
                }
                else if (child1.fitness > genes[familyNumber + 1].fitness)
                {
                    genes[familyNumber + 1] = child1;
                }
            }
        }
    }

    //This method is called by every unit as they are created and it will add them to their respective list according to which side they're on
    public void AddFighter(FighterController fighter, bool isDefender)
    {
        if(isDefender != enemy)
        {
            if(fighter != null)
            {
                defenders.Add(fighter);
            }
        }
        else
        {
            if(fighter != null && fighters != null)
            {
                fighters.Add(fighter);
            } else {
                print("Something's gone wrong");
            }
        }
    }

    //This method is called by every unit as they die and it will remove them from their respective list
    public void RemoveFighter(FighterController fighter, bool isDefender){
        if((isDefender && !enemy) || (!isDefender && enemy)){
            defenders.Remove(fighter);
            foreach(Gene gene in genes){
                gene.defenders.Remove(fighter);
            }
            //print("Removed defender: " + fighter);
        }
        else if((!isDefender && !enemy) || (isDefender && enemy)){
            fighters.Remove(fighter);
            foreach(Gene gene in genes){
                gene.fighters.Remove(fighter);
            }
            //print("Removed fighter: " + fighter);
        }
    }

    //This method is called once the game has ended and it will make all remaining units return to their original position
    public void Stop(){
        active = false;
        foreach(FighterController fighter in fighters){
            fighter.Reset();
        }
    }

    //This method compiles data for the UI
    public string getData(){
        string state = "Idle";
        double bestFitness = 0;
        if(genes.Count > 1){
            bestFitness = genes[genes.Count - 1].fitness;
        }
        if(active){
            state = "Thinking";
        }
        string data = "State: " + state + "\nGeneration: " + index + "\nBestFitness: " + bestFitness + "\nNexus Health: " + nexus.health;
        return data;
    }
}
