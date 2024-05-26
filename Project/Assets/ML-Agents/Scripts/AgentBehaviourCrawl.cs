using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using ALJV;

public class AgentBehavoirCrawl : Agent
{
    [SerializeField] private bool heuristic;
    [SerializeField] private bool inTrain;
    [SerializeField] private Transform targetTransform;
    private Transform agentTransform;
    private float moveForward;
    private float moveBackward;
    private float moveLeft;
    private float moveRight;
    private float direction;
    private float crawlAction;

    private float distanceToTarget;

    [SerializeField] private bool isCrawling = false;
    private bool isInCrawlTrigger = false;
    private bool isInWall = false;


    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float rotationSpeed = 1f;

    private void Start()
    {
        agentTransform = transform;
    }

    private void Update()
    {
    
        distanceToTarget = Vector3.Distance(agentTransform.localPosition, targetTransform.localPosition);
        // Reward that increases as the agent gets closer to the target
        if(inTrain)
        {
            AddReward(-distanceToTarget / 10);
        }
        

        MoveAgent();
        RotateAgent();

        if(Input.GetKey(KeyCode.C))
        {
            isCrawling = true;
            crawlAction = 1;
        }
        else
        {
            isCrawling = false;
            crawlAction = 0;
        }

        if(crawlAction == 1)
        {
            isCrawling = true;
        }
        else if(crawlAction == 0)
        {
            isCrawling = false;
        }

        CrawlPosition();

        if(isInCrawlTrigger)
        {
            if(!isCrawling)
            {
                AddReward(-5f);
            }
            else
            {
                AddReward(1f);
            }
        }
        else
        {
            if(isCrawling)
            {
                AddReward(-5f);
            }
            else
            {
                AddReward(1f);
            }
        }
        
    }

    private void MoveAgent()
    {
        if(heuristic)
        {
            moveForward = Input.GetAxis("Vertical");
        }

        agentTransform.localPosition += agentTransform.forward * Utils.ReLU(moveForward) * moveSpeed * Time.deltaTime;
        //agentTransform.gameObject.GetComponent<Rigidbody>().AddForce(agentTransform.forward * Utils.ReLU(moveForward) * moveSpeed * 0.1f);
    }

    private void RotateAgent()
    {
        if(heuristic)
        {
            direction = Input.GetAxis("Horizontal");
        }

        agentTransform.Rotate(0, direction * rotationSpeed * Time.deltaTime, 0);
    }

    private void CrawlPosition()
    {
        if(isCrawling)
        {
            agentTransform.position = new Vector3(agentTransform.position.x, -0.25f, agentTransform.position.z);
            agentTransform.localScale = new Vector3(agentTransform.localScale.x, 0.5f, agentTransform.localScale.z);
            moveSpeed = 3;
        }
        else if(!isCrawling && !isInWall)
        {
            agentTransform.position = new Vector3(agentTransform.position.x, 0, agentTransform.position.z);
            agentTransform.localScale = new Vector3(agentTransform.localScale.x, 1, agentTransform.localScale.z);
            moveSpeed = 5;
        }
        
    }

    public override void OnEpisodeBegin()
    {
        if(inTrain)
        {
            SetReward(0);
        }
        
        float valueX = Random.Range(-15f, -9f);
        if (valueX > -12f)
        {
            valueX += 24;
        }
        float valueZ = Random.Range(-15f, -9f);
        if (valueZ > -12f)
        {
            valueZ += 24;
        }
        transform.localPosition = new Vector3(0.0f, transform.position.y, 0.0f);
        targetTransform.localPosition = new Vector3(valueX, targetTransform.position.y, valueZ);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(agentTransform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
        sensor.AddObservation(distanceToTarget);
        sensor.AddObservation(isInCrawlTrigger);
        sensor.AddObservation(isCrawling);
        sensor.AddObservation(moveSpeed);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        moveForward = actions.ContinuousActions[0];
        //moveLeft = actions.ContinuousActions[1];
        //moveRight = actions.ContinuousActions[2];
        direction = actions.ContinuousActions[1];
        crawlAction = actions.DiscreteActions[0];
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!inTrain)
        {
            return;
        }
        else if(other.CompareTag("target"))
        {
            AddReward(1000);
            Debug.Log("Target reached!");
            EndEpisode();
        }
        else if(other.CompareTag("crawl"))
        {
            isInCrawlTrigger = true;
        }
        else if(other.CompareTag("wall"))
        {
            EndEpisode();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("crawl"))
        {
            isInCrawlTrigger = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!isCrawling && collision.gameObject.tag == "crawl_wall")
        {
            moveSpeed = 0;
            isInWall = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "crawl_wall")
        {
            moveSpeed = 7;
            isInWall = false;
        }
    }
}
