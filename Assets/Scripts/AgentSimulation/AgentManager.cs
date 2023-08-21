using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private GameObject agentVisualPrefab;

    [SerializeField]
    private List<Displayer> displayers;

    [SerializeField]
    private GameObject mapPrefab;

    [SerializeField]
    private int numberOfAgents = 40;

    [SerializeField]
    [Range(5, 20)]
    private float mapSizeX = 5.0f;

    [SerializeField]
    [Range(5, 20)]
    private float mapSizeZ = 5.0f;


    private Camera mainCamera;


    private List<GameObject> agents;

    private ParameterManager parameterManager;

    private FrameDisplayer frameDisplayer;

    private Displayer[] existingDisplayers;

    private LogClipFrame frame;

    // Start is called before the first frame update
    void Start()
    {
        GameObject map=Instantiate(mapPrefab);
        map.transform.parent = null;
        map.transform.position = new Vector3(mapSizeX / 2.0f, 0.0f, mapSizeZ / 2.0f);
        map.transform.localScale = new Vector3(mapSizeX, 1.0f, mapSizeZ);

        mainCamera = FindObjectOfType<Camera>();
        mainCamera.transform.position= new Vector3(mapSizeX / 2.0f, Mathf.Max(mapSizeZ,mapSizeX), mapSizeZ / 2.0f);
        mainCamera.transform.rotation = Quaternion.Euler(90, 0, 0);


        parameterManager = FindObjectOfType<ParameterManager>();
        if (parameterManager == null) Debug.LogError("ParameterManager is missing in the scene", this);

        frameDisplayer = new FrameDisplayer(agentVisualPrefab);

        existingDisplayers = FindObjectsOfType<Displayer>();

        agents = new List<GameObject>();

        FrameTransmitter frameTransmitter = FindObjectOfType<FrameTransmitter>();


        if (frameTransmitter!=null)
        {
            LogClipFrame frame = frameTransmitter.GetFrameAndDestroy();
            this.numberOfAgents = frame.getAgentData().Count;

            for (int i = 0; i < numberOfAgents; i++)
            {
                GameObject newAgent = GameObject.Instantiate(prefab);
                newAgent.transform.position = frame.getAgentData()[i].getPosition();
                Agent a = newAgent.GetComponent<Agent>();
                a.SetSpeed(frame.getAgentData()[i].getSpeed());                
                agents.Add(newAgent);
            }
            LogParameters parameters = frame.GetParameters();
            
            parameterManager.SetAlignmentIntensity(parameters.GetAlignmentIntensity());
            parameterManager.SetCohesionIntensity(parameters.GetCohesionIntensity());
            parameterManager.SetSeparationIntensity(parameters.GetSeparationIntensity());

            parameterManager.SetBlindSpotSize(parameters.GetBlindSpotSize());
            parameterManager.SetFieldOfViewSize(parameters.GetFieldOfViewSize());
            parameterManager.SetDistanceBetweenAgents(parameters.GetDistanceBetweenAgents());

            parameterManager.SetFrictionIntensity(parameters.GetFrictionIntensity());
            parameterManager.SetMaxSpeed(parameters.GetMaxSpeed());
            parameterManager.SetMoveForwardIntensity(parameters.GetMoveForwardIntensity());
            parameterManager.SetRandomMovementIntensity(parameters.GetRandomMovementIntensity());

        } else
        {
            
            for (int i = 0; i < numberOfAgents; i++)
            {
                GameObject newAgent = GameObject.Instantiate(prefab);
                newAgent.transform.position = new Vector3(Random.Range(0.0f, mapSizeX), 0.001f, Random.Range(0.0f, mapSizeZ));
                newAgent.transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 359.0f), 0.0f);
                agents.Add(newAgent);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        this.frame = RecordFrame();
        frameDisplayer.DisplaySimpleFrame(frame);

        foreach(Displayer d in existingDisplayers)
        {
            d.ClearVisual();
        }

      /*  foreach(Displayer d in displayers)
        {
            d.DisplayVisual(frame);
        }*/
        
    }

    /// <summary> Record the current frame (state) of the swarm</summary>
    /// <returns> A <see cref="LogClipFrame"/> instance representing the recorded frame</returns>
    public LogClipFrame RecordFrame()
    {
        List<LogAgentData> agentData = new List<LogAgentData>();
        List<GameObject> agents = GetAgents();

        foreach (GameObject o in agents)
        {
            Agent a = o.GetComponent<Agent>();
            LogAgentData log = new LogAgentData(a.transform.position, a.GetSpeed(), a.GetAcceleration(), a.GetForces(), a.GetAgentBehaviour());
            agentData.Add(log);
        }
        LogParameters parameters = new LogParameters(parameterManager.GetFieldOfViewSize(), parameterManager.GetBlindSpotSize(), parameterManager.GetMoveForwardIntensity(), parameterManager.GetRandomMovementIntensity(), parameterManager.GetFrictionIntensity(), parameterManager.GetMaxSpeed(), parameterManager.GetCohesionIntensity(), parameterManager.GetAlignmentIntensity(), parameterManager.GetSeparationIntensity(), parameterManager.GetDistanceBetweenAgents());
        LogClipFrame frame = new LogClipFrame(agentData, parameters);
        return frame;
    }

    public List<GameObject> GetAgents()
    {
        return agents;
    }

    public float GetMapSizeX()
    {
        return mapSizeX;
    }

    public float GetMapSizeZ()
    {
        return mapSizeZ;
    }

    public LogClipFrame getFrame()
    {
        return this.frame;
    }
}
