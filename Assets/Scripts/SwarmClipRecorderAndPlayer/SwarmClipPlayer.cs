using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using AnotherFileBrowser.Windows;

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


    float timer = 0.0f;

    int frameNumber = 0;

    private List<GameObject> actors = new List<GameObject>();
    #endregion



    #region Methods - MonoBehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {
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
    /**
     * This method displays the current frame of the loaded clip
     * Before doing it, is check if there is the right amount of actors (gameobject) to simulate each agent of the clip
     * If there is more, it deletes the surplus actor
     * Is there is less, it create new gameObjects to fit the right amount of agents
     * Then, it display the position of saved agents in the clip using actors
     * This method do not return value
     * */
    private void DisplayFrame()
    {
        int numberOfAgents = clip.getClipFrames()[frameNumber].getAgentData().Count;
        int numberOfActors = actors.Count;

        //Create missing actors
        if (numberOfActors<numberOfAgents)
        {
            for (int i = 0; i < (numberOfAgents-numberOfActors); i++)
            {
                GameObject newAgent = GameObject.Instantiate(actorPrefab);
                newAgent.transform.position = new Vector3(0.0f, 0.001f, 0.0f);
                newAgent.transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 359.0f), 0.0f);
                actors.Add(newAgent);
            }
        }

        //Destroy surplus actors
        if (numberOfActors>numberOfAgents)
        {
            for (int i = numberOfAgents; i < numberOfActors; i++)
            {
                GameObject.Destroy(actors[i].gameObject);
            }
            actors.RemoveRange(numberOfAgents, numberOfActors - numberOfAgents); 
        }

        //Update actors position
        for (int i = 0; i < numberOfAgents; i++)
        {
            actors[i].transform.position = clip.getClipFrames()[frameNumber].getAgentData()[i].getPosition();
        }
    }


    /**
     * This method udpates the visual state of the slider in the UI
     * In doing so, it prevent the call of the "on slider change" by setting a bool value to true
     * This method do not return value
     * */
    private void UpdateSliderValue()
    {
        sliderValueChanged = true;
        slider.value = (float)frameNumber / (float)(this.nbFrames - 1);
    }


    /**
     * This methods allow to select a specific frame moment in the clip
     * This selection is based on the slider value (set in parameter in the editor)
     * Reload the display to show the actual frame of the clip
     * This method do not return value
     * */
    public void SelectFrame()
    {
        if(!sliderValueChanged)
        {
            this.frameNumber = (int)((this.nbFrames - 1) * slider.value);
            DisplayFrame();
        }
        sliderValueChanged = false;
    }

    /** 
     * This method reverses the state of the clip player
     * If the clip player was playing, then this method changes it state to pause
     * Else, if the clip player was paused, this method changes it state to play
     * This method do not return value
     * */
    public void PlayOrPauseClip()
    {
        if (loaded)
        {
            playing = ! playing;
            playButton.GetComponentInChildren<TMP_Text>().text = playing ? "Pause" : "Play";
        }
    }

    /**
     * This method sets the state of the clip player to "pause"
     * This method do not return value
     **/
    public void PauseClipPlayer()
    {
        playing = false;
        playButton.GetComponentInChildren<TMP_Text>().text = "Play";
    }

    /**
     * This method allows to remove a part of the clip, to reduce it size.
     * There is two options : 
     * -remove the first part of the clip (all frame before "frameNumber") by setting "firstPart" parameter at true
     * -remove the last part of the clip (all frame after "frameNumber" included) by setting "firstPart" parameter at false
     * The removed part is lost, and there is no return value.
     * */
    public void RemoveClipPart(bool firstPart)
    {
        //If there are enough frame to cut the clip
        if(clip.getClipFrames().Count>2)
        {
            if (firstPart) //Remove the first part of the clip
            {
                clip.getClipFrames().RemoveRange(0, frameNumber);

                frameNumber = 0;
            }
            else //Remove the last part of the clip
            {
                clip.getClipFrames().RemoveRange(frameNumber, this.nbFrames - frameNumber);
                frameNumber = clip.getClipFrames().Count-1;
            }
            modifiedClip = true;
            this.nbFrames = clip.getClipFrames().Count;
            UpdateSliderValue();
            DisplayFrame();
            
        }
    }

    /**
     * This method allows to save a modified clip
     * When a clip is modified, the bool "modifiedClip" is set to True
     * Consequently, this method save the modified clip (under the original filename + "_mod") in the same folder as the original clip
     * This method do not return value
     * */
    public void SaveUpdatedClip()
    {
        if(modifiedClip)
        {
            int pos=filePath.LastIndexOf('.');
            string newFilePath = filePath.Remove(pos);
            newFilePath += "_mod.dat";
            Debug.Log(newFilePath);
            SwarmClipTools.SaveClip(clip, newFilePath);
        }
    }

    /**
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

    /**
     * This method allow to select a clip and load it using a windows explorer 
     * The files have a .dat extension
     * When the clip is load, initialize it
     * This method do not return value
     **/
    public void ShowExplorer()
    {
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

    }

    public void ExitApp()
    {
        Application.Quit();
    }

    #endregion
}
