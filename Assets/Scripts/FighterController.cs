using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FighterController : MonoBehaviour
{
    // Public Properties
    public double health;
    public int damage;
    public int armor;
    public bool defender;
    public float deathTime;
    public float attackDelay;
    public bool blocked;
    public bool dead;
    public GameObject Target;
    public Material deathMaterial;
    public double maxHealth;
    public int maxDamage;
    public int maxArmor;

    // Private properties
    private GameObject[] AIs;
    private NavMeshAgent agent;
    private LineRenderer line;
    private bool fighting;
    private bool textVisible = false;
    private bool clicked = false;
    private GameObject[] attackTargets;
    private GameObject statLabel;
    private GameObject cameraObject;
    private float deathTimer;
    private float attackTimer;
    private Vector2 originalPosition;

    /*
     * This function initializes the fighter
     */

    void Start()
    {
        // Find all of the AIs

        while(AIs == null){
            AIs = GameObject.FindGameObjectsWithTag("AI");
        }

        // Find the fighter's starting position

        originalPosition = new Vector2(transform.position.x, transform.position.z);

        attackTargets = new GameObject[2];
        deathTimer = deathTime;
        attackTimer = 0;
        dead = false;
        blocked = false;

        // Find the AI's stats

        health = (int) (Random.value * (maxHealth - 1)) + 1;
        damage = (int) (Random.value * (maxDamage - 1)) + 1;
        armor = (int) (Random.value * (maxArmor - 1)) + 1;
        
        // Register the fighter with all AIs
        
        foreach(GameObject AI in AIs){
            AI.GetComponent<AI>().AddFighter(this, defender);
        }

        // Set up the pathfinder

        agent = GetComponent<NavMeshAgent>();
        agent.destination = transform.position;
        
        line = GetComponent<LineRenderer>();
        statLabel = transform.GetChild(0).gameObject;
        cameraObject = GameObject.FindWithTag("Camera");
    }

    /*
     * This is the main gameplay loop
     */
    void Update()
    {
        // If the text is showing then rotate it towards the camera

        if(textVisible || clicked){
            statLabel.transform.eulerAngles = new Vector3(40, cameraObject.transform.eulerAngles.y, 0);
            statLabel.GetComponent<TextMesh>().text = "Health: " + health + "\nDamage: " + damage + "\nArmor: " + armor;
        }

        // If the health is below zero then kill the fighter

        if(health <= 0){
            Die();
            agent.destination = Target.transform.position;
            deathTimer -= Time.fixedDeltaTime;
            if(deathTimer < 0){
                Destroy(gameObject);
            }
        }
        else{
            
            // If the fighter has a target then move towards that target

            if(Target && Target != gameObject){
                agent.destination = Target.transform.position;
            }

            // If the fighter is fighting someone then attack at regular intervals

            if(fighting){
                attackTimer -= Time.fixedDeltaTime;
                if(attackTimer < 0){
                    Attack();
                    attackTimer = attackDelay;
                }
            }

            // Update the line

            blocked = false;
            Vector3[] points = agent.path.corners;
            line.SetPositions(points);
        }
    }

    /*
     * This function can be used to make the fighter target another fighter
     */

    public void Fight(FighterController target){
        blocked = true;
        Target = target.gameObject;
    }

    /*
     * This function can be used to make the fighter target a nexus
     */

    public void Fight(Nexus target){
        Target = target.gameObject;
    }

    /*
     * This function is called when the fighter collides with something
     */

    void OnTriggerEnter(Collider other) {
        
        // If the fighter is dead then don't do anything

        if(!dead) {

            // If the fighter is touching it's target then start fighting it
            if(other.gameObject == Target){
                fighting = true;
                attackTargets[0] = other.gameObject;
            }
            else{

                // Get the fighter that is touching us

                FighterController otherFighter = other.gameObject.GetComponent<FighterController>();
                if(otherFighter){

                    // If that other fighter is fighting us then fight back

                    if(otherFighter.Target == gameObject){
                        fighting = true;
                        attackTargets[1] = other.gameObject;
                    }
                }
            }
        }
    }

    /*
     * This function is called when the fighter stops colliding with something
     */

    void OnTriggerExit(Collider other){
        for(int i = 0; i < attackTargets.Length; i++){
            if(other.gameObject == attackTargets[i]){
                attackTargets[i] = gameObject;
            }
        }
    }

    /*
     * This function can be called to make the fighter attack all of it's targets
     */

    void Attack(){

        // If the fighter is dead then don't do anything

        if(!dead){

            // Iterate through every target

            foreach(GameObject target in attackTargets){

                // If the target exists then attack it

                if(target && target != gameObject){

                    // Get the other fighter/nexus

                    FighterController otherFighter = target.gameObject.GetComponent<FighterController>();
                    Nexus otherNexus = target.gameObject.GetComponent<Nexus>();
                    
                    // Reduce the health of the other fighter using a formula based on our stats

                    if(otherFighter){
                        otherFighter.health -= (damage - (damage * (otherFighter.armor / 20)));
                    }

                    // Reduce the health of the nexus
                    if(otherNexus){
                        otherNexus.health -= damage;
                    }
                }
            }
        }
    }
    
    /*
     * This function is used to kill a fighter
     */

    public void Die(){

        // If the fighter is already dead then don't kill it again

        if(!dead) {

            // Deregister the fighter with all AIs
            foreach (GameObject AI in AIs) {
                AI.GetComponent<AI>().RemoveFighter(this, defender);
            }

            // Turn the dying fighter red

            MeshRenderer renderer = GetComponent<MeshRenderer>();
            renderer.material = deathMaterial;

            dead = true;

            // Stop the pathfinder

            Target = gameObject;
        }
    }

    /*
     * This function will make the fighter pathfind back to it's starting position
     */

    public void Reset(){
        Target = gameObject;
        agent.destination = new Vector3(originalPosition.x, transform.position.y, originalPosition.y);
    }

    /*
     * This function will toggle the stats label whenever the fighter is clicked
     */

    void OnMouseDown(){
        clicked = !clicked;
        statLabel.SetActive(clicked);
    }

    /*
     * This function will show the stats label whenever the mouse hovers over the fighter
     */

    void OnMouseEnter(){
        statLabel.SetActive(true);
        textVisible = true;
    }

    /*
     * This function will hide the stats label when the mouse stops hovering over it
     */

    void OnMouseExit(){
        if(!clicked){
            statLabel.SetActive(false);
        }
        textVisible = false;
    }
}
