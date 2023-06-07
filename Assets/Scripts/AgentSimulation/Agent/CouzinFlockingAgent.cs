using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CouzinFlockingAgent : Agent
{
    #region Serialized Fields



    [Header("Field of view size")]
    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("This is the size of the radius.")]
    private float attractionZoneSize = 0.3f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("This is the size of the radius.")]
    private float alignmentZoneSize = 0.3f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    [Tooltip("This is the size of the radius.")]
    private float repulsionZoneSize = 0.3f;

   /* [Header("Feeler parameters")]
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
    private float cohesionIntensity = 1.0f;
    [SerializeField]
    [Range(0.0f, 20.0f)]
    private float alignmentIntensity = 1.0f;
    [SerializeField]
    [Range(0.0f, 20.0f)]
    private float separationIntensity = 1.0f;
    //[SerializeField]
    //[Range(0.0f, 20.0f)]
    //private float avoidingObstaclesIntensity = 1.0f;


    #endregion

    #region Private fields


    private List<GameObject> detectedAgentsInAttractionZone;
    private List<GameObject> detectedAgentsInAlignmentZone;
    private List<GameObject> detectedAgentsInRepulsionZone;


    #endregion

    #region MonoBehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {
        agentManager = FindObjectOfType<AgentManager>();
        mapSizeX = agentManager.GetMapSizeX();
        mapSizeZ = agentManager.GetMapSizeZ();

        parameterManager = FindObjectOfType<ParameterManager>();
        forceField = FindObjectOfType<PatternCreator>();

        detectedAgents = new List<GameObject>();
        detectedAgentsInAttractionZone = new List<GameObject>();
        detectedAgentsInAlignmentZone = new List<GameObject>();
        detectedAgentsInRepulsionZone = new List<GameObject>();

    /*feeler = new GameObject();
    feeler.AddComponent<Feeler>();
    feeler.transform.parent = this.transform;
    feeler.transform.localPosition = Vector3.forward * feelerDistance;

    feelerCollider = feeler.AddComponent<SphereCollider>();
    feelerCollider.isTrigger = true;*/

        savedPosition = this.transform.position;
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
        Cohesion();
        Separation();
        Alignment();
        //if(feelerEnable) AvoidingObstacles();
        EnvironmentalForce();

    }

    private void LateUpdate()
    {
        updateAgent();
        //UpdateFeeler();
    }
    #endregion



    private void Cohesion()
    {
        int count = 0;
        Vector3 totalForce = Vector3.zero;
        foreach (GameObject o in detectedAgentsInAttractionZone)
        {
            count += 1;
            //g += NearestPositionInInfiniteArea(o.transform.position);

            //Same than (cj-ci) / |(cj-ci)|
            Vector3 force = o.transform.position - transform.position;
            force.Normalize();

            totalForce += force;
        }
        if (count > 0)
        {
            totalForce.y = 0.0f; //To stay in 2D


            //g /= count;
            //Vector3 force = g - transform.position;
            totalForce *= this.cohesionIntensity;
            addForce(totalForce);
        }
    }


    private void Separation()
    {
        int count = 0;
        Vector3 totalForce = Vector3.zero;

        foreach (GameObject o in detectedAgentsInRepulsionZone)
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
            //totalForce /= count;
            totalForce *= separationIntensity;
            addForce(totalForce);
        }
    }

    private void Alignment()
    {
        int count = 0;
        Vector3 vm = Vector3.zero;

        foreach (GameObject o in detectedAgentsInAlignmentZone)
        {
            Agent temp = o.GetComponent<Agent>();
            if (temp != null)
            {
                count += 1;
                vm += temp.GetSpeed().normalized;
            }

        }

        if (count > 0)
        {
            vm.y = 0.0f; //To stay in 2D
            //vm /= count;
            vm *= alignmentIntensity;

            addForce(vm);
        }
    }


    protected override void getAgentsInFieldOfView()
    {
        List<GameObject> agents = agentManager.GetAgents();
        detectedAgents = new List<GameObject>();
        detectedAgentsInAttractionZone = new List<GameObject>();
        detectedAgentsInAlignmentZone = new List<GameObject>();
        detectedAgentsInRepulsionZone = new List<GameObject>();



        float zone1 = repulsionZoneSize;
        float zone2 = repulsionZoneSize + alignmentZoneSize;
        float zone3 = repulsionZoneSize + alignmentZoneSize + attractionZoneSize;

        foreach (GameObject g in agents)
        {
            if (GameObject.ReferenceEquals(g, this.gameObject)) continue;
            float distance = Vector3.Distance(g.transform.position, this.transform.position);
            if (distance <= zone3)
            {
                Vector3 dir = g.transform.position - this.transform.position;
                float angle = Vector3.Angle(this.speed, dir);

                if (angle <= 180 - (blindSpotSize / 2))
                {
                    detectedAgents.Add(g);
                    if (distance > zone1)
                    {
                        if (distance > zone2)
                        {
                            detectedAgentsInAttractionZone.Add(g);
                        }
                        else
                        {
                            detectedAgentsInAlignmentZone.Add(g);
                        }
                    }
                    else
                    {
                        detectedAgentsInRepulsionZone.Add(g);
                    }
                }
            }
        }
    }



    private void UpdateParameters()
    {
        blindSpotSize = this.parameterManager.GetBlindSpotSize();
        attractionZoneSize = this.parameterManager.GetAttractionZoneSize();
        alignmentZoneSize = this.parameterManager.GetAlignmentZoneSize();
        repulsionZoneSize = this.parameterManager.GetRepulsionZoneSize();
        cohesionIntensity = this.parameterManager.GetCohesionIntensity();
        alignmentIntensity = this.parameterManager.GetAlignmentIntensity();
        separationIntensity = this.parameterManager.GetSeparationIntensity();
        //avoidingObstaclesIntensity = this.parameterManager.GetAvoidingObstaclesIntensity();
        moveForwardIntensity = this.parameterManager.GetMoveForwardIntensity();
        randomMovementIntensity = this.parameterManager.GetRandomMovementIntensity();
        frictionIntensity = this.parameterManager.GetFrictionIntensity();
        maxSpeed = this.parameterManager.GetMaxSpeed();
        //feelerEnable = this.parameterManager.IsFeelerEnable();
        //feelerDistance = this.parameterManager.GetFeelerDistance();
        //feelerSize = this.parameterManager.GetFeelerSize();
    }


    #region Get Methods


    public override float GetFieldOfViewSize()
    {
        return (repulsionZoneSize + alignmentZoneSize + attractionZoneSize);
    }

    #endregion
}
