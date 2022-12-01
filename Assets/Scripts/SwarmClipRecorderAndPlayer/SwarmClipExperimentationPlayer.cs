using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmClipExperimentationPlayer : MonoBehaviour
{
    #region Serialized fields
    [SerializeField]
    private GameObject actorPrefab;
    #endregion

    #region Private fields
    string filePath = "C:/Users/hmaym/OneDrive/Bureau/UnityBuild/Clip/"; //The folder containing clip files
    string[] fileNames = {
        "clip_20221121140352.dat",
        "clip_20221121140431.dat"
    };

    private List<LogClip> clips = new List<LogClip>();

    private int currentClip = 0;
    private int currentFrame = 0;

    private List<GameObject> actors = new List<GameObject>();

    private bool playing = true;

    float timer = 0.0f;

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        //Load all clip
        foreach(string fname in fileNames)
        {
            //Concatenation of file path and file name
            string s = this.filePath + fname;

            //Loading clip from full file path
            LogClip clip = SwarmClipTools.LoadClip(s);

            if (clip != null)
                clips.Add(clip); //Add the loaded clip to the list
            else
                Debug.LogError("Clip can't be load from " + s,this);

            //TO DO !!! (ajouter un visuel d'avancement du chargement en cas de nombreux clip, car cela peut être long)
        }
        if (clips.Count > 0)
            Debug.Log("Loading complete, " + clips.Count + " clips have been loaded");
        else
            playing = false;

        InitialiseCameraPlacement();
    }

    // Update is called once per frame
    void Update()
    {
        if(playing)
        {
            if (timer >= (1.0f / clips[currentClip].getFps()))
            {
                DisplaySimpleFrame();
                currentFrame ++;
                if(currentFrame >= clips[currentClip].getClipFrames().Count)
                {
                    currentClip++;
                    if(currentClip >= clips.Count)
                    {
                        Debug.Log("Experimentation finished");
                        playing = false;
                    } else
                    {
                        currentFrame = 0;
                        InitialiseCameraPlacement();
                        Debug.Log("Clip number " + currentClip);
                    }
                }
                timer = timer - (1.0f / clips[currentClip].getFps());
            }
            timer += Time.deltaTime;
        }
    }


    #region Methods - Clip player methods

    /// <summary>
    /// This method check if there is the right amount of actors (<see cref="GameObject"/>) to simulate each agent of the clip.
    /// If there is more, it deletes the surplus actors. 
    /// Is there is less, it create new <see cref="GameObject"/> to fit the right amount of agents.
    /// </summary>
    private void AdjustActorNumber()
    {
        int numberOfAgents = clips[currentClip].getClipFrames()[currentFrame].getAgentData().Count;
        int numberOfActors = actors.Count;

        //Create missing actors
        if (numberOfActors < numberOfAgents)
        {
            for (int i = 0; i < (numberOfAgents - numberOfActors); i++)
            {
                GameObject newAgent = GameObject.Instantiate(actorPrefab);
                newAgent.transform.position = new Vector3(0.0f, 0.001f, 0.0f);
                newAgent.transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 359.0f), 0.0f);
                actors.Add(newAgent);
            }
        }

        //Destroy surplus actors
        if (numberOfActors > numberOfAgents)
        {
            for (int i = numberOfAgents; i < numberOfActors; i++)
            {
                GameObject.Destroy(actors[i].gameObject);
            }
            actors.RemoveRange(numberOfAgents, numberOfActors - numberOfAgents);
        }
    }


    /// <summary>
    /// This method displays the current frame of the loaded <see cref="LogClip"/>, 
    /// by displaying the position of saved agents in the clip using actors.
    /// It display the frame in the simpliest way possible, meaning that all actor are the same <see cref="UnityEngine.Color"/>.
    /// </summary>
    private void DisplaySimpleFrame()
    {
        AdjustActorNumber();

        int numberOfAgents = clips[currentClip].getClipFrames()[currentFrame].getAgentData().Count;
        //Update actors position
        for (int i = 0; i < numberOfAgents; i++)
        {
            actors[i].transform.position = clips[currentClip].getClipFrames()[currentFrame].getAgentData()[i].getPosition();
        }
    }

    private void InitialiseCameraPlacement()
    {
        //Camera positionning
        Camera mainCamera = FindObjectOfType<Camera>();
        mainCamera.transform.position = new Vector3(clips[currentClip].GetMapSizeX() / 2.0f, Mathf.Max(clips[currentClip].GetMapSizeZ(), clips[currentClip].GetMapSizeX()), clips[currentClip].GetMapSizeZ() / 2.0f);
        mainCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
    }




    #endregion
}
