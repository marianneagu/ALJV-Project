using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AgentBehavoir : Agent
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

    private float distanceToTarget;

    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float rotationSpeed = 1f;

    private void Start()
    {
        agentTransform = transform;
    }

    private void Update()
    {
        if(inTrain && distanceToTarget > 10f)
        {
            Debug.Log("Too far from target");
            SetReward(-1);
            EndEpisode();
        }

        distanceToTarget = Vector3.Distance(agentTransform.localPosition, targetTransform.localPosition);
        // Reward that increases as the agent gets closer to the target
        if(inTrain)
        {
            AddReward(-distanceToTarget / 10);
        }
        

        MoveAgent();
        RotateAgent();
        
    }

    private void MoveAgent()
    {
        if(heuristic)
        {
            moveForward = Input.GetAxis("Vertical");
            moveBackward = -Input.GetAxis("Vertical");
            moveLeft = -Input.GetAxis("Horizontal");
            moveRight = Input.GetAxis("Horizontal");
        }
        else
        {
            agentTransform.localPosition += agentTransform.forward * moveForward * moveSpeed * Time.deltaTime;
            //agentTransform.localPosition -= agentTransform.right * moveLeft * moveSpeed * Time.deltaTime;
            //agentTransform.localPosition += agentTransform.right * moveRight * moveSpeed * Time.deltaTime;
        }
    }

    private void RotateAgent()
    {
        if(heuristic)
        {
            direction = Input.GetAxis("Horizontal");
        }

        agentTransform.Rotate(0, direction * rotationSpeed * Time.deltaTime, 0);
    }

    public override void OnEpisodeBegin()
    {
        if(inTrain)
        {
            SetReward(0);
        }
        
        transform.localPosition = new Vector3(0.0f, transform.position.y, 0.0f);
        targetTransform.localPosition = new Vector3(Random.Range(-8f, 8f), targetTransform.position.y, Random.Range(-8f, 8f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(agentTransform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
        sensor.AddObservation(distanceToTarget);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        moveForward = actions.ContinuousActions[0];
        //moveLeft = actions.ContinuousActions[1];
        //moveRight = actions.ContinuousActions[2];
        direction = actions.ContinuousActions[1];
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
            EndEpisode();
        }
    }
}
