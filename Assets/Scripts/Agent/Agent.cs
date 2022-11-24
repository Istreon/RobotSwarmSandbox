using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    #region Serialized fields - Perception parameters
    [Header("Field of view size")]
    [SerializeField]
    [Range(0.0f, 5.0f)]
    [Tooltip("This is the size of the radius (in meters).")]
    protected float fieldOfViewSize = 1.0f; //meters
    [SerializeField]
    [Range(0, 360)]
    [Tooltip("This is the size of blind spot of the agent (in degrees).")]
    protected float blindSpotSize = 30; //Degrees
    #endregion

    #region Serialized fields - Intensity parameters
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
    #endregion


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


    #region Methods - MonoBehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion

    #region Methods - Agent's primary methods
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

    /**----------------------------
     * This method add a force to the agent acceleration
     * 
     * Return value :
     * -There is no return value
     **/
    protected void addForce(Vector3 force)
    {
        this.acceleration += force; //  *(1.0f/this.mass);
    }

    /**----------------------------
     * This method get the other agents perceived by the current agent.
     * The perceived agent depend of the field of view size and the blind spot angle
     * Agents in the blind spot are not perceived
     * Agents further the field of view size are not perceived
     * 
     * Return value :
     * -There is no return value
     **/
    protected virtual void getAgentsInFieldOfView()
    {

        List<GameObject> agents = agentManager.GetAgents();
        detectedAgents = new List<GameObject>();

        foreach (GameObject g in agents)
        {
            if (GameObject.ReferenceEquals(g, this.gameObject)) continue;
            if (Vector3.Distance(g.transform.position, this.transform.position) <= fieldOfViewSize)
            {
                Vector3 dir = g.transform.position - this.transform.position;
                float angle = Vector3.Angle(this.speed, dir);

                if (angle <= 180 - (blindSpotSize / 2))
                {
                    detectedAgents.Add(g);
                }
            }
        }
    }
    #endregion

    #region Methods - Agent rules

    /**----------------------------
     * This method create a random force, added to the acceleration of the agent
     * It aim to allow an agent to move randomly
     * The greater the intensity, the greater the force
     * 
     * Return value :
     * -There is no return value
     **/
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

    /**----------------------------
    * This method create a force leading forward, added to the acceleration of the agent
    * It aim to allow an agent to move forward, depending of the move forward intensity
    * The greater the intensity, the greater the force
    * 
    * Return value :
    * -There is no return value
    **/
    protected void MoveForward()
    {
        Vector3 force = speed.normalized;

        //Modification de la puissance de cette force
        force *= moveForwardIntensity;

        addForce(force);
    }

    /**----------------------------
    * This method create a force opposite to the current speed, added to the acceleration of the agent
    * It aim to reduce the agent speed, depending of the friction intensity
    * 
    * Return value :
    * -There is no return value
    **/
    protected void Friction()
    {
        float k = frictionIntensity;
        Vector3 force = this.speed;
        force *= -k;
        addForce(force);
    }

    protected void AvoidCollisionWithNeighbors()
    {
        foreach(GameObject g in detectedAgents)
        {
            if(Vector3.Distance(g.transform.position, this.transform.position)<=0.09f) {
                Vector3 force = this.transform.position - g.transform.position;
                force.Normalize();
                force *= 20*this.maxSpeed;
                addForce(force);
            }
        }
    }

    /**----------------------------
    * This method create a force generated from the environmental forces of the map, added to the acceleration of the agent
    * It aim to influence the agent movement depending of the environmental constraints
    * 
    * Return value :
    * -There is no return value
    **/
    protected void EnvironmentalForce()
    {
        Vector2 temp = new Vector2(transform.position.x, transform.position.z);
        temp = forceField.GetEnvironmentalForce(temp);
        Vector3 force = new Vector3(temp.x, 0.0f, temp.y);
        force *= 100;
        addForce(force);
    }
    #endregion

    #region Methods - Finite area
    /**----------------------------
     * This method prevents the agent to leave the borders of the map
     * And add a force to push back the agent in the area
     * 
     * Return value :
     * -There is no return value
     **/
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
    #endregion

    #region Methods - Infinite area
    /**----------------------------
     * This method return the nearest position of an agent if we considere the environment as infinite
     * The area is infinite in height and width
     * Return value :
     * -(Vector 3) Nearest position in infinite area
     **/
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


    /**----------------------------
     * This method allow the agent to navigate in an infinite environment
     * When the agent goes beyond the borders of the map (in x or z), loops its position to the other border
     * 
     * Return value :
     * -The is no return value
     **/
    protected void StayInInfiniteArea()
    {
        Vector3 temp = this.transform.position;
        if (this.transform.position.x > mapSizeX) temp.x -= mapSizeX;
        if (this.transform.position.x < 0.0f) temp.x += mapSizeX;

        if (this.transform.position.z > mapSizeZ) temp.z -= mapSizeZ;
        if (this.transform.position.z < 0.0f) temp.z += mapSizeZ;

        //same as
        //temp.x = (temp.x + mapSizeX) % mapSizeX
        //temp.z = (temp.z + mapSizeZ) % mapSizeZ

        this.transform.position = temp;
    }
    #endregion

    #region Methods - Getter
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
