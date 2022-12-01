using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;


public class SwarmClipPlayer : MonoBehaviour
{
    #region Serialized fields
    [SerializeField]
    private GameObject actorPrefab;

    [SerializeField]
    private GameObject buttonsIfClipLoaded;

    [SerializeField]
    private GameObject saveButton;

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private Button playButton;

    [SerializeField]
    private List<Color> colorPalette;
    #endregion

    #region Private fields - clip parameters
    private LogClip clip;

    private int fps;
    private int nbFrames;
    private string filePath = "";
    #endregion

    #region Private fields - clip player parameters
    private bool loaded = false;
    private bool playing = false;
    private bool sliderValueChanged = false;
    private bool modifiedClip = false;
    private bool displayClusterColors = true;


    float timer = 0.0f;

    int frameNumber = 0;

    private List<GameObject> actors = new List<GameObject>();
    #endregion

    #region Methods - MonoBehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {
        colorPalette = ColorTools.GetShuffledColorPalette(40);
    }

    // Update is called once per frame
    void Update()
    {
        //Display or hide UI button
        buttonsIfClipLoaded.SetActive(loaded);
        saveButton.SetActive(modifiedClip);


        if (playing)
        {
            if (timer >= (1.0f / this.fps))
            {
                DisplayFrame();
                frameNumber = (frameNumber + 1) % nbFrames;
                UpdateSliderValue();
                timer = timer - (1.0f / fps);
            }
            timer += Time.deltaTime;
        }
    }
    #endregion

    #region Methods - Clip player methods

    /// <summary>
    /// This method check if there is the right amount of actors (<see cref="GameObject"/>) to simulate each agent of the clip.
    /// If there is more, it deletes the surplus actors. 
    /// Is there is less, it create new <see cref="GameObject"/> to fit the right amount of agents.
    /// </summary>
    private void AdjustActorNumber()
    {
        int numberOfAgents = clip.getClipFrames()[frameNumber].getAgentData().Count;
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
    /// This method displays the current frame of the loaded <see cref="LogClip"/>.
    /// By displaying the position of saved agents in the clip using actors.
    /// There is two possiblities, depending on <see cref="SwarmClipPlayer.displayClusterColors"/> value : 
    /// <see cref="SwarmClipPlayer.DisplaySimpleFrame"/> displays the frame in the simpliest way possible, meaning that all actor are the same color.
    /// <see cref="SwarmClipPlayer.DisplayColoredClusterFrame"/> displays the frame coloring actors from the same clusters in an unique color, allowing an user to identify groups visually.
    /// </summary>
    private void DisplayFrame()
    {
        if(displayClusterColors)
        {
            DisplayColoredClusterFrame();
        } 
        else
        {
            DisplaySimpleFrame();
        }
    }


    /// <summary>
    /// This method displays the current frame of the loaded <see cref="LogClip"/>,
    /// by displaying the position of saved agents in the clip using actors.
    /// It display the frame coloring actors from the same clusters in an unique <see cref="UnityEngine.Color"/>, allowing an user to identify groups visually.
    /// </summary>
    private void DisplayColoredClusterFrame()
    {
        AdjustActorNumber();

        //Searching for fracture
        List<List<LogAgentData>> clusters = GetClusters();

        int i = 0;
        int c = 0;
        foreach(List<LogAgentData> l in clusters)
        {
            foreach(LogAgentData a in l)
            {
                actors[i].transform.position = a.getPosition();


                if (clusters.Count > colorPalette.Count)
                {
                    actors[i].GetComponent<Renderer>().material.color = Color.black;
                }
                else
                {
                    actors[i].GetComponent<Renderer>().material.color = colorPalette[c];
                } 
                i++;
            }
            c++;
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

        int numberOfAgents = clip.getClipFrames()[frameNumber].getAgentData().Count;
        //Update actors position
        for (int i = 0; i < numberOfAgents; i++)
        {
            actors[i].transform.position = clip.getClipFrames()[frameNumber].getAgentData()[i].getPosition();
            actors[i].GetComponent<Renderer>().material.color = Color.red;
        }
    }


    /// <summary>
    /// This method udpates the visual state of the slider in the UI.
    /// In doing so, it prevent the call of the "on slider change" by setting a bool value to true.
    /// </summary>
    private void UpdateSliderValue()
    {
        sliderValueChanged = true;
        slider.value = (float)frameNumber / (float)(this.nbFrames - 1);
    }

    /// <summary>
    /// This methods allow to select a specific frame moment in the clip.
    /// This selection is based on the slider value  (set in parameter in the editor).
    /// Reload the display to show the actual frame of the clip.
    /// </summary>
    public void SelectFrame()
    {
        if (!sliderValueChanged)
        {
            this.frameNumber = (int)((this.nbFrames - 1) * slider.value);
            DisplayFrame();
        }
        sliderValueChanged = false;
    }

    /// <summary> 
    /// This method reverses the state of the clip player.
    /// If the clip player was playing, then this method changes it state to pause.
    /// Else, if the clip player was paused, this method changes it state to play
    /// </summary>
    public void PlayOrPauseClip()
    {
        if (loaded)
        {
            playing = !playing;
            playButton.GetComponentInChildren<TMP_Text>().text = playing ? "Pause" : "Play";
        }
    }

    /// <summary>
    /// This method sets the state of the clip player to "pause".
    /// </summary>
    public void PauseClipPlayer()
    {
        playing = false;
        playButton.GetComponentInChildren<TMP_Text>().text = "Play";
    }

    /// <summary>
    /// This method allows to remove a part of the clip, to reduce it size. The removed part is lost"/>
    /// </summary>
    /// <param name="firstPart">
    /// A <see cref="bool"/> value that decides whether the first or the last part of the clip which is deleted. 
    /// True, the first part is deleted. 
    /// False, the last part is deleted.
    /// </param>
    public void RemoveClipPart(bool firstPart)
    {
        //If there are enough frame to cut the clip
        if (clip.getClipFrames().Count > 2)
        {
            if (firstPart) //Remove the first part of the clip
            {
                clip.getClipFrames().RemoveRange(0, frameNumber);

                frameNumber = 0;
            }
            else //Remove the last part of the clip
            {
                clip.getClipFrames().RemoveRange(frameNumber, this.nbFrames - frameNumber);
                frameNumber = clip.getClipFrames().Count - 1;
            }
            modifiedClip = true;
            this.nbFrames = clip.getClipFrames().Count;
            UpdateSliderValue();
            DisplayFrame();

        }
    }

    /// <summary>
    /// Save a modified clip, under the original filename adding "_mod", in the same folder as the original clip. 
    /// A clip can't be saved using this method if it wasn't modified.
    /// </summary>
    public void SaveUpdatedClip()
    {
        if (modifiedClip)
        {
            int pos = filePath.LastIndexOf('.');
            string newFilePath = filePath.Remove(pos);
            newFilePath += "_mod.dat";
            Debug.Log(newFilePath);
            SwarmClipTools.SaveClip(clip, newFilePath);
        }
    }

    public void ChangeVisualization()
    {
        displayClusterColors = !displayClusterColors;
        DisplayFrame();
    }

    /// <summary>
    /// This method intialize a clip, allowing it to be play in the right conditions.
    /// It pause the player, get the clip parameter, set the camera position according to the clip parameters, and then display the first frame (and update the UI).
    /// </summary>
    private void InitializeClip()
    {
        //Pause clip player
        PauseClipPlayer();

        //Reset values for a new clip
        modifiedClip = false;
        frameNumber = 0;

        //Get clip informations
        this.fps = clip.getFps();
        this.nbFrames = clip.getClipFrames().Count;

        //Camera positionning
        Camera mainCamera = FindObjectOfType<Camera>();
        mainCamera.transform.position = new Vector3(clip.GetMapSizeX() / 2.0f, Mathf.Max(clip.GetMapSizeZ(), clip.GetMapSizeX()), clip.GetMapSizeZ() / 2.0f);
        mainCamera.transform.rotation = Quaternion.Euler(90, 0, 0);

        //Udpate UI
        UpdateSliderValue();
        DisplayFrame();
    }

    /// <summary>
    /// Open a windows explorer to select a clip and load it.
    /// Allowed files have .dat extension.
    /// Once the clip is load, it's intialized.
    /// </summary>
    public void ShowExplorer()
    {
        var extensions = new[] {
        new ExtensionFilter("Data files", "dat" ),
        };
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, true);
        filePath = paths[0];

        if (filePath != string.Empty)
        {
            clip = SwarmClipTools.LoadClip(filePath);
            loaded = (clip != null);

            if (loaded)
            {
                Debug.Log("Clip loaded");
                InitializeClip();
            }
        }
    }

    public void ExitApp()
    {
        Application.Quit();
    }
    #endregion


    #region Methods - Analyser

    /// <summary>
    /// Analyse the loaded clip and get the différent groups based on agent perception and graph theory.
    /// An agent belong to only one cluster.
    /// </summary>
    /// <returns> A <see cref="List{T}"/> of clusters represented by a <see cref="List{T}"/> of <see cref="LogAgentData"/>.</returns>
    private List<List<LogAgentData>> GetClusters()
    {
        //Reset clusters list
        List<List<LogAgentData>> clusters = new List<List<LogAgentData>>();

        //Create a clone of the log agent data list, to manipulate it
        List<LogAgentData> agentsClone = new List<LogAgentData>(clip.getClipFrames()[frameNumber].getAgentData());

        while (agentsClone.Count > 0)
        {
            //Add first agent in the new cluster
            List<LogAgentData> newCluster = new List<LogAgentData>();
            LogAgentData firstAgent = agentsClone[0];
            agentsClone.Remove(firstAgent);
            newCluster.Add(firstAgent);

            int i = 0;
            while (i < newCluster.Count)
            {
                List<LogAgentData> temp = GetNeighbours(newCluster[i], clip.getClipFrames()[frameNumber].getAgentData(), clip.getClipFrames()[frameNumber].GetParameters().GetFieldOfViewSize(), clip.getClipFrames()[frameNumber].GetParameters().GetBlindSpotSize());
                foreach (LogAgentData g in temp)
                {
                    if (!newCluster.Contains(g))
                    {
                        bool res = agentsClone.Remove(g);
                        if (res) newCluster.Add(g);
                    }
                }
                i++;
            }
            clusters.Add(newCluster);
        }
        return clusters;
    }

    /// <summary>
    /// Detect all neighbours of an agent based on its perception, and return them.
    /// </summary>
    /// <param name="agent"> A <see cref="LogAgentData"/> representing the agent searching its neighbours.</param>
    /// <param name="agentList"> A <see cref="List{T}"/>  of all the agent, containing the possible neighbours.</param>
    /// <param name="fieldOfViewSize"> A <see cref="float"/> value representing the distance of perception of the agent.</param>
    /// <param name="blindSpotSize"> A <see cref="float"/> value representing the blind angle behind the agent where neighbours are not perceived.</param>
    /// <returns> The <see cref="List{T}"/> of neighbours.</returns>
    private List<LogAgentData> GetNeighbours(LogAgentData agent, List<LogAgentData> agentList, float fieldOfViewSize, float blindSpotSize) {
        List<LogAgentData>  detectedAgents = new List<LogAgentData>();
        foreach (LogAgentData g in agentList)
        {
            if (System.Object.ReferenceEquals(g, agent)) continue;
            if (Vector3.Distance(g.getPosition(), agent.getPosition()) <= fieldOfViewSize)
            {
                Vector3 dir = g.getPosition() - agent.getPosition();
                float angle = Vector3.Angle(agent.getSpeed(), dir);

                if (angle <= 180 - (blindSpotSize / 2))
                {
                    detectedAgents.Add(g);
                }
            }
        }
        return detectedAgents;
    }
    #endregion
}
