using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
//using AnotherFileBrowser.Windows;
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
    /**----------------------------
     * This method check if there is the right amount of actors (gameobject) to simulate each agent of the clip
     * If there is more, it deletes the surplus actor
     * Is there is less, it create new gameObjects to fit the right amount of agents
     * 
     * Return value :
     * -There is no return value
     **/
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

    /**----------------------------
     * This method displays the current frame of the loaded clip
     * By displaying the position of saved agents in the clip using actors
     * There is two possiblities :
     * -(See DisplaySimpleFrame method) It display the frame in the simpliest way possible, meaning that all actor are the same color
     * -(See DisplayColoredClusterFrame method) Or it display the frame coloring actors from the same clusters in an unique color, allowing an user to identify groups visually
     * 
     * Return value :
     * -There is no return value
     * */
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

    /**----------------------------
     * This method displays the current frame of the loaded clip
     * By displaying the position of saved agents in the clip using actors
     * It display the frame coloring actors from the same clusters in an unique color, allowing an user to identify groups visually
     * 
     * Return value :
     * -There is no return value
     * */
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

    /**----------------------------
     * This method displays the current frame of the loaded clip
     * By displaying the position of saved agents in the clip using actors
     * It display the frame in the simpliest way possible, meaning that all actor are the same color
     * 
     * 
     * Return value :
     * -There is no return value
     * */
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


    /**----------------------------
     * This method udpates the visual state of the slider in the UI
     * In doing so, it prevent the call of the "on slider change" by setting a bool value to true
     * This method do not return value
     * */
    private void UpdateSliderValue()
    {
        sliderValueChanged = true;
        slider.value = (float)frameNumber / (float)(this.nbFrames - 1);
    }


    /**----------------------------
     * This methods allow to select a specific frame moment in the clip
     * This selection is based on the slider value (set in parameter in the editor)
     * Reload the display to show the actual frame of the clip
     * This method do not return value
     * */
    public void SelectFrame()
    {
        if (!sliderValueChanged)
        {
            this.frameNumber = (int)((this.nbFrames - 1) * slider.value);
            DisplayFrame();
        }
        sliderValueChanged = false;
    }

    /** ----------------------------
     * This method reverses the state of the clip player
     * If the clip player was playing, then this method changes it state to pause
     * Else, if the clip player was paused, this method changes it state to play
     * This method do not return value
     * */
    public void PlayOrPauseClip()
    {
        if (loaded)
        {
            playing = !playing;
            playButton.GetComponentInChildren<TMP_Text>().text = playing ? "Pause" : "Play";
        }
    }

    /**----------------------------
     * This method sets the state of the clip player to "pause"
     * This method do not return value
     **/
    public void PauseClipPlayer()
    {
        playing = false;
        playButton.GetComponentInChildren<TMP_Text>().text = "Play";
    }

    /**----------------------------
     * This method allows to remove a part of the clip, to reduce it size.
     * There is two options : 
     * -remove the first part of the clip (all frame before "frameNumber") by setting "firstPart" parameter at true
     * -remove the last part of the clip (all frame after "frameNumber" included) by setting "firstPart" parameter at false
     * The removed part is lost, and there is no return value.
     * */
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

    /**----------------------------
     * This method allows to save a modified clip
     * When a clip is modified, the bool "modifiedClip" is set to True
     * Consequently, this method save the modified clip (under the original filename + "_mod") in the same folder as the original clip
     * This method do not return value
     * */
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

    /**----------------------------
     * This method intialize a clip, allowing to be play in the right conditions
     * It pause the player, get the clip parameter, set the camera position according to the clip parameters, and then display the first frame (and update the UI)
     * This method do not return value
     * */
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

    /**----------------------------
     * This method allow to select a clip and load it using a windows explorer 
     * The files have a .dat extension
     * When the clip is load, initialize it
     * This method do not return value
     **/
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
        /*
        #if UNITY_EDITOR

                //Only in editor
                this.filePath = EditorUtility.OpenFilePanel("Choose a file", Application.persistentDataPath, "");
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

                #else




                var bp = new BrowserProperties();
                bp.filter = "Images files (*.dat) | *.dat";
                bp.filterIndex = 0;

                new FileBrowser().OpenFileBrowser(bp, filePath =>
                {
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
                });
                #endif
                */

    }

    public void ExitApp()
    {
        Application.Quit();
    }
    #endregion


    #region Methods - Analyser
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
