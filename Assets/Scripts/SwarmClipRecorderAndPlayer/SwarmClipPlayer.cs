using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.UI;
using UnityEditor;

public class SwarmClipPlayer : MonoBehaviour
{
    private LogClip clip;

    private bool loaded = false;
    private bool playing = false;
    private bool sliderValueChanged = false;

    private int fps;
    int nbFrames;

    float timer = 0.0f;

    int frameNumber = 0;

    private List<GameObject> actors = new List<GameObject>();
    public GameObject actorPrefab;

    [SerializeField]
    private Slider slider;

    // Start is called before the first frame update
    void Start()
    {
        loaded = LoadClip();

    }

    // Update is called once per frame
    void Update()
    {
        if (playing)
        {
            if (timer >= (1.0f / this.fps))
            {
                DisplayFrame();
                frameNumber = (frameNumber + 1) % nbFrames;
                sliderValueChanged = true;
                slider.value = (float) frameNumber / (float)(this.nbFrames-1);
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

    private bool LoadClip()
    {
        string filename = "/" + "test" + ".dat";
        if (File.Exists(Application.persistentDataPath + filename))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + filename, FileMode.Open);
            this.clip = (LogClip) bf.Deserialize(file);
            file.Close();

            //Get clip informations
            this.fps = clip.getFps();
            this.nbFrames = clip.getClipFrames().Count;

            //Camera positionning
            Camera mainCamera = FindObjectOfType<Camera>();
            mainCamera.transform.position = new Vector3(clip.GetMapSizeX() / 2.0f, Mathf.Max(clip.GetMapSizeZ(), clip.GetMapSizeX()), clip.GetMapSizeZ() / 2.0f);
            mainCamera.transform.rotation = Quaternion.Euler(90, 0, 0);

            //Create gameObject simulating the swarm
            return (true);
        }
        else
        {
            return (false);
        }
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


    public void PlayClip()
    {
        if (loaded)
        {
            playing = true;
        }
    }

    public void PauseCLip()
    {
        playing = false;
        slider.value = (float)frameNumber / (float)(this.nbFrames - 1);
    }

    public void ShowExplorer()
    {
        string path = EditorUtility.OpenFilePanel("Choose a file", Application.persistentDataPath, "");
        Debug.Log(path);
    }


}
