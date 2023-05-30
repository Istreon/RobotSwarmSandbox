using System.Collections.Generic;
using UnityEngine;
using System;

public class WeaknessDetector : MonoBehaviour
{
    AgentManager agentManager;

    public GameObject prefab;

    private Gradient gradient;
    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;

    List<GameObject> displayCube = new List<GameObject>();

    List<Tuple<Agent, Agent, float>> links = new List<Tuple<Agent, Agent, float>>();

    // Start is called before the first frame update
    void Start()
    {
        agentManager = FindObjectOfType<AgentManager>();
        if (agentManager == null) Debug.LogError("AgentManager is missing in the scene", this);


        gradient = new Gradient();

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        colorKey = new GradientColorKey[2];
        colorKey[0].color = Color.red;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.green;
        colorKey[1].time = 1.0f;




        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;


        gradient.SetKeys(colorKey, alphaKey);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach(GameObject g in displayCube)
        {
            Destroy(g);
        }
        displayCube.Clear();

        List<GameObject> agents = agentManager.GetAgents();

        UpdateLinksList(agents);

        Debug.Log(links.Count);

        /*
        foreach (GameObject a in agents)
        {
            float agentScore = ComputeAgentScore(a.GetComponent<Agent>(), agents);
            GameObject temp = GameObject.Instantiate(prefab);
            temp.GetComponent<Renderer>().material.color = gradient.Evaluate(agentScore);
            temp.transform.position = a.transform.position;

            displayCube.Add(temp);
        }
        */

        foreach (GameObject g in agents)
        {
            g.GetComponent<Agent>().SavePosition();
        }

        //List<List<GameObject>> neighbours = GetAgentLinks(agents[0].GetComponent<Agent>());

        //GetAgentLinksNumber(neighbours);

        //DisplayLinkTension(agents[0].GetComponent<Agent>(), neighbours);
        //DisplayLinkAngle(agents[0].GetComponent<Agent>(), neighbours);



    }

    private void SortLinksUsingScore ()
    {
        for (int i = 1; i < links.Count; i++)
        {
            for (int j = 0; j < links.Count - i; j++)
            {
                if(links[j].Item3 < links[j+1].Item3)
                {
                    Tuple<Agent, Agent, float> temp = links[j];
                    links[j] = links[j + 1];
                    links[j + 1] = temp;
                }
            }
        }
    }


    private void UpdateLinksList(List<GameObject> agents)
    {
        links.Clear();

        foreach(GameObject g in agents)
        {
            Agent agent = g.GetComponent<Agent>();
            List<GameObject> neigbours = SwarmAnalyserTools.GetNeighbours(g, agents, agent.GetFieldOfViewSize(), agent.GetBlindSpotSize());

            foreach(GameObject n in neigbours)
            {
                Agent neighbour = n.GetComponent<Agent>();
                if(!ContainsLink(agent,neighbour))
                {
                    float score = ComputeLinkTensionScore(agent, neighbour);
                    Tuple<Agent, Agent, float> link = new Tuple<Agent, Agent, float>(agent, neighbour,score);
                    links.Add(link);
                }
            }
        }

        SortLinksUsingScore();
    }


    private bool ContainsLink(Agent agent, Agent neighbour)
    {
        bool exists = false;
        foreach(Tuple<Agent,Agent,float> t in links)
        {
            if (System.Object.ReferenceEquals(t.Item1, agent) && System.Object.ReferenceEquals(t.Item2, neighbour)) exists = true;
            if (System.Object.ReferenceEquals(t.Item1, neighbour) && System.Object.ReferenceEquals(t.Item2, agent)) exists = true;
        }

        return exists;
    }

    /*
    private float ComputeAgentScore(Agent agent, List<GameObject> agents)
    {
        List<GameObject> neigbours = SwarmAnalyserTools.GetNeighbours(agent.gameObject, agents, agent.GetFieldOfViewSize(), agent.GetBlindSpotSize());

        float score = 0;
        foreach(GameObject g in neigbours)
        {
            score += ComputeLinkScore(agent, g.GetComponent<Agent>());
        }

        if(neigbours.Count<0)  score /= neigbours.Count;


        return score;
    }
    */

    private float ComputeLinkTensionScore(Agent agent, Agent neighbour)
    {
        return ComputeLinkTension(agent, neighbour) * (ComputeLinkTensionEvolution(agent, neighbour) + 1.0f);
    }


    private float ComputeLinkTension(Agent agent, Agent neighbour)
    {
        float dist = Vector3.Distance(neighbour.transform.position, agent.transform.position);
        float ratio = dist / agent.GetFieldOfViewSize();

        return ratio;
    }

