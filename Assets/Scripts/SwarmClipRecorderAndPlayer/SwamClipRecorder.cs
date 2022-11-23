using System.Collections.Generic;
using UnityEngine;


public class SwamClipRecorder : MonoBehaviour
{
    #region Serialized fields
    [SerializeField]
    private int fps = 60; //frames per second
    #endregion

    #region Private fields
    private AgentManager agentManager;
    private ParameterManager parameterManager;

    private bool recording = false;

    float timer = 0.0f;

    private List<LogClipFrame> frames;
    #endregion

    #region MonoBehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {
        agentManager = FindObjectOfType<AgentManager>();
        if (agentManager == null) Debug.LogError("AgentManager is missing in the scene", this);

        parameterManager = FindObjectOfType<ParameterManager>();
        if (parameterManager == null) Debug.LogError("ParameterManager is missing in the scene", this);

        frames = new List<LogClipFrame>();
    }

    // Update is called once per frame
    void Update()
    {
        if (recording)
        {
            if (timer >= (1.0f / fps))
            {
                frames.Add(RecordFrame());
                Debug.Log("--frame");
                timer = timer - (1.0f / fps);
            }
            timer += Time.deltaTime;
        }
        else
        {
            if (frames.Count > 0)
            {
                Debug.Log("Clip Saved");
                LogClip clip = new LogClip(frames, fps, agentManager.GetMapSizeX(), agentManager.GetMapSizeZ());
                //Create filename based on current date time
                string date = System.DateTime.Now.ToString("yyyyMMddHHmmss");
                string filename = "/" + "clip_" + date + ".dat";
                //Save clip
                SwarmClipTools.SaveClip(clip, Application.persistentDataPath + filename);
                //Refresh recorder
                timer = 0.0f;
                frames.Clear();
            }
        }
    }
    #endregion

    #region Methods
    /**----------------------------
     * Reverses the state of the recorder
     * 
     * Return value :
     * -There is no return value
     * */
    public void ChangeRecordState()
    {
        recording = !recording;
    }

    /**----------------------------
     * Record the actual frame of the swarm, and add it to the list of frames
     * 
     * Return value :
     * -Return the recorded frame (LogCLipFrame)
     * */
    private LogClipFrame RecordFrame()
    {
        List<LogAgentData> agentData = new List<LogAgentData>();
        List<GameObject> agents = agentManager.GetAgents();

        foreach (GameObject o in agents)
        {
            Agent a = o.GetComponent<Agent>();
            LogAgentData log = new LogAgentData(a.transform.position, a.GetSpeed());
            agentData.Add(log);
        }
        LogParameters parameters = new LogParameters(parameterManager.GetFieldOfViewSize(), parameterManager.GetBlindSpotSize(), parameterManager.GetMoveForwardIntensity(), parameterManager.GetRandomMovementIntensity(), parameterManager.GetFrictionIntensity(), parameterManager.GetMaxSpeed(), parameterManager.GetCohesionIntensity(), parameterManager.GetAlignmentIntensity(), parameterManager.GetSeparationIntensity(), parameterManager.GetDistanceBetweenAgents());
        LogClipFrame frame = new LogClipFrame(agentData, parameters);
        return frame;
    }
    #endregion


}

