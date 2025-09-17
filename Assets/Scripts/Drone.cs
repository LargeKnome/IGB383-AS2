using UnityEngine;
using System.Collections;
using Unity.Mathematics;

public class Drone : Enemy {

    GameManager gameManager;

    Rigidbody rb;

    //Movement & Rotation Variables
    public float speed = 50.0f;
    private float rotationSpeed = 5.0f;
    private float adjRotSpeed;
    private Quaternion targetRotation;
    public GameObject target;
    public float targetRadius = 200f;

    //Boid Steering/Flocking Variables
    [SerializeField] float seperationDistance;
    [SerializeField] float cohesionDistance;
    [SerializeField] float seperationStrength;
    [SerializeField] float cohesionStrength;
    Vector3 cohesionPos = Vector3.zero;
    int boidIndex = 0;

    // Use this for initialization
    void Start() {

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {

        //Acquire player if spawned in
        if (gameManager.gameStarted)
            target = gameManager.playerDreadnaught.transform.GetChild(0).gameObject;

        //Move towards valid targets
        if(target)
            MoveTowardsTarget();

        BoidBehaviour();
    }

    private void MoveTowardsTarget() {
        //Rotate and move towards target if out of range
        if (Vector3.Distance(target.transform.position, transform.position) > targetRadius) {

            //Lerp Towards target
            targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
            adjRotSpeed = Mathf.Min(rotationSpeed * Time.deltaTime, 1);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, adjRotSpeed);

            rb.AddRelativeForce(Vector3.forward * speed * 20 * Time.deltaTime);
        }
    }

    private void BoidBehaviour()
    {
        boidIndex++;
        if (boidIndex >= gameManager.enemyList.Length)
        {
            Vector3 cohesiveForce = (cohesionStrength / Vector3.Distance(cohesionPos, transform.position)) * (cohesionPos - transform.position);

            rb.AddForce(cohesiveForce); 
            boidIndex = 0;
            cohesionPos = Vector3.zero;
        }

        Vector3 pos = gameManager.enemyList[boidIndex].transform.position;
        Quaternion rot = gameManager.enemyList[boidIndex].transform.rotation;
        float dist = Vector3.Distance(transform.position, pos);

        if (dist > 0f) 
        {
            if (dist <= seperationDistance)
            {
                float scale = seperationStrength / dist;
                rb.AddForce(scale * Vector3.Normalize(transform.position - pos));
            }
            else if (dist < cohesionDistance && dist > seperationDistance)
            {
                cohesionPos = cohesionPos + pos * (1f / (float)gameManager.enemyList.Length);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, 1f);
            }
        }

    }

}
