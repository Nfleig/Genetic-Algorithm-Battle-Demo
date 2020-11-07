# Genetic-Algorithm
 
INTRODUCTION:

This demo simulates a battle between two armies that are controlled by Genetic Algorithms designed to find the best possible strategy to win. You can modify the variables of the blue army's AI and see how they affect what the AI does.

CONTROLS:

ESC - Open/Close Menu
WASD/Arrow Keys - Move the camera.
Right Click - Rotate the camera

Hover over units to see their attributes, left click on a unit to pin it's attribute display.

WRITEUP:
Below is a full in-depth writeup of exactly how the AI works:


//////////////////////////
    GENETIC ALGORITHM
        WRITEUP
//////////////////////////


The Scenario:
	In this simulation there are two armies (one blue and one orange) that are both made up of fighters. Each fighter has 3 randomized attributes: Health, Damage, and Armor. Health  determines how much damage a unit can take before dying, Damage determines how much damage a unit deals when they attack, and Armor determines how much damage reduction a unit gets with each unit of Armor offering a 5% damage reduction. Behind each army is their Nexus, a large object that acts as the army’s core and can be attacked by fighters from the other side. An army loses if their Nexus is destroyed or if all of their fighters are killed.

The AI:
	Both armies have an AI that controls them and assigns fighters to attack enemies. Each fighter will be assigned to fight an enemy, and if there are more fighters than enemies the extra fighters will be assigned to attack the Nexus. To determine which fighters will attack which enemies the AI uses a Genetic Algorithm to generate and breed different chromosomes to form the best strategy.

The Chromosomes:
	In this situation each chromosome represents a possible strategy that the AI can use. The chromosome is structured as a list of units where each unit in the AI’s army is listed with their target being the next entry so that the chromosome will appear as follows:

{Friendly Unit 1, Enemy Unit 1, Friendly Unit 2, Enemy Unit 2, Friendly Unit 3, Enemy Unit 3, ...}

Friendly Unit 1 will be assigned to attack Enemy Unit 1, Friendly Unit 2 will attack Enemy Unit 2, and so on. If there are more units in one army than the other, any units that aren’t assigned to fight another unit will simply be added onto the end of the list so that they can be assigned to fight their enemy Nexus later.

The Fitness Algorithm:
	In addition to their list of units each chromosome also stores a fitness value corresponding to how good it’s strategy is which is found using a fitness algorithm. In this particular AI the fitness algorithm in its most basic form is as follows:

fitness = Total Damage Dealt - Total Damage Received - Total Distance Traveled

As can be seen, the AI’s goal at any given point is not actually to win, but rather to deal as much damage as possible while taking as little damage as possible and travelling as little distance as possible. While this simple formula encapsulates the basic goal of the AI, there is much more that needs to be added to account for the intricacies of the scenario and allow for the user to customize the AI. Here is an in-depth view of how these values are found and modified to factor into the formula and produce an effective fitness value.

Total Damage Dealt:
	To find the total damage dealt by the AI’s army, the fitness function first goes through each matchup of units and calculates the damage that the fighter will do to the enemy using this equation:

Final Damage = Fighter Damage - (Fighter Damage * (Enemy Armor / 20))

This is then added to the total damage, but first the function checks if the Final Damage value is greater than the enemy’s health (meaning that the damage would kill the enemy), if it is then it is multiplied by the Lethal Damage Weight. The purpose of this is to allow the AI to take lethal damage into account when calculating which units will fight each other.

Total Damage Received:
	To find the damage received by the AI’s army the same damage function is used from Total Damage Dealt but with the roles reversed so that the AI’s fighter is the one being attacked by the enemy unit. 

Total Distance Traveled:
	To find the total distance that each unit travels this function can use one of two methods based on the power of the user’s computer:

	Basic Distance:
	This method simply uses the distance between the two units to calculate the Total Distance Traveled value without accounting for obstacles and paths. This method is less accurate, but it is far more efficient than the other method and in cases of a simple battlefield that is mostly just a flat surface with a few obstacles it will not be too inaccurate.

	Pathfinding Method:
	This method uses Unity’s built in pathfinding algorithm to generate a path from the AI’s fighter to the enemy. It then goes through the path and calculates the total distance. This method is more accurate than the Basic Distance, however it is much more burdensome and it can greatly increase computation time for the AI and lag for the whole simulation, so in most cases using Basic Distance is recommended unless the battlefield is extremely complex.

