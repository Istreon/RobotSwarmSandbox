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
        forceField = FindObjectOfType<PatternCreator>();

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


    private void Separation()
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

    private void Alignment()
    {
        int count = 0;
        Vector3 vm = Vector3.zero;

        foreach (GameObject o in detectedAgents)
        {
            ReynoldsFlockingAgent temp = o.GetComponent<ReynoldsFlockingAgent>();
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

    /*private void AvoidingObstacles()
    {
        foreach (GameObject o in detectedObstacles)
        {
            //Calcul de "(r+d)" {1}
            float r = fieldOfViewSize;
            float d = Vector3.Distance(this.transform.position, o.transform.position);
            float rd = r + d;

            //Calul de " CP' / ||CP'|| " {2}
            Vector3 cp = feeler.transform.position - o.transform.position;
            cp.Normalize();

            //Calcul final "{1}*{2} - C" {3}
            cp *= rd;
            Vector3 pFinal = cp - o.transform.position;
            pFinal.y = 0.0f;


            //Seek({3})
            Vector3 force = pFinal - this.transform.position;
            force.Normalize();
            force *= avoidingObstaclesIntensity;

            addForce(force);


        }
    }*/
    /*
    private void UpdateFeeler()
    {
        if (feelerEnable)
        {
            feeler.transform.position = this.transform.position + (speed.normalized * feelerDistance);
            feelerCollider.radius = feelerSize;
        } else
        {
            feeler.SetActive(false);
        }

    }*/



    /*private void getObstacles()
    {
        detectedObstacles = feeler.GetComponent<Feeler>().getObstacles();
        if (detectedObstacles == null)
        {
            detectedObstacles = new List<GameObject>();
        }
    }*/





  


    private void UpdateParameters()
    {
        cohesionIntensity = this.parameterManager.GetCohesionIntensity();
        alignmentIntensity = this.parameterManager.GetAlignmentIntensity();
        separationIntensity = this.parameterManager.GetSeparationIntensity();
       // avoidingObstaclesIntensity = this.parameterManager.GetAvoidingObstaclesIntensity();
        fieldOfViewSize = this.parameterManager.GetFieldOfViewSize();
        moveForwardIntensity = this.parameterManager.GetMoveForwardIntensity();
        randomMovementIntensity = this.parameterManager.GetRandomMovementIntensity();
        frictionIntensity = this.parameterManager.GetFrictionIntensity();
        maxSpeed = this.parameterManager.GetMaxSpeed();
        /*feelerEnable = this.parameterManager.IsFeelerEnable();
        feelerDistance = this.parameterManager.GetFeelerDistance();
        feelerSize = this.parameterManager.GetFeelerSize();*/
    }


   


}
