using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class SwarmClipPlayer : MonoBehaviour
{
    private LogClip clip;

    private int fps;
    private int nbFrames;
    private string filePath = "";

    private bool loaded = false;
    private bool playing = false;
    private bool sliderValueChanged = false;
    private bool modifiedClip = false;

    float timer = 0.0f;

    int frameNumber = 0;

    private List<GameObject> actors = new List<GameObject>();
    public GameObject actorPrefab;


    [SerializeField]
    private GameObject buttonsIfClipLoaded;

    [SerializeField]
    private GameObject saveButton;

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private Button playButton;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
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



    private void UpdateSliderValue()
    {
        sliderValueChanged = true;
        slider.value = (float)frameNumber / (float)(this.nbFrames - 1);
    }

    public void SelectFrame()
    {
        if(!sliderValueChanged)
        {
            this.frameNumber = (int)((this.nbFrames - 1) * slider.value);
            DisplayFrame();
        }
        sliderValueChanged = false;
    }


    public void PlayOrPauseClip()
    {
        if (loaded)
        {
            playing = ! playing;
            playButton.GetComponentInChildren<TMP_Text>().text = playing ? "Pause" : "Play";
        }
    }

    /**
     * This methods allow to remove a part of the clip, to reduce it size.
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


    public void ShowExplorer()
    {
        this.filePath = EditorUtility.OpenFilePanel("Choose a file", Application.persistentDataPath, "");
        if (filePath!=string.Empty)
        {
            clip = SwarmClipTools.LoadClip(filePath);
            loaded = (clip != null);

            if(loaded)
            {
                Debug.Log("Clip loaded");

                //Reset values for this new clip
                modifiedClip = false;
                frameNumber = 0;
                playing = false;

                //Get clip informations
                this.fps = clip.getFps();
                this.nbFrames = clip.getClipFrames().Count;
                

                //Camera positionning
                Camera mainCamera = FindObjectOfType<Camera>();
                mainCamera.transform.position = new Vector3(clip.GetMapSizeX() / 2.0f, Mathf.Max(clip.GetMapSizeZ(), clip.GetMapSizeX()), clip.GetMapSizeZ() / 2.0f);
                mainCamera.transform.rotation = Quaternion.Euler(90, 0, 0);

                UpdateSliderValue();
                DisplayFrame();
            }
        }
    }
}
