using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using ALJV;

public class AgentBehavoirFinal : Agent
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
    private bool isTrapped;

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
            AddReward(-distanceToTarget / 25);
        }        

        MoveAgent();
        RotateAgent();
        
    }

    private void MoveAgent()
    {
        if(heuristic)
        {
            moveForward = Input.GetAxis("Vertical");
        }

        agentTransform.localPosition += agentTransform.forward * Utils.ReLU(moveForward) * moveSpeed * Time.deltaTime;
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
        
        transform.localPosition = new Vector3(2.5f, transform.position.y, 0.0f);
        targetTransform.localPosition = new Vector3(2.5f, targetTransform.position.y, 150.0f);
        transform.Rotate(0, 0, 0);
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
            Debug.Log("Target reached!");
            EndEpisode();
        }
        else if(other.CompareTag("wall"))
        {
            AddReward(-1000);
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "trap_wall" && !isTrapped)
        {
            isTrapped = true;
            AddReward(-200);
            Debug.Log("Trap hit!");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isTrapped = false;
    }
}
