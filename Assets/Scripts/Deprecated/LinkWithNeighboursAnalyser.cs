using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkWithNeighboursAnalyser : MonoBehaviour
{
    private Gradient gradient;
    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;

    public Material material;

    public bool displayLinks = false;

    AgentManager manager;
    ParameterManager parameterManager;
    private List<GameObject> agents;

    private List<LineRenderer> linksRenderer;


   // Dictionary<GameObject, List<GameObject>> agentsAndPastNeighbours;

    // Start is called before the first frame update
    void Start()
    {

        manager = FindObjectOfType<AgentManager>();
        if (manager == null) Debug.LogError("AgentManager is missing in the scene", this);
        parameterManager = FindObjectOfType<ParameterManager>();
        if(parameterManager == null) Debug.LogError("ParameterManager is missing in the scene", this);


        linksRenderer = new List<LineRenderer>();

        gradient = new Gradient();

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        colorKey = new GradientColorKey[2];
        colorKey[0].color = Color.blue;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.blue;
        colorKey[1].time = 1.0f;




        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.5f;
        alphaKey[1].alpha = 0.05f;
        alphaKey[1].time = 1.0f;


        gradient.SetKeys(colorKey, alphaKey);

        this.material.SetFloat("_Mode", 2);

        //agentsAndPastNeighbours = new Dictionary<GameObject, List<GameObject>>();

        

    }

    // Update is called once per frame
    void Update()
    {
        if (agents == null)
        {
            agents = manager.GetAgents();
            /*foreach (GameObject a in agents)
            {
                agentsAndPastNeighbours.Add(a, new List<GameObject>());
            }*/
        }
        ClearRenderer();
        if(displayLinks) DisplayLinks();

    }

    private void DisplayLinks()
    {
        //Create a dictionary of agents and their neighbours, to remove freely already drawn pairs
        Dictionary<GameObject, List<GameObject>> agentAndNeighbours = new Dictionary<GameObject, List<GameObject>>();
        foreach (GameObject a in agents)
        {
            agentAndNeighbours.Add(a, a.GetComponent<Agent>().GetNeighbors());
        }
        foreach (KeyValuePair<GameObject, List<GameObject>> a in agentAndNeighbours)
        {
            GameObject currentAgent = a.Key;
            List<GameObject> currentNeighbours = a.Value;
            //Vector3 position;
            foreach (GameObject g in currentNeighbours)
            {
                List<GameObject> otherNeighbours;
                if (agentAndNeighbours.TryGetValue(g,out otherNeighbours))
                {
                    otherNeighbours.Remove(currentAgent); //Removing the pair from the other agent neighbours
                }

                 /*float distChanges = -1.0f;
                 List<GameObject> pastNeighbours;
                 if (agentsAndPastNeighbours.TryGetValue(currentAgent, out pastNeighbours))
                 {
                     if(pastNeighbours.Contains(g))
                     {
                         float oldDist = Vector3.Distance(currentAgent.GetComponent<Agent>().GetSavedPosition(), g.GetComponent<Agent>().GetSavedPosition());
                         float currentDist = Vector3.Distance(currentAgent.transform.position, g.transform.position);
                         distChanges = ((currentDist - oldDist)/ Time.deltaTime) / parameterManager.GetMaxSpeed() + parameterManager.GetMaxSpeed()/2;
                         Debug.Log(oldDist + "   " + currentDist + "  " +distChanges);
                     }
                     //Time.deltaTime;   
                 } 
                Color lineColor = gradient.Evaluate(distChanges);*/
                

                float distOnMaxDistance = Vector3.Distance(g.transform.position, currentAgent.transform.position) / currentAgent.GetComponent<Agent>().GetFieldOfViewSize();
                Color lineColor = gradient.Evaluate(distOnMaxDistance);


                //For creating line renderer object
                LineRenderer lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
                lineRenderer.startColor = lineColor;
                lineRenderer.endColor = lineColor;

                lineRenderer.startWidth = 0.01f; //If you need to change the width of line depending on the distance between both agents :  0.03f*(1-distOnMaxDistance) + 0.005f;
                lineRenderer.endWidth = 0.01f;
                lineRenderer.positionCount = 2;
                lineRenderer.useWorldSpace = true;
                lineRenderer.material = material;
                //lineRenderer.material.SetFloat("_Mode", 2);
                lineRenderer.material.color = lineColor;

                //For drawing line in the world space, provide the x,y,z values
                lineRenderer.SetPosition(0, currentAgent.transform.position); //x,y and z position of the starting point of the line
                lineRenderer.SetPosition(1, g.transform.position); //x,y and z position of the end point of the line


                lineRenderer.receiveShadows = false;
                lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                


                linksRenderer.Add(lineRenderer);


                //agentsAndPastNeighbours[currentAgent] = currentNeighbours;
            }
        }
    }

    private void ClearRenderer()
    {
        foreach (LineRenderer l in linksRenderer)
        {
            GameObject.Destroy(l.gameObject);
        }
        linksRenderer.Clear();
    }

}