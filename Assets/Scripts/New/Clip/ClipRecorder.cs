using System.Collections.Generic;
using UnityEngine;

public class ClipRecorder : MonoBehaviour
{
    #region Serialized fields
    [SerializeField]
    private int fps = 60; //frames per second
    #endregion

    #region Private fields
    private SwarmManager swarmManager;

    private bool recording = false;

    float timer = 0.0f;

    private List<SwarmData> frames;
    #endregion

    #region MonoBehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {
        swarmManager = FindObjectOfType<SwarmManager>();
        if (swarmManager == null) Debug.LogError("AgentManager is missing in the scene", this);

        frames = new List<SwarmData>();
    }

    // Update is called once per frame
    void Update()
    {
        if (recording)
        {
            if (timer >= (1.0f / fps))
            {
                frames.Add(swarmManager.CloneFrame());
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
                SwarmClip clip = new SwarmClip(frames, fps);
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