using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentedVisual : MonoBehaviour
{
    public Material material;

    public bool displayVisual = false;

    public float intensity = 1.0f;

    AgentManager manager;
    ParameterManager parameterManager;
    private List<GameObject> agents;

    private List<LineRenderer> visualRenderer;


    // Start is called before the first frame update
    void Start()
    {

        manager = FindObjectOfType<AgentManager>();
        if (manager == null) Debug.LogError("AgentManager is missing in the scene", this);
        parameterManager = FindObjectOfType<ParameterManager>();
        if (parameterManager == null) Debug.LogError("ParameterManager is missing in the scene", this);


        visualRenderer = new List<LineRenderer>();


        this.material.SetFloat("_Mode", 2);

    }

    // Update is called once per frame
    void Update()
    {
        if (agents == null)
        {
            agents = manager.GetAgents();
        }
        ClearRenderer();
        if (displayVisual) DisplayVisual();

    }

    private void DisplayVisual()
    {
        Vector3 meanSwarmSpeed = Vector3.zero;
        foreach(GameObject a in agents)
        {
            meanSwarmSpeed += a.GetComponent<Agent>().GetSpeed();
        }
        meanSwarmSpeed = meanSwarmSpeed / agents.Count;

        foreach (GameObject a in agents)
        {
            Vector3 individualMouvement = a.GetComponent<Agent>().GetSpeed() - meanSwarmSpeed;

            //For creating line renderer object
            LineRenderer lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
            lineRenderer.startColor = Color.blue;
            lineRenderer.endColor = Color.blue;

            lineRenderer.startWidth = 0.01f; //If you need to change the width of line depending on the distance between both agents :  0.03f*(1-distOnMaxDistance) + 0.005f;
            lineRenderer.endWidth = 0.01f;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.material = material;
            //lineRenderer.material.SetFloat("_Mode", 2);
            lineRenderer.material.color = Color.blue;


            Vector3 temp = a.transform.position + (individualMouvement * intensity);
            //For drawing line in the world space, provide the x,y,z values
            lineRenderer.SetPosition(0, a.transform.position); //x,y and z position of the starting point of the line
            lineRenderer.SetPosition(1, temp); //x,y and z position of the end point of the line

            visualRenderer.Add(lineRenderer);
        }
    }

    private void ClearRenderer()
    {
        foreach (LineRenderer l in visualRenderer)
        {
            GameObject.Destroy(l.gameObject);
        }
        visualRenderer.Clear();
    }
}
