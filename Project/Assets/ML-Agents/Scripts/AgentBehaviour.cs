using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AgentBehavoir : Agent
{
    [SerializeField] private Transform targetTransform;
    private Transform agentTransform;
    private float moveForward;
    private float moveBackward;
    private float moveLeft;
    private float moveRight;
    private float direction;

    private float distanceToTarget;


    [SerializeField] private float moveSpeed = 1f;

    private void Start()
    {
        agentTransform = transform;
    }

    private void Update()
    {
        if(distanceToTarget > 10f)
        {
            Debug.Log("Too far from target");
            SetReward(-1);
            EndEpisode();
        }

        distanceToTarget = Vector3.Distance(agentTransform.localPosition, targetTransform.localPosition);
        // reward that increases as the agent gets closer to the target
        AddReward(-distanceToTarget / 10);

        //Debug.Log("Cumulative Reward: " + GetCumulativeReward());

        agentTransform.localPosition += agentTransform.forward * moveForward * moveSpeed * Time.deltaTime;
        agentTransform.localPosition -= agentTransform.forward * moveBackward * moveSpeed * Time.deltaTime;
        agentTransform.localPosition -= agentTransform.right * moveLeft * moveSpeed * Time.deltaTime;
        agentTransform.localPosition += agentTransform.right * moveRight * moveSpeed * Time.deltaTime;
    }

    public override void OnEpisodeBegin()
    {
        SetReward(0);
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
        moveBackward = actions.ContinuousActions[1];
        moveLeft = actions.ContinuousActions[2];
        moveRight = actions.ContinuousActions[3];
        direction = actions.ContinuousActions[4];
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Target"))
        {
            AddReward(100);
            EndEpisode();
        }
    }
}
