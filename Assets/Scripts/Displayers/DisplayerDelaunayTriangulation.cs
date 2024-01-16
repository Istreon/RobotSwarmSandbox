using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayerDelaunayTriangulation : Displayer
{
    #region Serialized fields
    [SerializeField]
    private Material material;

    #endregion

    #region Private fields
    private List<LineRenderer> linksRenderer;

    #endregion

    #region Methods - MonoBehaviour callbacks
    private void Start()
    {
        linksRenderer = new List<LineRenderer>();

    }
    #endregion

    #region Methods - Displayer override
    public override void DisplayVisual(SwarmData swarmData)
    {
        ClearVisual();

        List<Tuple<AgentData, AgentData, AgentData>> triangles = SwarmTools.GetDelaunayTriangulation(swarmData);
        
        foreach (Tuple<AgentData, AgentData, AgentData> t in triangles)
        {

            Color lineColor = Color.black;


            //For creating line renderer object
            LineRenderer lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;

            lineRenderer.startWidth = 0.02f; //If you need to change the width of line depending on the distance between both agents :  0.03f*(1-distOnMaxDistance) + 0.005f;
            lineRenderer.endWidth = 0.02f;
            lineRenderer.positionCount = 4;
            lineRenderer.useWorldSpace = true;
            lineRenderer.material = material;
            //lineRenderer.material.SetFloat("_Mode", 2);
            lineRenderer.material.color = lineColor;

            //For drawing line in the world space, provide the x,y,z values
            lineRenderer.SetPosition(0, t.Item1.GetPosition()); //x,y and z position of the starting point of the line
            lineRenderer.SetPosition(1, t.Item2.GetPosition()); //x,y and z position of the end point of the line
            lineRenderer.SetPosition(2, t.Item3.GetPosition()); //x,y and z position of the end point of the line
            lineRenderer.SetPosition(3, t.Item1.GetPosition()); //x,y and z position of the end point of the line


            lineRenderer.receiveShadows = false;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            lineRenderer.transform.parent = this.transform;



            linksRenderer.Add(lineRenderer);
        }
        
    }

    public override void ClearVisual()
    {
        ClearLinksRenderer();
    }
    #endregion

    #region Methods - Other methods
    private void ClearLinksRenderer()
    {
        foreach (LineRenderer l in linksRenderer)
        {
            GameObject.Destroy(l.gameObject);
        }
        linksRenderer.Clear();
    }
    #endregion
}
