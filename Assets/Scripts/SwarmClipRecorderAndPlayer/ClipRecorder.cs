using System.Collections.Generic;
using UnityEngine;

public class ClipRecorder : MonoBehaviour
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
                frames.Add(agentManager.RecordFrame());
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
                string date = System.DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
                string filename = "/" + "clip_" + date + ".dat";
                //Save clip
                ClipTools.SaveClip(clip, Application.dataPath + "/RecordedClips" + filename);
                //Refresh recorder
                timer = 0.0f;
                frames.Clear();
            }
        }
    }
    #endregion

    #region Methods

    /// <summary>
    /// Reverse the state of the recorder
    /// </summary>
    public void ChangeRecordState()
    {
        recording = !recording;
    }
    #endregion
}
