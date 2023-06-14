using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExternalEnvelope : Displayer
{
    #region Serialized fields
    [SerializeField]
    private Material material;
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

    #region Methods - Displayer override
    public override void DisplayVisual(LogClipFrame frame)
    {
        ClearVisual();

        List<List<Vector3>> convexHuls = GetConvexHul(frame);

        foreach(List<Vector3> pile in convexHuls)
        {

                //For creating line renderer object
                LineRenderer lineRenderer = new GameObject("Line").AddComponent<LineRenderer>();
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.red;

                lineRenderer.startWidth = 0.01f; //If you need to change the width of line depending on the distance between both agents :  0.03f*(1-distOnMaxDistance) + 0.005f;
                lineRenderer.endWidth = 0.01f;
                lineRenderer.positionCount = pile.Count+1;
                lineRenderer.useWorldSpace = true;
                lineRenderer.material = material;
                //lineRenderer.material.SetFloat("_Mode", 2);
                lineRenderer.material.color = Color.red;

                for (int i = 0; i < pile.Count; i++)
                {
                    //For drawing line in the world space, provide the x,y,z values
                    lineRenderer.SetPosition(i, pile[i]); //x,y and z position of the starting point of the line
                }
                lineRenderer.SetPosition(pile.Count, pile[0]);
                lineRenderer.transform.parent = this.transform;
                visualRenderer.Add(lineRenderer);
            
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

    #region Methods - Convex hul
    private List<List<Vector3>> GetConvexHul(LogClipFrame frame)
    {
        List<List<Vector3>> convexHuls = new List<List<Vector3>>();
        List<List<LogAgentData>> clusters = FrameTools.GetClusters(frame);

        foreach (List<LogAgentData> c in clusters)
        {
            if (c.Count < 3) continue;
            List<Vector3> positions = new List<Vector3>();

            foreach (LogAgentData g in c)
            {
                positions.Add(g.getPosition());
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

            for (int i = 2; i < positions.Count; i++)
            {
                while ((pile.Count >= 2) && VectorialProduct(pile[pile.Count - 2], pile[pile.Count - 1], positions[i]) <= 0 || pile[pile.Count - 1] == positions[i])
                {
                    pile.RemoveAt(pile.Count - 1);
                }
                pile.Add(positions[i]);
            }

            convexHuls.Add(pile);
        }
        return convexHuls;
    }



    private float VectorialProduct(Vector3 a, Vector3 b, Vector3 c)
    {
        return ((b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z));
    }
    #endregion
}
