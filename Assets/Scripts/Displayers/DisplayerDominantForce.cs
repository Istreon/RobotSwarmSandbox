using System;
using System.Collections;
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



            GameObject g;
            
            if(repIntensity<attIntensity)
            {
                g = Instantiate(pictoAttraction);
            } else
            {
                g = Instantiate(pictoRepulsion);
            }
            

            g.transform.parent = this.transform;

            g.transform.localPosition = a.GetPosition() + new Vector3(0.0f, pictoHeight, 0.0f);
            g.transform.localRotation = Quaternion.Euler(a.GetDirection());

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
