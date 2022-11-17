using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


public class SwamClipRecorder : MonoBehaviour
{
    [SerializeField]
    private int fps = 60; //frames per second

    private AgentManager agentManager;
    private ParameterManager parameterManager;

    private bool recording = false;

    float timer = 0.0f;

    private List<LogClipFrame> frames;

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
                SaveClip(clip);
                timer = 0.0f;
                frames.Clear();
            }
        }

    }

    public void ChangeRecordState()
    {
        recording = !recording;
    }

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

    private void SaveClip(LogClip clip)
    {
        string date = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        string filename = "/" + "clip_" + date + ".dat";
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + filename, FileMode.OpenOrCreate);
        Debug.Log(Application.persistentDataPath);
        //LogClip serializedClip = clip;
        bf.Serialize(file, clip);
        file.Close();
    }
}

