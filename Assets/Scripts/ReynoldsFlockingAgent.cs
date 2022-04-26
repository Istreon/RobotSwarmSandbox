using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReynoldsFlockingAgent : MonoBehaviour
{
    #region Serialized Fields

   

    [Header("Field of view size")]
    [SerializeField]
    [Range(0.0f, 5.0f)]
    [Tooltip("This is the size of the radius.")]
    private float fieldOfViewSize = 1.0f;


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
    [SerializeField]
    [Range(0.0f, 30.0f)]
    private float moveForwardIntensity = 1.0f;
    [SerializeField]
    [Range(0.0f, 30.0f)]
    private float randomMovementIntensity = 20.0f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    private float frictionIntensity = 0.1f;
    [SerializeField]
    [Range(0.0f, 2.0f)]
    private float maxSpeed = 1.0f;


    #endregion

    #region Private fields


    private Vector3 acceleration=Vector3.zero;
    private Vector3 speed=Vector3.zero;

    //private float mass = 1.0f;

    private List<GameObject> detectedAgents;
    private List<GameObject> detectedObstacles;

    private float mapSizeX = 5.0f;
    private float mapSizeZ = 5.0f;

    private ParameterManager parameterManager;
    private AgentManager agentManager;


    private Vector3 savedPosition;
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
        //InitializeAgent(true);

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
        Cohesion();
        Separation();
        Alignment();
    }

    private void LateUpdate()
    {
        updateAgent();
    }
    #endregion

    private void updateAgent()
    {
        //Update agent position based on speed and time passed since last change
        this.transform.position += this.speed * Time.deltaTime;
        this.transform.position = new Vector3(this.transform.position.x,0.1f, this.transform.position.z);
        //Stay in the finite area if it go further than map size limit
        StayInFiniteArea();

        //Update agent speed based on acceleration and time passed since last change
        this.speed += this.acceleration * Time.deltaTime;

        //Reset acceleration
        this.acceleration = Vector3.zero;

        //Limit speed vector based on agent max speed
        float temp = this.speed.sqrMagnitude; //faster than Vector3.Magnitude(this.speed);
        if (temp > (maxSpeed * maxSpeed))
        {
            this.speed.Normalize();
            this.speed *= this.maxSpeed;
        }
    }


    private void Cohesion()
    {
        int count = 0;
        Vector3 g = Vector3.zero;
        foreach(GameObject o in detectedAgents)
        {
            count += 1;
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


    private void RandomMovement()
    {
        float alea = 0.1f;
        if (Random.value < alea)
        {
            float x = Random.value - 0.5f;
            float z = Random.value - 0.5f;
            Vector3 force=new Vector3(x, 0.0f, z);
            force.Normalize();

            //Modification de la puissance de cette force
            force*=randomMovementIntensity;

            addForce(force);
        }
    }

    private void MoveForward()
    {
        Vector3 force = speed.normalized;

        //Modification de la puissance de cette force
        force *= moveForwardIntensity;

        addForce(force);   
    }

    private void Friction()
    {
        float k = frictionIntensity;
        Vector3 force = this.speed;
        force *= -k;
        addForce(force);
    }



    private void getAgentsInFieldOfView()
    {

        List<GameObject> agents=agentManager.GetAgents();
        detectedAgents = new List<GameObject>();

        foreach(GameObject g in agents)
        {
            if (GameObject.ReferenceEquals(g, this.gameObject)) continue;
            if (Vector3.Distance(g.transform.position,this.transform.position)<=fieldOfViewSize)
            {
                detectedAgents.Add(g);
            }
        }
    }



    private void addForce(Vector3 force)
    {
        this.acceleration += force; //  *(1.0f/this.mass);
    }


    private void StayInFiniteArea()
    {
        float x=0.0f;
        float z=0.0f;
        Vector3 temp = this.transform.position;
        if (this.transform.position.x > mapSizeX)
        {
            temp.x = mapSizeX;
            x = -1;
            speed.x = 0.0f;
        }
        if (this.transform.position.x < 0.0f)
        {
            temp.x = 0.0f;
            x = 1;
            speed.x = 0.0f;
        }

        if (this.transform.position.z > mapSizeZ)
        {
            temp.z = mapSizeZ;
            z = -1;
            speed.z = 0.0f;
        }
        if (this.transform.position.z < 0.0f)
        {
            temp.z = 0.0f;
            z = 1;
            speed.z = 0.0f;
        }
        Vector3 rebond = new Vector3(x, 0.0f, z);
        rebond *= 50;
        addForce(rebond);
        this.transform.position = temp;
    }



    private void UpdateParameters()
    {
        cohesionIntensity = this.parameterManager.GetCohesionIntensity();
        alignmentIntensity = this.parameterManager.GetAlignmentIntensity();
        separationIntensity = this.parameterManager.GetSeparationIntensity();
        fieldOfViewSize = this.parameterManager.GetFieldOfViewSize();
        moveForwardIntensity = this.parameterManager.GetMoveForwardIntensity();
        randomMovementIntensity = this.parameterManager.GetRandomMovementIntensity();
        frictionIntensity = this.parameterManager.GetFrictionIntensity();
        maxSpeed = this.parameterManager.GetMaxSpeed();
    }


    #region Get Methods
    public Vector3 GetSpeed()
    {
        return speed;
    }

    public float GetFieldOfViewSize()
    {
        return fieldOfViewSize;
    }


    public List<GameObject> GetNeighbors()
    {
        if (detectedAgents == null) return new List<GameObject>();
        else return detectedAgents;
    }

    public float GetMaxSpeed()
    {
        return maxSpeed;
    }

    public Vector3 GetSavedPosition()
    {
        return savedPosition;
    }

    public void SavePosition()
    {
        savedPosition = this.transform.position;
    }
    #endregion


}