    private float ComputeLinkTensionEvolution(Agent agent, Agent neighbour)
    {
        float dist = Vector3.Distance(neighbour.transform.position, agent.transform.position);
        float pastDist = Vector3.Distance(neighbour.GetSavedPosition(), agent.GetSavedPosition());

        float change = dist - pastDist;

        float ratio = change / agent.GetFieldOfViewSize();

        ratio *= Time.fixedDeltaTime;

        return ratio;
    }

    /*
    public List<List<GameObject>> GetAgentLinks(Agent agent)
    {
        List<GameObject> reciprocalNeigbours = SwarmAnalyserTools.GetNeighboursBasedOnTypeLink(agent.gameObject, agentManager.GetAgents(), agent.GetFieldOfViewSize(), agent.GetBlindSpotSize(), SwarmAnalyserTools.LinkType.reciprocal);
        List<GameObject> unidirectional_to_neighbour = SwarmAnalyserTools.GetNeighboursBasedOnTypeLink(agent.gameObject, agentManager.GetAgents(), agent.GetFieldOfViewSize(), agent.GetBlindSpotSize(), SwarmAnalyserTools.LinkType.unidirectional);
        List<GameObject> unidirectional_to_agent = SwarmAnalyserTools.GetNeighboursBasedOnTypeLink(agent.gameObject, agentManager.GetAgents(), agent.GetFieldOfViewSize(), agent.GetBlindSpotSize(), SwarmAnalyserTools.LinkType.unidirectional_reverse);

        List<List<GameObject>> neighbours = new List<List<GameObject>>();
        neighbours.Add(reciprocalNeigbours);
        neighbours.Add(unidirectional_to_neighbour);
        neighbours.Add(unidirectional_to_agent);

        return neighbours;
    }

    private float GetAgentLinksNumber(List<List<GameObject>> neighbours)
    {

        Debug.Log(neighbours[0].Count + "," + neighbours[1].Count + "," + neighbours[2].Count);

        return neighbours[0].Count + neighbours[1].Count + neighbours[2].Count;
    }


    private void DisplayLinkTension(Agent agent, List<List<GameObject>> neighbours)
    {
        string line = "{";
       foreach (List<GameObject> l in neighbours)
       {
            line += "{";
            foreach(GameObject a in l)
            {
                //Vector3 link = a.transform.position - agent.transform.position;
                float dist = Vector3.Distance(a.transform.position, agent.transform.position);
                float ratio = dist / agent.GetFieldOfViewSize();
                line += ratio +";";
            }
            line += "}";
       }
        line += "}";
        Debug.Log(line);
    }

    private void DisplayLinkAngle(Agent agent, List<List<GameObject>> neighbours)
    {
        string line = "{";
        for(int i=0;i< neighbours.Count; i++)
        {
            line += "{";
            foreach (GameObject a in neighbours[i])
            {
                float val;
                float angle;
                float blindSpotSize;
                if (i<2)
                {
                    Vector3 dir = a.transform.position - agent.transform.position;
                    angle = Vector3.Angle(agent.GetSpeed(), dir);
                    blindSpotSize = agent.GetBlindSpotSize();
                } else
                {
                    Vector3 dir = agent.transform.position - a.transform.position;
                    angle = Vector3.Angle(a.GetComponent<Agent>().GetSpeed(), dir);
                    blindSpotSize = a.GetComponent<Agent>().GetBlindSpotSize();

                }
                if (blindSpotSize == 0) val = -1;
                else val = 180 - (blindSpotSize / 2) - angle;
                line += val + ";";

            }
            line += "}";
        }
        line += "}";
        Debug.Log(line);
    }*/

    /*
    private float ComputeAngleScore(Agent agent, Agent neighbour)
    {
        float val1 = 0, val2=0 ;
        float angle;
        float blindSpotSize;

        Vector3 dir = neighbour.transform.position - agent.transform.position;
        angle = Vector3.Angle(agent.GetSpeed(), dir);
        blindSpotSize = agent.GetBlindSpotSize();


        if (angle <= (180 - (blindSpotSize / 2)))
        {
            if (blindSpotSize == 0) val1 = 180;
            else val1 = 180 - (blindSpotSize / 2) - angle;
        }


        dir = agent.transform.position - neighbour.transform.position;
        angle = Vector3.Angle(neighbour.GetSpeed(), dir);
        blindSpotSize = neighbour.GetBlindSpotSize();


        if (angle <= (180 - (blindSpotSize / 2)))
        {
            if (blindSpotSize == 0) val2 = 180;
            else val2 = 180 - (blindSpotSize / 2) - angle;
        }
        val1 /= 180;
        val2 /= 180;

        return Mathf.Max(val1, val2);

    }
    */


}
