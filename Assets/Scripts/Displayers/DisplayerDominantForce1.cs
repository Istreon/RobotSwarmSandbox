using System;
using System.Collections.Generic;
using UnityEngine;

public class DisplayerDominantForce1 : Displayer
{
    #region Serialized fields


    [SerializeField]
    private GameObject picto;

    [SerializeField]
    private GameObject pictoIsolated;

    [SerializeField]
    private float pictoHeight = 0.1f;
    #endregion

    #region Private fields
    private List<GameObject> pictos;

    Gradient gradient;
    #endregion

    #region Methods - Monobehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {
        pictos = new List<GameObject>();

        this.gradient = new Gradient();

        // Blend color from red at 0% to blue at 100%
        var colors = new GradientColorKey[3];
        colors[0] = new GradientColorKey(Color.blue, 0.0f);
        colors[1] = new GradientColorKey(Color.white, 0.5f);
        colors[2] = new GradientColorKey(Color.red, 1.0f);

        // Blend alpha from opaque at 0% to transparent at 100%
        var alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphas[1] = new GradientAlphaKey(1.0f, 1.0f);

        gradient.SetKeys(colors, alphas);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion


    #region Methods - Displayer override
    public override void DisplayVisual(SwarmData swarmData)
    {
        ClearVisual();

        List<AgentData> agents =  swarmData.GetAgentsData();
        BehaviourManager.AgentBehaviour agentBehaviour = swarmData.GetParameters().GetAgentBehaviour();
        foreach(AgentData a in agents)
        {
            List<Tuple<BehaviourManager.ForceType, Vector3>> forces =  BehaviourManager.GetForces(a, agentBehaviour);

            List<Vector3> repulsion = new List<Vector3>();
            List<Vector3> attraction = new List<Vector3>();
            List<Vector3> otherForces = new List<Vector3>();

            foreach(Tuple<BehaviourManager.ForceType, Vector3> t in forces)
            {
                switch(t.Item1)
                {
                    case BehaviourManager.ForceType.Attraction:
                        attraction.Add(t.Item2);
                        break;
                    case BehaviourManager.ForceType.Repulsion:
                        repulsion.Add(t.Item2);
                        break;
                    default:
                        otherForces.Add(t.Item2);
                        break;
                }
            }

            float repIntensity = 0.0f;
            Vector3 rep = Vector3.zero;
            float attIntensity = 0.0f;
            Vector3 att = Vector3.zero;            
            float otherForcesIntensity = 0.0f;

            foreach (Vector3 v in repulsion)
            {
                if(v.magnitude > repIntensity)
                {
                    repIntensity = v.magnitude;
                    rep = v;
                }
            }            
            
            foreach (Vector3 v in attraction)
            {
                if(v.magnitude > attIntensity)
                {
                    attIntensity = v.magnitude;
                    att = v;
                }
            }

            Vector3 vecSum = Vector3.zero;
            foreach (Vector3 v in otherForces)
            {
                /* if(v.magnitude > otherForcesIntensity)
                 {
                     otherForcesIntensity = v.magnitude;
                 }*/
                vecSum += v;
            }
            otherForcesIntensity = vecSum.magnitude;


            GameObject g;
            Vector3 dir;

            if (repIntensity == 0.0f && attIntensity == 0.0f)
            {
                g = Instantiate(pictoIsolated);
            }
            else
            {

                float ratio = 1.0f;

                ratio = attIntensity / repIntensity;



                float val = ratio - 0.5f;

                if (val > 1.0f) val = 1.0f;

                if (val < 0.0f) val = 0.0f;
                g = Instantiate(picto);
                //if (aliIntensity > repIntensity && aliIntensity > attIntensity)

                if (otherForcesIntensity > repIntensity && otherForcesIntensity > attIntensity)
                {
                    val = 0.5f;
                }

                g.GetComponentInChildren<Renderer>().material.color = gradient.Evaluate(val);
                /* else
                 {
                     if (repIntensity > attIntensity)
                     {

                         dir = rep;
                     }
                     else
                     {
                         dir = att;
                     }
                 }*/
            }
            dir = a.GetAcceleration();

            // What's the color at the relative time 0.25 (25%) ?
            


            g.transform.parent = this.transform;

            g.transform.localPosition = a.GetPosition() + new Vector3(0.0f, pictoHeight, 0.0f);

            float agentDirection_YAxis = 180 - (Mathf.Acos(dir.normalized.x) * 180.0f / Mathf.PI);
            if (dir.z < 0.0f) agentDirection_YAxis = agentDirection_YAxis * -1;
            g.transform.localRotation = Quaternion.Euler(0.0f, agentDirection_YAxis, 0.0f);

            pictos.Add(g);
            
        }


    }
    public override void ClearVisual()
    {
        foreach(GameObject g in pictos)
        {
            Destroy(g);
        }
        pictos.Clear();
    }
    #endregion
}
