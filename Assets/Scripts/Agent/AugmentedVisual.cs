using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AugmentedVisual : MonoBehaviour
{
    public Material material;

    public bool displayVisual = false;
    public bool diplayConvexHul = false;

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
        if (diplayConvexHul) ConvexHul();

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

    private void ConvexHul()
    {

        List<List<GameObject>> clusters = SwarmAnalyserTools.GetClusters(agents);

        Debug.Log(clusters.Count);

        foreach (List<GameObject> c in clusters)
        {
            if (c.Count < 3) continue;
            List<Vector3> positions = new List<Vector3>();

            foreach (GameObject g in c)
            {
                positions.Add(g.transform.position);
            }

            //Calcul du point pivot
            float ordinate = float.MaxValue;
            float abcissa = float.MaxValue;
            Vector3 pivot = Vector3.zero;
            foreach (Vector3 p in positions)
            {
                if (p.z < ordinate || (p.z == ordinate && p.x < abcissa))
                {
                    pivot = p;
                    ordinate = pivot.z;
                    abcissa = pivot.x;
                }
            }
            positions.Remove(pivot);

            //Calcul des angles pour tri
            List<float> angles = new List<float>();
            Vector3 abissaAxe = new Vector3(1, 0, 0);
            foreach (Vector3 p in positions)
            {
                Vector3 temp = p - pivot;
                angles.Add(Vector3.Angle(temp, abissaAxe));
            }

            //Tri des points
            for (int i = 1; i < positions.Count; i++)
            {
                for (int j = 0; j < positions.Count - i; j++)
                {
                    if (angles[j] > angles[j + 1])
                    {
                        float temp = angles[j + 1];
                        angles[j + 1] = angles[j];
                        angles[j] = temp;

                        Vector3 tempPos = positions[j + 1];
                        positions[j + 1] = positions[j];
                        positions[j] = tempPos;
                    }
                }
            }
            angles.Clear();
            positions.Insert(0, pivot);

            //Itérations
            List<Vector3> pile = new List<Vector3>();
            pile.Add(positions[0]);
            pile.Add(positions[1]);

            for (int i = 3; i < positions.Count; i++)
            {
                while ((pile.Count >= 2) && VectorialProduct(pile[pile.Count - 2], pile[pile.Count - 1], positions[i]) <= 0 || pile[pile.Count - 1] == positions[i])
                {
                    pile.RemoveAt(pile.Count - 1);
                }
                pile.Add(positions[i]);
            }

            //Affichage

            for (int i = 0; i < pile.Count; i++)
            {
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

                //For drawing line in the world space, provide the x,y,z values
                int nextVal = (i + 1) % pile.Count;
                lineRenderer.SetPosition(0, pile[i]); //x,y and z position of the starting point of the line
                lineRenderer.SetPosition(1, pile[nextVal]); //x,y and z position of the end point of the line

                visualRenderer.Add(lineRenderer);
            }
        }
    }

    private float VectorialProduct(Vector3 a, Vector3 b, Vector3 c)
    {
        return ((b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z));
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
