using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkWithNeighboursAnalyser : MonoBehaviour
{
    private Gradient gradient;
    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;


    AgentManager manager;
    private List<GameObject> agents;

    private List<LineRenderer> linksRenderer;


    // Start is called before the first frame update
    void Start()
    {

        manager = FindObjectOfType<AgentManager>();
        if (manager == null) Debug.LogError("AgentManager is missing in the scene", this);
        linksRenderer = new List<LineRenderer>();

        gradient = new Gradient();

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        colorKey = new GradientColorKey[3];
        colorKey[0].color = Color.blue;
        colorKey[0].time = 0.5f;
        colorKey[1].color = Color.white;
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
    void Update()
    {
        if (agents == null)
        {
            agents = manager.GetAgents();
        }
        ClearRenderer();
        foreach(GameObject a in agents)
        {
            Agent currentAgent = a.GetComponent<Agent>();
            List <GameObject> currentNeighbours = currentAgent.GetNeighbors();
            //Vector3 position;
            foreach (GameObject g in currentNeighbours)
            {


                float distOnMaxDistance = Vector3.Distance(g.transform.position, a.transform.position) / currentAgent.GetFieldOfViewSize();
                Color temp = gradient.Evaluate(distOnMaxDistance);

                //For creating line renderer object
                LineRenderer lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
                lineRenderer.startColor = temp;
                lineRenderer.endColor = temp;

                lineRenderer.startWidth = 0.01f;
                lineRenderer.endWidth = 0.01f;
                lineRenderer.positionCount = 2;
                lineRenderer.useWorldSpace = true;
                lineRenderer.material.color = temp;
                //For drawing line in the world space, provide the x,y,z values
                lineRenderer.SetPosition(0, currentAgent.transform.position); //x,y and z position of the starting point of the line
                lineRenderer.SetPosition(1, g.transform.position); //x,y and z position of the end point of the line

                linksRenderer.Add(lineRenderer);


                //if (pastNeighbours.TryGetValue(g, out position))
                //{
                //Time.deltaTime;   
                //}
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
