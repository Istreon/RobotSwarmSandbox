using System;
using System.Collections.Generic;
using UnityEngine;

public class DisplayerDominantForce : Displayer
{
    #region Serialized fields
    [SerializeField]
    private GameObject pictoAttraction;

    [SerializeField]
    private GameObject pictoRepulsion;

    [SerializeField]
    [Range(0.0f,0.5f)]
    private float toleranceThreshold = 0.0f;    
    
    [SerializeField]
    private float pictoHeight = 0.1f;
    #endregion

    #region Private fields
    private List<GameObject> pictos;
    #endregion

    #region Methods - Monobehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {
        pictos = new List<GameObject>();
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
                }
            }

            float repIntensity = 0.0f;
            Vector3 rep = Vector3.zero;
            float attIntensity = 0.0f;
            Vector3 att = Vector3.zero;

            
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


            if (repIntensity == 0.0f && attIntensity == 0.0f) continue;

            float ratio = repIntensity / attIntensity;

            if (ratio < 1.0f + toleranceThreshold && ratio > 1.0f - toleranceThreshold) continue;

            GameObject g;

            Vector3 dir;
            if(repIntensity<attIntensity)
            {
                g = Instantiate(pictoAttraction);
                dir = att;
            } else
            {
                g = Instantiate(pictoRepulsion);
                dir = rep;
            }
            

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
