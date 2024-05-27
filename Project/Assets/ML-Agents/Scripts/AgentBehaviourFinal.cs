using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using ALJV;

public class AgentBehavoirFinal : Agent
{
    [Header("Training stage (choose only one)")]
    
    [SerializeField] private bool justMoveStage;
    [SerializeField] private bool justCrawlStage;
    [SerializeField] private bool justJumpStage;
    [SerializeField] private bool justTrapWallsStage;
    [SerializeField] private bool allStage;

    [Header("General settings")]
    [SerializeField] private bool heuristic;
    [SerializeField] private bool inTrain;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Rigidbody agentRigidbody;

    private Transform agentTransform;
    private float moveForward;
    private float direction;
    private float distanceToTarget;
    private float lastDistanceToTarget;

    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float rotationSpeed = 1f;

    // Crawl
    [SerializeField] private bool isCrawling = false;
    private float crawlAction;
    private bool isInCrawlTrigger = false;
    private bool isInWall = false;

    // JUMP
    [SerializeField] private Collider groundCheckCollider;
    [SerializeField] private Vector3 jumpVectorForce = new Vector3(0f, 3f, 1f);
    private bool agentOnGround;
    private int jump;

    private void Start()
    {
        agentTransform = transform;
        agentRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        distanceToTarget = Vector3.Distance(agentTransform.localPosition, targetTransform.localPosition);

        // Reward that increases as the agent gets closer to the target
        if(inTrain)
        {
            if(lastDistanceToTarget > distanceToTarget  )
            {
                AddReward(0.1f);
            }
            else if(lastDistanceToTarget < distanceToTarget)
            {
                AddReward(-0.1f);
            }
            lastDistanceToTarget = distanceToTarget;
        }        

        MoveAgent();
        RotateAgent();
        CrawlAgent();
        JumpAgent();
        CrawlAgent();

        AgentGroundCheck();
    }

    #region MOVEMENT AND ROTATION
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
    #endregion

    #region CRAWL
    private void CrawlAgent()
    {
        if (inTrain)
        {
            AddReward(-distanceToTarget / 10);
        }

        if (Input.GetKey(KeyCode.C))
        {
            isCrawling = true;
            crawlAction = 1;
        }
        else
        {
            isCrawling = false;
            crawlAction = 0;
        }

        if (crawlAction == 1)
        {
            isCrawling = true;
        }
        else if (crawlAction == 0)
        {
            isCrawling = false;
        }


        // Rewards
        if (isInCrawlTrigger)
        {
            if (!isCrawling)
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
            if (isCrawling)
            {
                AddReward(-5f);
            }
            else
            {
                AddReward(1f);
            }
        }

        CrawlPosition();
    }
    private void CrawlPosition()
    {
        if (isCrawling)
        {
            //agentTransform.position = new Vector3(agentTransform.position.x, agentTransform.position.y - 0.25f, agentTransform.position.z);
            agentTransform.localScale = new Vector3(agentTransform.localScale.x, 0.5f, agentTransform.localScale.z);
            moveSpeed = 3;
        }
        else if (!isCrawling && !isInWall)
        {
            //agentTransform.position = new Vector3(agentTransform.position.x, agentTransform.position.y + 0.25f, agentTransform.position.z);
            agentTransform.localScale = new Vector3(agentTransform.localScale.x, 1, agentTransform.localScale.z);
            moveSpeed = 5;
        }
    }
    #endregion

    #region JUMP
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
        else
        {
            if (jump == 1)
            {
                JumpForce();
            }
        }
    }
    private void JumpForce()
    {
        if (agentOnGround)
        {
            agentRigidbody.AddRelativeForce(jumpVectorForce * jump, ForceMode.Impulse);
        }
        jump = 0;
    }
    private void AgentGroundCheck()
    {
        agentOnGround = groundCheckCollider.gameObject.GetComponent<GroundCheck>().IsGrounded();
    }
    #endregion

    #region ML AGENTS FUNCTIONS
    public override void OnEpisodeBegin()
    {
        if(inTrain)
        {
            SetReward(0);
        }

        if(justMoveStage)
        {
            transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            targetTransform.localPosition = new Vector3(Random.Range(-14f, 14f), targetTransform.position.y, Random.Range(-14f, 14f));
        }
        else if(justCrawlStage)
        {
            transform.localPosition = new Vector3(0.0f, 0.0f, -12.0f);
            targetTransform.localPosition = new Vector3(Random.Range(-14f, 14f), targetTransform.position.y, Random.Range(6f, 14f));
        }
        else if(justJumpStage)
        {
            transform.localPosition = new Vector3(0.0f, 0.0f, -12.0f);
            targetTransform.localPosition = new Vector3(Random.Range(-14f, 14f), targetTransform.position.y, Random.Range(6f, 14f));
        }
        else if(justTrapWallsStage)
        {
            Debug.LogError("This stage is not implemented yet!");
        }
        else if(allStage)
        {
            transform.localPosition = new Vector3(2.5f, 0.0f, 0.0f);
            targetTransform.localPosition = new Vector3(2.5f, targetTransform.position.y, 150.0f);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // MOVEMENT
        sensor.AddObservation(agentTransform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
        sensor.AddObservation(distanceToTarget);
        sensor.AddObservation(moveSpeed);
        // CRAWL
        sensor.AddObservation(isCrawling);
        sensor.AddObservation(isInCrawlTrigger);
        // JUMP
        sensor.AddObservation(jump);
        sensor.AddObservation(agentOnGround);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // CONTINUOUS ACTIONS
        moveForward = actions.ContinuousActions[0];
        direction = actions.ContinuousActions[1];
        // DISCRETE ACTIONS
        crawlAction = actions.DiscreteActions[0];
        jump = actions.DiscreteActions[1];
    }
    #endregion


    #region COLLISIONS
    private void OnTriggerEnter(Collider other)
    {
        if (!inTrain)
        {
            return;
        }
        // TARGET REACHED
        else if (other.CompareTag("target"))
        {
            AddReward(2000);
            Debug.Log("Target reached!");
            EndEpisode();
        }
        // CRAWL
        else if (other.CompareTag("crawl"))
        {
            isInCrawlTrigger = true;
        }
        // EXTERNAL WALLS
        else if (other.CompareTag("wall"))
        {
            EndEpisode();
            SetReward(-10000);
        }
        // JUMP
        else if (other.CompareTag("jump_trigger"))
        {
            AddReward(500);
            Debug.Log("Jump trigger reached!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // CRAWL
        if (other.CompareTag("crawl"))
        {
            isInCrawlTrigger = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // CRAWL
        if (!isCrawling && collision.gameObject.CompareTag("crawl_wall"))
        {
            AddReward(-500);
            moveSpeed = 0;
            isInWall = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // CRAWL
        if (collision.gameObject.CompareTag("crawl_wall"))
        {
            AddReward(200);
            moveSpeed = 7;
            isInWall = false;
        }
    }
    #endregion
}
