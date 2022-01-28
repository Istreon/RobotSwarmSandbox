using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReynoldsFlockingAgent : Agent
{
    #region Serialized Fields

   

    [Header("Field of view size")]
    [SerializeField]
    [Range(0.0f, 5.0f)]
    [Tooltip("This is the size of the radius.")]
    private float fieldOfViewSize = 1.0f;

    [Header("Feeler parameters")]
    [SerializeField]
    private bool feelerEnable = true;
    [SerializeField]
    [Range(0.0f, 2.0f)]
    [Tooltip("This is the distance of the feeler from the agent.")]
    private float feelerDistance = 0.5f;
    [Range(0.0f, 0.5f)]
    [Tooltip("This is the size of the feeler radius.")]
    private float feelerSize = 0.1f;

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
    [Range(0.0f, 20.0f)]
    private float avoidingObstaclesIntensity = 1.0f;
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
    //Field of view
    private GameObject fieldOfView;
    private SphereCollider fieldOfViewCollider;

    //Feeler
    private GameObject feeler;
    private SphereCollider feelerCollider;

    private Vector3 acceleration=Vector3.zero;
    private Vector3 speed=Vector3.zero;

    //private float mass = 1.0f;

    private List<GameObject> detectedAgents;
    private List<GameObject> detectedObstacles;

    private float mapSizeX = 5.0f;
    private float mapSizeZ = 5.0f;

    private ParameterManager parameterManager;
    private AgentManager agentManager;

    private PatternCreator forceField;


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
        forceField = FindObjectOfType<PatternCreator>();

        detectedAgents = new List<GameObject>();
        InitializeAgent(true);

        fieldOfView = new GameObject();
        fieldOfView.AddComponent<VisionZone>();
        fieldOfView.transform.parent = this.transform;
        fieldOfView.transform.localPosition = Vector3.zero;


        fieldOfViewCollider = fieldOfView.AddComponent<SphereCollider>();
        fieldOfViewCollider.isTrigger = true;


        feeler = new GameObject();
        feeler.AddComponent<Feeler>();
        feeler.transform.parent = this.transform;
        feeler.transform.localPosition = Vector3.forward * feelerDistance;

        feelerCollider = feeler.AddComponent<SphereCollider>();
        feelerCollider.isTrigger = true;

        savedPosition = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateParameters();
        getAgentsInFieldOfView();
        if (feelerEnable) getObstacles();
        RandomMovement();
        Friction();
        Cohesion();
        Separation();
        Alignment();
        if(feelerEnable) AvoidingObstacles();
        EnvironmentalForce();

    }

    private void LateUpdate()
    {
        updateAgent();
        UpdateFieldOfViewSize();
        UpdateFeeler();
    }
    #endregion

    private void updateAgent()
    {
        //Update agent position based on speed and time passed since last change
        this.transform.position += this.speed * Time.deltaTime;
        this.transform.position = new Vector3(this.transform.position.x,0.1f, this.transform.position.z);
        //Loop position if it go further than map size limit
        //StayInInfiniteArea();
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

    private void AvoidingObstacles()
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

    private void EnvironmentalForce()
    {
        Vector2 temp = new Vector2(transform.position.x, transform.position.z);
        temp = forceField.GetEnvironmentalForce(temp);
        Vector3 force = new Vector3(temp.x, 0.0f, temp.y);
        force *= 100;
        addForce(force);
    }

    private void UpdateFieldOfViewSize()
    {
        fieldOfViewCollider.radius = fieldOfViewSize;
    }

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

    }

    private void getAgentsInFieldOfView()
    {
        detectedAgents = fieldOfView.GetComponent<VisionZone>().getAgentsInsideZone();
        if(detectedAgents==null)
        {
            detectedAgents = new List<GameObject>();
        }
    }

    private void getObstacles()
    {
        detectedObstacles = feeler.GetComponent<Feeler>().getObstacles();
        if (detectedObstacles == null)
        {
            detectedObstacles = new List<GameObject>();
        }
    }

    private void addForce(Vector3 force)
    {
        this.acceleration += force; //  *(1.0f/this.mass);
    }



    //A mettre à jour pour obtenir la position du "to", et ne pas normaliser
    private Vector3 NearestPositionInInfiniteArea(Vector3 to)
    {
        Vector3 from = this.transform.position;

        float minX = Mathf.Abs(from.x - to.x);
        float minZ = Mathf.Abs(from.z - to.z);

        if (from.x > to.x)
        {
            float xTemp = to.x + mapSizeX;
            if (Mathf.Abs(from.x - xTemp) < minX) to.x += mapSizeX;
        }
        else
        {
            float xTemp = from.x + mapSizeX;
            if (Mathf.Abs(xTemp - from.x) < minX) to.x -= mapSizeX;
        }

        if (from.z > to.z)
        {
            float zTemp = to.z + mapSizeZ;
            if (Mathf.Abs(from.z - zTemp) < minZ) to.z += mapSizeZ;
        }
        else
        {
            float zTemp = from.z + mapSizeZ;
            if (Mathf.Abs(zTemp - from.z) < minZ) to.z -= mapSizeZ;
        }

        return to;
    }


    private void StayInInfiniteArea()
    {
        Vector3 temp = this.transform.position;
        if (this.transform.position.x > mapSizeX) temp.x -= mapSizeX;
        if (this.transform.position.x < 0.0f) temp.x += mapSizeX;

        if (this.transform.position.z > mapSizeZ) temp.z -= mapSizeZ;
        if (this.transform.position.z < 0.0f) temp.z += mapSizeZ;

        this.transform.position = temp;
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
        avoidingObstaclesIntensity = this.parameterManager.GetAvoidingObstaclesIntensity();
        fieldOfViewSize = this.parameterManager.GetFieldOfViewSize();
        moveForwardIntensity = this.parameterManager.GetMoveForwardIntensity();
        randomMovementIntensity = this.parameterManager.GetRandomMovementIntensity();
        frictionIntensity = this.parameterManager.GetFrictionIntensity();
        maxSpeed = this.parameterManager.GetMaxSpeed();
        feelerEnable = this.parameterManager.IsFeelerEnable();
        feelerDistance = this.parameterManager.GetFeelerDistance();
        feelerSize = this.parameterManager.GetFeelerSize();
    }


    #region Get Methods
    public Vector3 GetSpeed()
    {
        return speed;
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
