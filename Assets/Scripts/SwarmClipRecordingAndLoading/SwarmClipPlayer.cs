using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


public class SwarmClipPlayer : MonoBehaviour
{
    private LogClip clip;

    private bool loaded = false;

    private int fps;
    int nbFrames;

    float timer = 0.0f;

    int frameNumber = 0;

    private List<GameObject> actors = new List<GameObject>();
    public GameObject actorPrefab;

    // Start is called before the first frame update
    void Start()
    {
        loaded = Load();
        if (loaded)
        {
            this.fps = clip.getFps();
            this.nbFrames = clip.getClipFrames().Count;
            int numberOfAgents = clip.getClipFrames()[0].getNbAgents();
            
            for (int i = 0; i < numberOfAgents; i++)
            {
                GameObject newAgent = GameObject.Instantiate(actorPrefab);
                newAgent.transform.position = new Vector3(0.0f, 0.001f, 0.0f);
                newAgent.transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 359.0f), 0.0f);
                actors.Add(newAgent);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (loaded)
        {
            if (timer >= (1.0f / this.fps))
            {
                DisplayFrame();
                frameNumber = (frameNumber + 1) % nbFrames;
                timer = timer - (1.0f / fps);
            }
            timer += Time.deltaTime;
        }
    }

    private void DisplayFrame()
    {
        int numberOfAgents = clip.getClipFrames()[frameNumber].getNbAgents();

        for (int i = 0; i < numberOfAgents; i++)
        {
            actors[i].transform.position = clip.getClipFrames()[frameNumber].getAgentData()[i].getPosition();

        }
    }

    private bool Load()
    {
        string filename = "/" + "test" + ".dat";
        if (File.Exists(Application.persistentDataPath + filename))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + filename, FileMode.Open);
            SerializableLogClip serializedClip = (SerializableLogClip) bf.Deserialize(file);
            this.clip = serializedClip;
            file.Close();
            return (true);
        }
        else
        {
            return (false);
        }
    }
}
