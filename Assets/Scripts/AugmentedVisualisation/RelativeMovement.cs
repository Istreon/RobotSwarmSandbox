using System.Collections.Generic;
using UnityEngine;

public class RelativeMovement : Displayer
{
    #region Serialized fields
    [SerializeField]
    private Material material;

    [SerializeField]
    private float intensity = 1.0f; //Percentage which adjusts the size of the arrow ( 1.0f = 100%)
    #endregion

    #region Private fields
    private List<LineRenderer> visualRenderer;
    #endregion

    #region Methods - MonoBehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {

        visualRenderer = new List<LineRenderer>();


        this.material.SetFloat("_Mode", 2);
    }
    #endregion

    #region Methods - Displayer Override
    public override void DisplayVisual(LogClipFrame frame)
    {
        ClearVisual();

        List<List<LogAgentData>> clusters =  FrameTools.GetClusters(frame);


        foreach (List<LogAgentData> c in clusters)
        {
            Vector3 meanSwarmSpeed = Vector3.zero;
            foreach (LogAgentData a in c)
            {
                meanSwarmSpeed += a.getSpeed();
            }
            meanSwarmSpeed = meanSwarmSpeed / c.Count;

            foreach (LogAgentData a in c)
            {
                Vector3 individualMouvement = a.getSpeed() - meanSwarmSpeed;

                //For creating line renderer object
                LineRenderer lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.red;

                lineRenderer.startWidth = 0.01f; //If you need to change the width of line depending on the distance between both agents :  0.03f*(1-distOnMaxDistance) + 0.005f;
                lineRenderer.endWidth = 0.01f;
                lineRenderer.positionCount = 2;
                lineRenderer.useWorldSpace = true;
                lineRenderer.material = material;
                //lineRenderer.material.SetFloat("_Mode", 2);
                lineRenderer.material.color = Color.red;


                Vector3 temp = a.getPosition() + (individualMouvement * intensity);
                //For drawing line in the world space, provide the x,y,z values
                lineRenderer.SetPosition(0, a.getPosition()); //x,y and z position of the starting point of the line
                lineRenderer.SetPosition(1, temp); //x,y and z position of the end point of the line

                lineRenderer.transform.parent = this.transform;

                visualRenderer.Add(lineRenderer);
            }
        }
    }

    public override void ClearVisual()
    {
        foreach (LineRenderer l in visualRenderer)
        {
            GameObject.Destroy(l.gameObject);
        }
        visualRenderer.Clear();
    }
    #endregion
}
