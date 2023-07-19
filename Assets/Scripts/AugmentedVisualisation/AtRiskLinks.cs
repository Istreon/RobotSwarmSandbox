using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtRiskLinks : Displayer
{
    #region Serialized fields

    [SerializeField]
    [Range(1,39)]
    private int nbLinks = 3; //Number of links expected

    [SerializeField]
    [Range(0.01f,0.08f)]
    private float width = 0.03f;

    [SerializeField]
    private Material material;
    #endregion

    #region Private fields
    private Mesh mesh;
    #endregion

    #region Methods - MonoBehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {
        //Create a mesh filter (need an upgrade in case of already axisting meshfilter)
        MeshFilter meshFilter = this.gameObject.AddComponent<MeshFilter>();
        this.mesh = new Mesh();
        meshFilter.mesh = mesh;

        //Create a mesh renderer (need an upgrade in case of already axisting meshrenderer)
        MeshRenderer meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion

    #region Methods - Displayer override
    public override void DisplayVisual(LogClipFrame frame)
    {
        ClearVisual();

        List<Tuple<LogAgentData, LogAgentData>> res = new List<Tuple<LogAgentData, LogAgentData>>();

        //Get the clusters in the swarm
        List<List<LogAgentData>> clusters = FrameTools.GetOrderedClusters(frame);

        foreach (List<LogAgentData> cluster in clusters)
        {
            if (cluster.Count < 2) continue;

            List<Tuple<LogAgentData, LogAgentData>> links = FrameTools.GetLinksList(cluster, frame.GetParameters().GetFieldOfViewSize(), frame.GetParameters().GetBlindSpotSize());




            List<List<LogAgentData>> groups = new List<List<LogAgentData>>();

            foreach (LogAgentData a in cluster)
            {
                List<LogAgentData> group = new List<LogAgentData>();
                group.Add(a);
                groups.Add(group);
            }



            while (groups.Count > 1)
            {
                //Get the closest duo in the links list
                Tuple<LogAgentData, LogAgentData> closestDuo = null;
                float minDist = float.MaxValue;
                foreach (Tuple<LogAgentData, LogAgentData> t in links)
                {
                    float dist = Vector3.Distance(t.Item1.getPosition(), t.Item2.getPosition());
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestDuo = t;
                    }
                }

                if (groups.Count <= (nbLinks + 1)) res.Add(closestDuo);

                //Merge the closest duo
                List<LogAgentData> group1 = null;
                List<LogAgentData> group2 = null;
                foreach (List<LogAgentData> g in groups)
                {
                    if (g.Contains(closestDuo.Item1)) group1 = g;
                    if (g.Contains(closestDuo.Item2)) group2 = g;
                }

                if (!System.Object.ReferenceEquals(group1, group2))
                {
                    group1.AddRange(group2);
                    groups.Remove(group2);
                }

                //Remove all the links inter group
                List<Tuple<LogAgentData, LogAgentData>> linkToRemove = new List<Tuple<LogAgentData, LogAgentData>>();
                foreach (Tuple<LogAgentData, LogAgentData> t in links)
                {
                    foreach (List<LogAgentData> g in groups)
                    {
                        if (g.Contains(t.Item1) && g.Contains(t.Item2))
                        {
                            linkToRemove.Add(t);
                            break;
                        }
                    }
                }

                foreach (Tuple<LogAgentData, LogAgentData> t in linkToRemove)
                {
                    links.Remove(t);
                }
            }
        }
        //Display the remaining links
        //Initialise lists of vertices and triangles for the mesh
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        foreach (Tuple<LogAgentData, LogAgentData> l in res)
        {
            //Get the vertices of the line
            List<Vector3> v = MeshTools.TranformLineToRectanglePoints(l.Item1.getPosition(), l.Item2.getPosition(), width);
            //Get the triangles from the vertices of the line
            List<int> t = MeshTools.DrawFilledTriangles(v.ToArray());

            //Update triangles indexes before adding them to the triangles list
            for (int k = 0; k < t.Count; k++)
            {
                t[k] += vertices.Count;
            }

            //Updathe vertices and triangles list with the new line
            vertices.AddRange(v);
            triangles.AddRange(t);
        }

        Vector2[] uv = new Vector2[vertices.Count];

        mesh.vertices = vertices.ToArray();
        mesh.uv = uv;
        mesh.triangles = triangles.ToArray();


    }

    public override void ClearVisual()
    {
        mesh.Clear();
    }
    #endregion

}