Once all of these values are calculated they are multiplied by user-defined weight values to give the AI it’s own “personality” and way of calculating strategy. Total Damage Dealt is multiplied by an “aggression” value so that the higher an AI’s aggression is set, the more it will care about dealing damage. At the same time Total Damage Received is multiplied by the inverse of the aggression value. This way an AI with a higher aggression value will care about damage dealt more and damage taken less, while an AI with a lower aggression will care more about damage taken than damage dealt. Then Total Distance Traveled is multiplied by its own separate weight value to determine how much the AI cares about the distance between units. 

The Selection Algorithm:
	Once fitness values have been assigned to each chromosome it is time to determine which chromosomes will be allowed to breed. To allow for user choice three selection methods have been integrated, with the user being able to choose which one the AI will use:

Best Fitness Selection:
	This selection method is by far the simplest. The AI simply sorts the list of chromosomes by fitness and chooses all of the most fit chromosomes to breed. At first this method may seem like it is easily the best as the best chromosomes will have the best genotypes to give to their children, however unfortunately selecting chromosomes is not that simple. While this method does produce quality chromosomes extremely quickly it also completely eliminates any diversity in the process. Within a few generations the gene pool will become completely saturated with minor variations of the same genotype, and while this genotype may be good, there is also the possibility that it will not be the best genotype, which is why maintaining diversity is so important. It is always possible that while a chromosome may not have a high fitness in its current state, it could have the potential to unlock a whole new and better genotype that will make the entire genepool stronger if it is given the chance at breeding. That is not to say that this is necessarily a bad selection algorithm, it just may not be the best possible choice in any given situation.

Random Selection:
	This method is on the complete opposite side of the spectrum from the Best Fitness method. In it the AI will choose chromosomes to breed completely randomly with no account for fitness. This means that the genepool will not have any problems with diversity, but instead it will have the opposite problem of potentially being too random to make progress. It’s difficult to refine an already good genotype if it never gets chosen, and instead this method runs the risk of potentially filling the genepool with nothing but weak or average chromosomes because the good chromosomes were completely ignored. Once again that is not to say that this is necessarily a bad selection method, It just once again might not be the best.

Roulette Wheel Selection:
	This method combines the other two selection algorithms to create a weighted random selection. To do this the AI first sorts the chromosomes and finds the sum of all of their fitnesses. It then multiplies that sum by a random number between 0 and 1 (inclusive) to find a target value. Then the AI will go through each chromosome starting with the one with the highest fitness and it will add each chromosome’s fitness to a partial sum value. It will keep doing this until the target value is reached, with the chromosome whose fitness reached the target value being chosen. This method means that the selection of chromosomes is still random but each chromosome will be given a certain weight based on their fitness, with stronger chromosomes being given more weight. 


Crossover algorithm:
	Once chromosomes have been chosen to breed, It is then time for them to actually create children. This is done by creating two new empty children, labelled child 1 and child 2, and assign their genotypes based on the genetic data of their parents. to do this, first two points in the genotype of the parents, labelled A and B, are chosen. A will be a random point between the first and last entry in the genotype (see the chromosomes section for more information about how the genotype is formatted), and B is somewhere between A and the end of the genotype. If points A and B are the same then this process will restart and loop until they are not the same. Once these points are found, then the children are generated as follows:

Child 1 will receive all of it’s genetic data from the beginning of the genotype to point A from parent 1, then it will receive it’s genotype from point A to point B from parent 2, then finally it will receive it’s genetic data from point B to the end of the genotype from parent 1.
Child 2 will receive all of it’s genetic data from the beginning of the genotype to point A from parent 2, then it will receive it’s genotype from point A to point B from parent 1, then finally it will receive it’s genetic data from point B to the end of the genotype from parent 2.

Once this process is complete there will now be a family of two parents and two children who both receive genetic data from the parents. Now it must be decided which members of this family will rejoin the gene pool and which ones will be deleted. This is done in one of three ways depending on the users choice:

Weakest Parent Replacement: If either of the children are stronger than either of the parents they will replace that parent.

Both Parents Replacement: Both of the parents will be replaced by their children regardless of fitness.

Weakest Chromosome Replacement: Both of the children will replace the two weakest chromosomes in the genepool.

Completing the Algorithm:
	Once all of the previous steps have been completed and the new children have been introduced to the genepool, one generation has been completed. From there the AI then selects new parents as described in the Selection Algorithms section and the breeding process repeats. After one thousand generations (or as many as the user wants) the hope is that one of the chromosomes will have a refined and high quality strategy for the AI to use. Once this point has been reached the AI will pick the chromosome with the highest fitness and execute its strategy for 10 seconds before restarting the entire process to adapt its strategy.
