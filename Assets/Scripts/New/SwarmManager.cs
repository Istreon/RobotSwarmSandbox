using System;
using System.Collections.Generic;
using UnityEngine;

public class SwarmManager : MonoBehaviour
{
    #region Serialized fields
    [SerializeField]
    private GameObject mapPrefab;

    [SerializeField]
    private float numberOfAgents;

    [SerializeField]
    EditorParametersInterface parametersInterface;

    [SerializeField]
    private List<Displayer> displayers;
    #endregion

    private SwarmData swarm;

    private GameObject map;

    private Displayer[] existingDisplayers;

    

    // Start is called before the first frame update
    void Start()
    {
        //Instantiation of the map
        map = Instantiate(mapPrefab);
        map.transform.parent = null;

        existingDisplayers = FindObjectsOfType<Displayer>();

        parametersInterface = FindObjectOfType<EditorParametersInterface>();
        if (parametersInterface == null) {
            Debug.LogError("ParameterManager is missing in the scene", this);
        }


        FrameTransmitter frameTransmitter = FindObjectOfType<FrameTransmitter>();

        if (frameTransmitter != null)
        {
            SwarmData frame = frameTransmitter.GetFrameAndDestroy();
            swarm = frame.Clone();
            parametersInterface.SetParameters(swarm.GetParameters());
        }
        else
        {
            SwarmParameters parameters = parametersInterface.GetParameters();

            List<AgentData> agents = new List<AgentData>();
            for (int i = 0; i < numberOfAgents; i++)
            {

                Vector3 position = new Vector3(UnityEngine.Random.Range(0.0f, parameters.GetMapSizeX()), 0.0f, UnityEngine.Random.Range(0.0f, parameters.GetMapSizeZ()));
                Vector3 direction = new Vector3(UnityEngine.Random.Range(-1.0f, 1.0f), 0.0f, UnityEngine.Random.Range(-1.0f, 1.0f));

                AgentData agent = new AgentData(position, direction.normalized);
                agents.Add(agent);
            }
            swarm = new SwarmData(agents, parameters);
        }

            

        
    }

    // Update is called once per frame
    void Update()
    {
        //Update swarm parameters
        SwarmParameters parameters = parametersInterface.GetParameters();
        swarm.SetParameters(parameters);

        List<AgentData> agents = swarm.GetAgentsData();

        //Update Position, direction
        foreach (AgentData a in agents)
        {
            Tuple<Vector3, Vector3> positionAndDirection = MovementManager.ApplyAgentMovement(parameters.GetAgentMovement(), a, Time.deltaTime);

            Vector3 position = CorrectPosition(positionAndDirection.Item1, 0.04f);
            //Vector3 position = positionAndDirection.Item1;

            a.SetPosition(position);
            a.SetDirection(positionAndDirection.Item2);
        }

        //Reset forces and apply agent's behaviour
        foreach (AgentData a in agents)
        {
            //Clear forces
            a.ClearForces();

            //Get new agent's forces
            List<Vector3> forces = BehaviourManager.ApplySocialBehaviour(parameters.GetAgentBehaviour(), a, swarm);
            a.SetForces(forces);
        }

        foreach (AgentData a in agents)
        {
            a.UdpateAcceleration();
            Vector3 speed = a.UpdateSpeed(Time.deltaTime);

            //Limit speed vector based on agent max speed
            float maxSpeed = parameters.GetMaxSpeed();
            float temp = speed.sqrMagnitude; //faster than Vector3.Magnitude(this.speed);
            if (temp > (maxSpeed * maxSpeed)) // Temp is squared, so it's necessary to compare whith "maxSpeed" squared too
            {
                speed.Normalize();
                speed *= maxSpeed;
                a.SetSpeed(speed);
            }
        }


        //--Affichage--//
        foreach (Displayer d in existingDisplayers)
        {
            if(!displayers.Contains(d))
                d.ClearVisual();
        }

        foreach (Displayer d in displayers)
        {
            d.DisplayVisual(swarm);
        }

        UpdateMap();
    }

    #region Methods - Map
    private void UpdateMap()
    {
        float x = swarm.GetParameters().GetMapSizeX();
        float z = swarm.GetParameters().GetMapSizeZ();
        map.transform.position = new Vector3(x / 2.0f, 0.0f, z / 2.0f);
        map.transform.localScale = new Vector3(x, 1.0f, z);
    }

    public Vector3 CorrectPosition(Vector3 position, float objectRadius)
    {
        float mapSizeX = swarm.GetParameters().GetMapSizeX();
        float mapSizeZ = swarm.GetParameters().GetMapSizeZ();
  

        float x = position.x;
        float z = position.z;

        if (position.x > mapSizeX - objectRadius)
        {
            x = mapSizeX - objectRadius;
        }
        if (position.x < objectRadius)
        {
            x = objectRadius;
        }

        if (position.z > mapSizeZ - objectRadius)
        {
            z = mapSizeZ - objectRadius;
        }
        if (position.z < objectRadius)
        {
            z = objectRadius;
        }
        Vector3 newPosition = new Vector3(x, 0.0f, z);

        return newPosition;
    }
    #endregion

    #region Methods - 
    public SwarmData CloneFrame()
    {
        return swarm.Clone();
    }
    #endregion
}
