using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using ALJV;

public class AgentBehavoir : Agent
{
    [SerializeField] private bool heuristic;
    [SerializeField] private bool inTrain;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Rigidbody agentRigidbody;
    [SerializeField] private Vector3 jumpVectorForce = new Vector3(0f, 3f, 1f);
    [SerializeField] private Collider groundCollider;

    private Transform agentTransform;
    private float moveForward;
    private float moveBackward;
    private float moveLeft;
    private float moveRight;
    private float direction;
    private bool agentOnGround;

    private int jump;

    private float distanceToTarget;

    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float rotationSpeed = 1f;

    private void Start()
    {
        agentTransform = transform;
        agentRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        /*if(inTrain && distanceToTarget > 23f)
        {
            Debug.Log("Too far from target");
            SetReward(-1);
            EndEpisode();

        }*/

        distanceToTarget = Vector3.Distance(agentTransform.localPosition, targetTransform.localPosition);
        // Reward that increases as the agent gets closer to the target
        if(inTrain)
        {
            AddReward(-distanceToTarget / 25);
        }
        

        MoveAgent();
        RotateAgent();
        JumpAgent();
        AgentGroundCheck();
    }

    private void MoveAgent()
    {
        if(heuristic)
        {
            moveForward = Input.GetAxis("Vertical");
            moveBackward = -Input.GetAxis("Vertical");
        }

        if(agentOnGround)
        {
            agentTransform.localPosition += agentTransform.forward * Utils.ReLU(moveForward) * moveSpeed * Time.deltaTime;
        }
        
        //agentTransform.localPosition -= agentTransform.right * moveLeft * moveSpeed * Time.deltaTime;
        //agentTransform.localPosition += agentTransform.right * moveRight * moveSpeed * Time.deltaTime;

    }

    private void RotateAgent()
    {
        if(heuristic)
        {
            direction = Input.GetAxis("Horizontal");
        }

        agentTransform.Rotate(0, direction * rotationSpeed * Time.deltaTime, 0);
    }

    private void AgentGroundCheck()
    {
        agentOnGround = groundCollider.gameObject.GetComponent<GroundCheck>().IsGrounded();
    }

    private void JumpAgent()
    {
        if (heuristic)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                jump = 1;
                JumpForce();
            }
        }
        else if(inTrain)
        {
            if(jump == 1)
            {
                JumpForce();
            }
        }
    }

    private void JumpForce()
    {
        if(agentOnGround)
        {
            agentRigidbody.AddRelativeForce(jumpVectorForce * jump, ForceMode.Impulse);  
        }
        jump = 0;
    }

    public override void OnEpisodeBegin()
    {
        if(inTrain)
        {
            SetReward(0);
        }
        
        transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

        int randomSide1 = Random.Range(0, 2);
        int randomSide2 = Random.Range(0, 2);
        if (randomSide1 == 0)
        {
            if (randomSide2 == 0)
            {
                targetTransform.localPosition = new Vector3(Random.Range(-14f, 14f), targetTransform.position.y, Random.Range(9f, 14f));
            }
            else
            {
                targetTransform.localPosition = new Vector3(Random.Range(-14f, 14f), targetTransform.position.y, Random.Range(-14f, -9f));
            }
        }
        else
        {
            if (randomSide2 == 0)
            {
                targetTransform.localPosition = new Vector3(Random.Range(-14f, -9f), targetTransform.position.y, Random.Range(-14f, 14f));
            }
            else
            {
                targetTransform.localPosition = new Vector3(Random.Range(9f, 14f), targetTransform.position.y, Random.Range(-14f, 14f));
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(agentTransform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
        sensor.AddObservation(distanceToTarget);
        sensor.AddObservation(agentOnGround);
        sensor.AddObservation(jump);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        moveForward = actions.ContinuousActions[0];
        //moveLeft = actions.ContinuousActions[1];
        //moveRight = actions.ContinuousActions[2];
        direction = actions.ContinuousActions[1];

        jump = actions.DiscreteActions[0];
        
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
            Debug.Log("Reached target");
            EndEpisode();
        }
        else if(other.CompareTag("wall"))
        {
            EndEpisode();
        }
    }
}
