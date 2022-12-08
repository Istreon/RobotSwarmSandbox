using System.Collections.Generic;
using UnityEngine;

public class FrameDisplayer
{
    #region Private fields
    private GameObject actorPrefab;

    private List<GameObject> actors = new List<GameObject>();

    private Vector3 spatialOrigin;

    private List<Color> colorPalette;
    #endregion

    #region Contructor
    public FrameDisplayer(GameObject actorPrefab)
    {
        this.actorPrefab = actorPrefab;


        this.spatialOrigin = Vector3.zero;

        colorPalette = ColorTools.GetShuffledColorPalette(40);
    }
    #endregion

    #region Methods - Actors management
    /// <summary>
    /// This method check if there is the right amount of actors (<see cref="GameObject"/>) to simulate each agent of a clip.
    /// If there is more, it deletes the surplus actors. 
    /// Is there is less, it create new <see cref="GameObject"/> to fit the right amount of agents.
    /// </summary>
    /// <param name="numberOfAgents"> A <see cref="int"/> value that represent the right amount of actor needed.</param>
    public void AdjustActorNumber(int numberOfAgents)
    {
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
    /// Remove all actors and destroy their gameObject.
    /// </summary>
    private void ClearActors()
    {
        foreach(GameObject a in actors)
        {
            GameObject.Destroy(a.gameObject);
        }
        actors.Clear();
    }
    #endregion

    #region Methods - Frame display
    /// <summary>
    /// Displays the <see cref="LogClipFrame"/> set in parameter,
    /// by displaying the position of saved agents in the clip using actors.
    /// It display the frame coloring actors from the same clusters in an unique <see cref="UnityEngine.Color"/>, allowing an user to identify groups visually.
    /// </summary>
    /// <param name="frame"> The <see cref="LogClipFrame"/> value correspond to the frame which must be displayed.</param>
    public void DisplayColoredClusterFrame(LogClipFrame frame)
    {
        AdjustActorNumber(frame.getAgentData().Count);

        //Searching for fracture
        List<List<LogAgentData>> clusters = ClipTools.GetClusters(frame);

        int i = 0;
        int c = 0;
        foreach (List<LogAgentData> l in clusters)
        {
            foreach (LogAgentData a in l)
            {
                actors[i].transform.position = a.getPosition() + spatialOrigin;


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
        Debug.Log(i);
    }

    /// <summary>
    /// Displays the <see cref="LogClipFrame"/> set in parameter,, 
    /// by displaying the position of saved agents in the clip using actors.
    /// It display the frame in the simpliest way possible, meaning that all actor are the same <see cref="UnityEngine.Color"/>.
    /// </summary>
    /// /// <param name="frame"> The <see cref="LogClipFrame"/> value correspond to the frame which must be displayed.</param>
    public void DisplaySimpleFrame(LogClipFrame frame)
    {
        int numberOfAgents = frame.getAgentData().Count;

        AdjustActorNumber(numberOfAgents);

        
        //Update actors position
        for (int i = 0; i < numberOfAgents; i++)
        {
            actors[i].transform.position = frame.getAgentData()[i].getPosition() + spatialOrigin;
            actors[i].GetComponent<Renderer>().material.color = Color.red;
        }
    }
    #endregion

    #region Methods - Setter
    public void SetSpatialOrigin(Vector3 origin)
    {
        this.spatialOrigin = origin;
    }

    public void setActorPrefab(GameObject prefab)
    {
        this.actorPrefab = prefab;
        ClearActors();
    }
    #endregion
}
