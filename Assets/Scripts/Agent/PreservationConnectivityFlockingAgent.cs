using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreservationConnectivityFlockingAgent : Agent
{
    #region Serialized Fields

    [Header("Intensity parameters")]
    [SerializeField]
    [Range(0.0f, 20.0f)]
    private float alignmentIntensity = 1.0f;


    [SerializeField]
    [Range(0.01f, 2.0f)]
    private float distanceBetweenAgents = 1.0f;


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

        savedPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateParameters();
        getAgentsInFieldOfView();
        RandomMovement();
        MoveForward();
        Friction();
        PotentialFunction();
        Alignment();
        EnvironmentalForce();

    }

    private void LateUpdate()
    {
        updateAgent();
    }
    #endregion


    private void PotentialFunction()
    {
        int count = 0;

        Vector3 g = Vector3.zero;
        foreach (GameObject o in detectedAgents)
        {
            Agent temp = o.GetComponent<Agent>();
            if (temp != null)
            {
                Vector3 rij = this.transform.position - o.transform.position;
                rij /= distanceBetweenAgents;
                float uX = (-2 * this.transform.position.x* rij.x + 2 * this.transform.position.x * rij.x * (Mathf.Pow(rij.x, 2) + Mathf.Pow(rij.z, 2))) / Mathf.Pow((Mathf.Pow(rij.x, 2) + Mathf.Pow(rij.z, 2)), 2);
                float uZ = (-2 * this.transform.position.z * rij.z + 2 * this.transform.position.x * rij.z * (Mathf.Pow(rij.x, 2) + Mathf.Pow(rij.z, 2))) / Mathf.Pow((Mathf.Pow(rij.x, 2) + Mathf.Pow(rij.z, 2)), 2);
                
        

                g+=new Vector3(uX, 0.0f, uZ);

                count += 1;

            }
        }
        addForce(-g);
    }


    private void Alignment()
    {
        int count = 0;
        Vector3 a = Vector3.zero;

        foreach (GameObject o in detectedAgents)
        {
            Agent temp = o.GetComponent<Agent>();
            if (temp != null)
            {
                count += 1;
                a += (this.GetSpeed() - temp.GetSpeed());

            }

        }

        if (count > 0)
        {
            a.y = 0.0f; //To stay in 2D
            a *= alignmentIntensity;
            addForce(-a);
        }
    }

    private void UpdateParameters()
    {
        alignmentIntensity = this.parameterManager.GetAlignmentIntensity();
        fieldOfViewSize = this.parameterManager.GetFieldOfViewSize();
        blindSpotSize = this.parameterManager.GetBlindSpotSize();
        moveForwardIntensity = this.parameterManager.GetMoveForwardIntensity();
        randomMovementIntensity = this.parameterManager.GetRandomMovementIntensity();
        frictionIntensity = this.parameterManager.GetFrictionIntensity();
        maxSpeed = this.parameterManager.GetMaxSpeed();
        distanceBetweenAgents = this.parameterManager.GetDistanceBetweenAgents();

    }
}
