using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetricsAnalyzer : MonoBehaviour
{
    private float time = 0.0f;
    private List<GameObject> agents;
    AgentManager manager;


    private List<List<GameObject>> clusters;



    private Vector3 centerOfMass = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        manager = FindObjectOfType<AgentManager>();
        if (manager == null) Debug.LogError("AgentManager is missing in the scene", this);
    }

    // Update is called once per frame
    void Update()
    {
        if (agents == null)
        {
            agents = manager.GetAgents();
        }

        time += Time.deltaTime;
        if(time>=1.0f)
        {
            UpdateCenterOfMassValue();
            UpdateClusters();


            //Debug.Log(TotalDistance());
            Debug.Log(DistanceWeightDistributionQuality());
            //Debug.Log(AggregationQuality());
            //Debug.Log(MeanSquareDistanceFromCenterOfMass());
            //Debug.Log(ExpectedClusterSize());
            //Debug.Log(LargestAggregateSize());
            //Debug.Log(LargestAggregateSizeRatio());
            //Debug.Log(AggregateNumber());
            //Debug.Log(AverageSpeed());
            //Debug.Log(LargestClusterAverageSpeed());
            //Debug.Log(RescaledSpeed(AverageSpeed()));
            //Debug.Log(RescaledSpeed(LargestClusterAverageSpeed()));
            //Debug.Log(AverageOrientation());
            time = 0.0f;
        }
    }


    private void UpdateCenterOfMassValue()
    {
        //Calculate the group's center of mass 
        centerOfMass = Vector3.zero;
        foreach (GameObject g in agents)
        {
            centerOfMass += g.transform.position;
        }
        centerOfMass /= agents.Count;
    }

    private void UpdateClusters()
    {
        //Reset clusters list
        clusters = new List<List<GameObject>>();

        //Create a clone of the agents list, to manipulate it
        List<GameObject> agentsClone = new List<GameObject>(agents);

        while (agentsClone.Count>0)
        {
            //Add first agent in the new cluster
            List<GameObject> newCluster = new List<GameObject>();
            GameObject firstAgent = agentsClone[0];
            agentsClone.Remove(firstAgent);
            newCluster.Add(firstAgent);

            int i = 0;
            while (i < newCluster.Count)
            {
                List<GameObject> temp=newCluster[i].GetComponent<ReynoldsFlockingAgent>().GetNeighbors();
                foreach(GameObject g in temp)
                {
                    if(!newCluster.Contains(g))
                    {
                        bool res = agentsClone.Remove(g);
                        if (res) newCluster.Add(g);
                    }
                }
                i++;

            }
            clusters.Add(newCluster);
        }
    }


    #region Metric Methods
    private float ExpectedClusterSize()
    {
        /*
        int count = 0;
        //Browse all clusters
        foreach (List<GameObject> l in clusters)
        {
            count += l.Count;
        }
        */
        //adding the number of agents in each cluster is equivalent to obtaining the total number of agents
        //we can thus simplify in
        int count = agents.Count;
        count /= clusters.Count;
        return count;
    }

    private float LargestAggregateSize()
    {
        int max = 0;
        foreach (List<GameObject> l in clusters)
        {
            if (l.Count > max) max = l.Count;
        }
        return max;
    }

    private float LargestAggregateSizeRatio()
    {
        float res = LargestAggregateSize() / agents.Count;
        return res;
    }

    private float AggregateNumber()
    {
        return clusters.Count;
    }

    private float TotalDistance()
    {
        float total = 0;
        int i, j;
        for(i=0; i<agents.Count; i++)
        {
            for (j = i; j < agents.Count; j++)
            {
                if(i!=j) total += Vector3.Distance(agents[i].transform.position, agents[j].transform.position);
            }
        }
        return total;
    }

    private float DistanceWeightDistributionQuality()
    {
        float total = 0;
        int i, j;
        for (i = 0; i < agents.Count; i++)
        {
            for (j = 0; j < agents.Count; j++)
            {
                if (i != j) total += Vector3.Distance(agents[i].transform.position, agents[j].transform.position);
            }
        }
        float awd = total/ (agents.Count * (agents.Count - 1));

        float t = awd + 1.0f;
        float fw = 1.0f - (1.0f / Mathf.Sqrt(t));

        return fw;
    }

    private float AggregationQuality()
    {
        float res = 0;

        foreach (GameObject g in agents)
        {
            res += Vector3.Distance(g.transform.position, centerOfMass);

            //Il y a quelque chose en plus dans l'article
        }

        res /= agents.Count;

        return res;
    }


    private float MeanSquareDistanceFromCenterOfMass()
    {
        float res = 0;

        foreach (GameObject g in agents)
        {
            float val = Vector3.Distance(g.transform.position, centerOfMass);
            res += val*val;
        }

        res /= agents.Count;

        return res;
    }

    private float AverageSpeed()
    {
        float averageSpeed = 0.0f;
        foreach(GameObject g in agents)
        {
           Vector3 speed= g.GetComponent<ReynoldsFlockingAgent>().GetSpeed();
            averageSpeed += speed.magnitude;
        }

        averageSpeed /= agents.Count;
        return averageSpeed;
    }


    private float LargestClusterAverageSpeed()
    {
        //Get largest Cluster
        int max = 0;
        List<GameObject> largestCluster=new List<GameObject>();
        foreach (List<GameObject> l in clusters)
        {
            if (l.Count > max)
            {
                max = l.Count;
                largestCluster = l;
            }
        }

        if(largestCluster!=null || largestCluster.Count!=0)
        {
            //Get average speed of the largest cluster
            float averageSpeed = 0.0f;
            foreach (GameObject g in largestCluster)
            {
                Vector3 speed = g.GetComponent<ReynoldsFlockingAgent>().GetSpeed();
                averageSpeed += speed.magnitude;
            }

            averageSpeed /= largestCluster.Count;
            return averageSpeed;
        } else
        {
            Debug.LogError("Il n'y a pas de cluster, cela peut arriver si il n'y a pas d'agent", this);
            return 0.0f;
        }
    }



    private float RescaledSpeed(float speed)
    {
        float res = speed / agents[0].GetComponent<ReynoldsFlockingAgent>().GetMaxSpeed();
        return res;
    }

    private float AverageOrientation()
    {
        Vector3 averageOrientation = Vector3.zero;
        foreach (GameObject g in agents)
        {
            averageOrientation += g.GetComponent<ReynoldsFlockingAgent>().GetSpeed();
        }
        return (Mathf.Atan2(averageOrientation.z, averageOrientation.x)/Mathf.PI)*180;
    }


    private float Order()
    {
        int n = agents.Count;

        int i, j;
        for(i=0; i<n; i++)
        {
            for(j=0; j<n; j++)
            {
                Vector3 speedi=agents[i].GetComponent<ReynoldsFlockingAgent>().GetSpeed();
                Vector3 speedj=agents[j].GetComponent<ReynoldsFlockingAgent>().GetSpeed();
                
            }
        }
        return 0.0f;
    }
    #endregion
}
