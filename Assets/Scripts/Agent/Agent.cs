using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{

    [Header("Field of view size")]
    [SerializeField]
    [Range(0.0f, 5.0f)]
    [Tooltip("This is the size of the radius.")]
    protected float fieldOfViewSize = 1.0f;
    [SerializeField]
    [Range(0, 360)]
    [Tooltip("This is the size of blind spot of the agent (in degrees)")]
    protected float blindSpotSize = 30;

    [Header("Intensity parameters")]
    [SerializeField]
    [Range(0.0f, 30.0f)]
    protected float moveForwardIntensity = 1.0f;
    [SerializeField]
    [Range(0.0f, 30.0f)]
    protected float randomMovementIntensity = 20.0f;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    protected float frictionIntensity = 0.1f;
    [SerializeField]
    [Range(0.0f, 2.0f)]
    protected float maxSpeed = 1.0f;



    #region Private fields
    //Field of view
    //private GameObject fieldOfView;
    //private SphereCollider fieldOfViewCollider;

    //Feeler
    //private GameObject feeler;
    //private SphereCollider feelerCollider;

    //private List<GameObject> detectedObstacles;


    protected Vector3 acceleration = Vector3.zero;
    protected Vector3 speed = Vector3.zero;


    protected List<GameObject> detectedAgents;


    protected float mapSizeX = 5.0f;
    protected float mapSizeZ = 5.0f;

    protected ParameterManager parameterManager;
    protected AgentManager agentManager;

    protected Vector3 savedPosition;

    protected PatternCreator forceField;



    #endregion

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected void updateAgent()
    {

        //Update agent position based on speed and time passed since last change
        this.transform.position += this.speed * Time.deltaTime;
        this.transform.position = new Vector3(this.transform.position.x, 0.001f, this.transform.position.z);
        //Update agent visual direction to match the current movement
        float agentDirection_YAxis = 180-(Mathf.Acos(this.speed.normalized.x) * 180.0f / Mathf.PI);
        if (this.speed.z < 0.0f) agentDirection_YAxis = agentDirection_YAxis * -1;
        this.transform.rotation = Quaternion.Euler(0.0f, agentDirection_YAxis, 0.0f);
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


    protected void addForce(Vector3 force)
    {
        this.acceleration += force; //  *(1.0f/this.mass);
    }


    protected void RandomMovement()
    {
        float alea = 0.1f;
        if (Random.value < alea)
        {
            float x = Random.value - 0.5f;
            float z = Random.value - 0.5f;
            Vector3 force = new Vector3(x, 0.0f, z);
            force.Normalize();

            //Modification de la puissance de cette force
            force *= randomMovementIntensity;

            addForce(force);
        }
    }


    protected void MoveForward()
    {
        Vector3 force = speed.normalized;

        //Modification de la puissance de cette force
        force *= moveForwardIntensity;

        addForce(force);
    }

    protected void Friction()
    {
        float k = frictionIntensity;
        Vector3 force = this.speed;
        force *= -k;
        addForce(force);
    }

    protected void EnvironmentalForce()
    {
        Vector2 temp = new Vector2(transform.position.x, transform.position.z);
        temp = forceField.GetEnvironmentalForce(temp);
        Vector3 force = new Vector3(temp.x, 0.0f, temp.y);
        force *= 100;
        addForce(force);
    }

    protected virtual void getAgentsInFieldOfView()
    {

        List<GameObject> agents = agentManager.GetAgents();
        detectedAgents = new List<GameObject>();

        foreach (GameObject g in agents)
        {
            if (GameObject.ReferenceEquals(g, this.gameObject)) continue;
            if (Vector3.Distance(g.transform.position, this.transform.position) <= fieldOfViewSize)
            {
                Vector3 dir  = g.transform.position - this.transform.position;
                float angle = Vector3.Angle(this.speed, dir);

                if (angle <= 180 - (blindSpotSize / 2))
                {
                    detectedAgents.Add(g);
                }
            }
        }
    }

    //A mettre à jour pour obtenir la position du "to", et ne pas normaliser
    protected Vector3 NearestPositionInInfiniteArea(Vector3 to)
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


    protected void StayInInfiniteArea()
    {
        Vector3 temp = this.transform.position;
        if (this.transform.position.x > mapSizeX) temp.x -= mapSizeX;
        if (this.transform.position.x < 0.0f) temp.x += mapSizeX;

        if (this.transform.position.z > mapSizeZ) temp.z -= mapSizeZ;
        if (this.transform.position.z < 0.0f) temp.z += mapSizeZ;

        this.transform.position = temp;
    }

    protected void StayInFiniteArea()
    {
        float x = 0.0f;
        float z = 0.0f;
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


    #region Get Methods
    public Vector3 GetSpeed()
    {
        return speed;
    }

    public virtual float GetFieldOfViewSize()
    {
        return fieldOfViewSize;
    }


    public List<GameObject> GetNeighbors()
    {
        if (detectedAgents == null) return new List<GameObject>();
        else return new List<GameObject>(detectedAgents);
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
