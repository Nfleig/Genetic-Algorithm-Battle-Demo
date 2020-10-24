using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FighterController : MonoBehaviour
{
    private GameObject[] gameControllers;
    private NavMeshAgent agent;
    private LineRenderer line;

    private float ox;
    private float oz;
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

    private bool fighting;
    private bool textVisible = false;
    private bool clicked = false;

    private GameObject[] attackTargets;
    private GameObject statLabel;
    private GameObject camera;

    private float deathTimer;
    private float attackTimer;
    // Start is called before the first frame update
    void Start()
    {
        while(gameControllers == null){
            gameControllers = GameObject.FindGameObjectsWithTag("AI");
        }
        attackTargets = new GameObject[2];
        ox = transform.position.x;
        oz = transform.position.z;
        deathTimer = deathTime;
        attackTimer = 0;
        dead = false;
        blocked = false;
        health = (int) (Random.value * (maxHealth - 1)) + 1;
        damage = (int) (Random.value * (maxDamage - 1)) + 1;
        armor = (int) (Random.value * (maxArmor - 1)) + 1;
        foreach(GameObject gameController in gameControllers){
            gameController.GetComponent<AI>().AddFighter(this, defender);
        }
        agent = GetComponent<NavMeshAgent>();
        agent.destination = transform.position;
        line = GetComponent<LineRenderer>();
        statLabel = transform.GetChild(0).gameObject;
        camera = GameObject.FindWithTag("Camera");
    }

    // Update is called once per frame
    void Update()
    {
        if(textVisible || clicked){
            statLabel.transform.eulerAngles = new Vector3(40, camera.transform.eulerAngles.y, 0);
            statLabel.GetComponent<TextMesh>().text = "Health: " + health + "\nDamage: " + damage + "\nArmor: " + armor;
        }
        if(health <= 0){
            Die();
            agent.destination = Target.transform.position;
            deathTimer -= Time.fixedDeltaTime;
            if(deathTimer < 0){
                Destroy(gameObject);
            }
        }
        else{
            if(Target && Target != gameObject){
                agent.destination = Target.transform.position;
            }
            if(fighting){
                attackTimer -= Time.fixedDeltaTime;
                if(attackTimer < 0){
                    Attack();
                    attackTimer = attackDelay;
                }
            }
            blocked = false;
            Vector3[] points = agent.path.corners;
            //print(points.Length);
            line.SetPositions(points);
        }
    }

    public void Fight(FighterController target){
        blocked = true;
        Target = target.gameObject;
    }

    public void Fight(Nexus target){
        Target = target.gameObject;
    }

    void OnTriggerEnter(Collider other){
        //print("Trigger");
        if(!dead){
            if(other.gameObject == Target){
                fighting = true;
                attackTargets[0] = other.gameObject;
            }
            else{
                FighterController otherFighter = other.gameObject.GetComponent<FighterController>();
                if(otherFighter){
                    if(otherFighter.Target == gameObject){
                        fighting = true;
                        attackTargets[1] = other.gameObject;
                    }
                }
            }
        }
    }

    void OnTriggerExit(Collider other){
        for(int i = 0; i < attackTargets.Length; i++){
            if(other.gameObject == attackTargets[i]){
                attackTargets[i] = gameObject;
            }
        }
    }

    void Attack(){
        if(!dead){
            foreach(GameObject target in attackTargets){
                if(target && target != gameObject){
                    FighterController otherFighter = target.gameObject.GetComponent<FighterController>();
                    Nexus otherNexus = target.gameObject.GetComponent<Nexus>();
                    if(otherFighter){
                        otherFighter.health -= (damage - (damage * (otherFighter.armor / 20)));
                    }
                    if(otherNexus){
                        otherNexus.health -= damage;
                    }
                }
            }
        }
    }
    
    public void Die(){
        if(!dead){
            foreach(GameObject gameController in gameControllers){
                //print(gameControllers.Length);
                gameController.GetComponent<AI>().RemoveFighter(this, defender);
            }
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            renderer.material = deathMaterial;
            dead = true;
            Target = gameObject;
        }
    }

    public void Reset(){
        Target = gameObject;
        agent.destination = new Vector3(ox, transform.position.y, oz);
    }

    void OnMouseDown(){
        clicked = !clicked;
        statLabel.SetActive(clicked);
    }

    void OnMouseEnter(){
        statLabel.SetActive(true);
        textVisible = true;
    }

    void OnMouseExit(){
        if(!clicked){
            statLabel.SetActive(false);
        }
        textVisible = false;
    }
}
