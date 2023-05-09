using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReynoldsFlockingAgent : Agent
{
    #region Serialized Fields

  

    /*[Header("Feeler parameters")]
    [SerializeField]
    private bool feelerEnable = true;
    [SerializeField]
    [Range(0.0f, 2.0f)]
    [Tooltip("This is the distance of the feeler from the agent.")]
    private float feelerDistance = 0.5f;
    [Range(0.0f, 0.5f)]
    [Tooltip("This is the size of the feeler radius.")]
    private float feelerSize = 0.1f;*/

    [Header("Intensity parameters")]
    [SerializeField]
    [Range(0.0f, 20.0f)]
    private float cohesionIntensity=1.0f;
    [SerializeField]
    [Range(0.0f, 20.0f)]
    private float alignmentIntensity = 1.0f;
    [SerializeField]
    [Range(0.0f, 20.0f)]
    private float separationIntensity = 1.0f;
    /*[SerializeField]
    [Range(0.0f, 20.0f)]
    private float avoidingObstaclesIntensity = 1.0f;*/



    #endregion



    #region MonoBehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {
        agentManager = FindObjectOfType<AgentManager>();
        mapSizeX = agentManager.GetMapSizeX();
        mapSizeZ = agentManager.GetMapSizeZ();

        parameterManager = FindObjectOfType<ParameterManager>();

        detectedAgents = new List<GameObject>();

        savedPosition = this.transform.position;


        /*feeler = new GameObject();
        feeler.AddComponent<Feeler>();
        feeler.transform.parent = this.transform;
        feeler.transform.localPosition = Vector3.forward * feelerDistance;

        feelerCollider = feeler.AddComponent<SphereCollider>();
        feelerCollider.isTrigger = true;*/


    }

    // Update is called once per frame
    void Update()
    {
        UpdateParameters();
        getAgentsInFieldOfView();
        //if (feelerEnable) getObstacles();
        RandomMovement();
        MoveForward();
        Friction();
        AvoidCollisionWithNeighbors();
        Cohesion();
        Separation();
        Alignment();
        //if(feelerEnable) AvoidingObstacles();

    }

    private void LateUpdate()
    {
        updateAgent();
        //UpdateFeeler();
    }
    #endregion


    /// <summary>
    /// Add to the current acceleration a cohesion force based on current neighbours. This force brings this agent closer to its detected neighbours.
    /// </summary>
    private void Cohesion()
    {
        int count = 0;
        Vector3 g = Vector3.zero;
        foreach(GameObject o in detectedAgents)
        {
            count += 1;
            //g += NearestPositionInInfiniteArea(o.transform.position);
            g += o.transform.position;
        }
        if(count>0)
        {
            g.y = 0.0f; //To stay in 2D


            g /= count;
            Vector3 force = g - transform.position;
            force *= this.cohesionIntensity;
            addForce(force);
        }
    }

    /// <summary>
    /// Add to the current acceleration a separation force based on current neighbours. This force moves this agent away to its detected neighbours.
    /// </summary>
    private void Separation() // Bonne version
    {
        int count = 0;
        Vector3 totalForce = Vector3.zero;

        foreach (GameObject o in detectedAgents)
        {
            count += 1;
            //Vector3 force = this.transform.position - NearestPositionInInfiniteArea(o.transform.position);
            Vector3 force = this.transform.position - o.transform.position;
            force.Normalize();

            totalForce += force;
        }

        if (count > 0)
        {
            totalForce.y = 0.0f; //To stay in 2D
            totalForce /= count;
            totalForce *= separationIntensity;
            addForce(totalForce);
        }
    }

   

    /// <summary>
    /// Add to the current acceleration a alignment force based on current neighbours. This force align this agent to match its detected neighbours speed (direction and intensity).
    /// </summary>
    private void Alignment()
    {
        int count = 0;
        Vector3 vm = Vector3.zero;

        foreach (GameObject o in detectedAgents)
        {
            Agent temp = o.GetComponent<Agent>();
            if (temp!=null)
            {
                count += 1;
                vm += temp.GetSpeed();
            }
        }

        if (count > 0)
        {
            vm.y = 0.0f; //To stay in 2D
            vm /= count;
            vm *= alignmentIntensity;

            addForce(vm);
        }
    }


    /// <summary>
    /// Update all parameters from the <see cref="ParameterManager"/> instance in the scene.
    /// </summary>
    private void UpdateParameters()
    {
        cohesionIntensity = this.parameterManager.GetCohesionIntensity();
        alignmentIntensity = this.parameterManager.GetAlignmentIntensity();
        separationIntensity = this.parameterManager.GetSeparationIntensity();
       // avoidingObstaclesIntensity = this.parameterManager.GetAvoidingObstaclesIntensity();
        fieldOfViewSize = this.parameterManager.GetFieldOfViewSize();
        blindSpotSize = this.parameterManager.GetBlindSpotSize();
        moveForwardIntensity = this.parameterManager.GetMoveForwardIntensity();
        randomMovementIntensity = this.parameterManager.GetRandomMovementIntensity();
        frictionIntensity = this.parameterManager.GetFrictionIntensity();
        maxSpeed = this.parameterManager.GetMaxSpeed();
        /*feelerEnable = this.parameterManager.IsFeelerEnable();
        feelerDistance = this.parameterManager.GetFeelerDistance();
        feelerSize = this.parameterManager.GetFeelerSize();*/
    }
}
