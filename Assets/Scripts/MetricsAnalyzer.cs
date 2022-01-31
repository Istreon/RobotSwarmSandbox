using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class MetricsAnalyzer : MonoBehaviour
{
    private float time = 0.0f;
    private float totalTime = 0.0f;

    private string fileName;
    private List<GameObject> agents;
    AgentManager manager;
    ParameterManager parameterManager;


    private List<List<GameObject>> clusters;


    StringBuilder sb = new StringBuilder();

    private Vector3 centerOfMass = Vector3.zero;
    private Vector3 savedCenterOfMassPosition = Vector3.zero;


    // Start is called before the first frame update
    void Start()
    {
        manager = FindObjectOfType<AgentManager>();
        if (manager == null) Debug.LogError("AgentManager is missing in the scene", this);

        parameterManager = FindObjectOfType<ParameterManager>();
        if (parameterManager == null) Debug.LogError("AgentManager is missing in the scene", this);


        fileName = "log_" + System.DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss") + ".csv";

        //Logs
        AddHeaderLogLine();
    }




    // Update is called once per frame
    void FixedUpdate()
    {
        if (agents == null)
        {
            agents = manager.GetAgents();
        }
        UpdateCenterOfMassValue();
        UpdateClusters();

        Debug.Log(EffectiveGroupMotion());


        time += Time.deltaTime;
        totalTime += Time.deltaTime;

        if (time >= 0.5f)
        {
            //Logs
            AddLogLine();
            WriteLogLines();
            time = 0.0f;


            //Update saved position
            foreach (GameObject g in agents)
            {
                g.GetComponent<ReynoldsFlockingAgent>().SavePosition();
            }
            savedCenterOfMassPosition = centerOfMass;
        }

        //Debug.Log(TotalDistance());
        //Debug.Log(DistanceWeightDistributionQuality());
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
        //Debug.Log(BBR());
        //Debug.Log(Order());
        }

    private void AddHeaderLogLine()
    {
        //Time stamp
        string line = "Time stamp";
        //Space
        line += ";TotalDistance";
        line += ";DistanceWeightDistributionQuality";
        line += ";MeanSquareDistanceFromCenterOfMass";
        line += ";BBR";
        line += ";Effective group motion";
        //Cluster
        line += ";ExpectedClusterSize";
        line += ";LargestAggregateSize";
        line += ";LargestAggregateSizeRatio";
        line += ";AggregateNumber";
        line += ";AverageNeighborhood";
        //Speed
        line += ";AverageSpeed";
        line += ";RescaledSpeed(AverageSpeed)";
        line += ";RescaledSpeed(LargestClusterAverageSpeed)";
        //Orientation
        line += ";AverageOrientation";
        line += ";Order";

        //Parameters
        line += ";Cohesion intensity";
        line += ";Alignement intensity";
        line += ";Separation intensity";
        line += ";Random movement intensity";
        line += ";Move forward intensity";
        line += ";Friction intensity";
        line += ";Max speed intensity";

        line += "\r";

        sb.Append(line);
    }

    private void AddLogLine()
    {
        //Time stamp
        string line = totalTime.ToString();

        //Space
        line += ";" + TotalDistance().ToString();
        line += ";" + DistanceWeightDistributionQuality().ToString();
        line += ";" + MeanSquareDistanceFromCenterOfMass().ToString();
        line += ";" + BBR().ToString();
        line += ";" + EffectiveGroupMotion().ToString();
        //Cluster
        line += ";" + ExpectedClusterSize().ToString();
        line += ";" + LargestAggregateSize().ToString();
        line += ";" + LargestAggregateSizeRatio().ToString();
        line += ";" + AggregateNumber().ToString();
        line += ";" + AverageNeighborhood().ToString();
        //Speed
        line += ";" + AverageSpeed().ToString();
        line += ";" + RescaledSpeed(AverageSpeed()).ToString();
        line += ";" + RescaledSpeed(LargestClusterAverageSpeed()).ToString();
        //Orientation
        line += ";" + AverageOrientation().ToString();
        line += ";" + Order().ToString();

        //Parameters
        line += ";" + parameterManager.GetCohesionIntensity().ToString();
        line += ";" + parameterManager.GetAlignmentIntensity().ToString();
        line += ";" + parameterManager.GetSeparationIntensity().ToString();
        line += ";" + parameterManager.GetRandomMovementIntensity().ToString();
        line += ";" + parameterManager.GetMoveForwardIntensity().ToString();
        line += ";" + parameterManager.GetFrictionIntensity().ToString();
        line += ";" + parameterManager.GetMaxSpeed().ToString();

        line += "\r";

        sb.Append(line);
    }

    private void WriteLogLines()
    {
        File.AppendAllText(Application.dataPath + "/Logs/" + fileName, sb.ToString());
        sb.Clear();
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

        while (agentsClone.Count > 0)
        {
            //Add first agent in the new cluster
            List<GameObject> newCluster = new List<GameObject>();
            GameObject firstAgent = agentsClone[0];
            agentsClone.Remove(firstAgent);
            newCluster.Add(firstAgent);

            int i = 0;
            while (i < newCluster.Count)
            {
                List<GameObject> temp = newCluster[i].GetComponent<ReynoldsFlockingAgent>().GetNeighbors();
                foreach (GameObject g in temp)
                {
                    if (!newCluster.Contains(g))
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
        for (i = 0; i < agents.Count; i++)
        {
            for (j = i; j < agents.Count; j++)
            {
                if (i != j) total += Vector3.Distance(agents[i].transform.position, agents[j].transform.position);
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
        float awd = total / (agents.Count * (agents.Count - 1));

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

    private float EffectiveGroupMotion()
    {
        float distCM = Vector3.Distance(centerOfMass, savedCenterOfMassPosition);

        float meanDist = 0.0f;
        foreach (GameObject g in agents)
        {
            Vector3 temp = g.GetComponent<ReynoldsFlockingAgent>().GetSavedPosition();
            meanDist += Vector3.Distance(temp, g.transform.position);
        }

        meanDist /= agents.Count;

        if (meanDist == 0.0f) return 0.0f; //Protect from division by 0
        else return (distCM / meanDist);
    }


    private float MeanSquareDistanceFromCenterOfMass()
    {
        float res = 0;

        foreach (GameObject g in agents)
        {
            float val = Vector3.Distance(g.transform.position, centerOfMass);
            res += val * val;
        }

        res /= agents.Count;

        return res;
    }

    private float AverageSpeed()
    {
        float averageSpeed = 0.0f;
        foreach (GameObject g in agents)
        {
            Vector3 speed = g.GetComponent<ReynoldsFlockingAgent>().GetSpeed();
            averageSpeed += speed.magnitude;
        }

        averageSpeed /= agents.Count;
        return averageSpeed;
    }


    private float LargestClusterAverageSpeed()
    {
        //Get largest Cluster
        int max = 0;
        List<GameObject> largestCluster = new List<GameObject>();
        foreach (List<GameObject> l in clusters)
        {
            if (l.Count > max)
            {
                max = l.Count;
                largestCluster = l;
            }
        }

        if (largestCluster != null || largestCluster.Count != 0)
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
        return (Mathf.Atan2(averageOrientation.z, averageOrientation.x) / Mathf.PI) * 180;
    }


    private float Order()
    {
        int n = agents.Count;

        Vector3 b = Vector3.zero;
        int i;
        for (i = 0; i < n; i++)
        {
            Vector3 speed = agents[i].GetComponent<ReynoldsFlockingAgent>().GetSpeed();
            Vector3 orientation = speed.normalized;
            b += orientation;
        }

        float psi = Vector3.Magnitude(b) / ((float)n);
        return psi;
    }

    private float BBR() //BoundingBoxRatio
    {
        float bbr = 0.0f;
        if(agents.Count>0)
        {
            float xMin = agents[0].transform.position.x;
            float xMax = agents[0].transform.position.x;
            float zMin = agents[0].transform.position.z;
            float zMax = agents[0].transform.position.z;
            int n = agents.Count;

            int i;
            for (i = 0; i < n; i++)
            {
                float xTemp = agents[i].transform.position.x;
                float zTemp = agents[i].transform.position.z;
                if (xTemp > xMax) xMax = xTemp;
                else if (xTemp < xMin) xMin = xTemp;

                if (zTemp > zMax) zMax = zTemp;
                else if (zTemp < zMin) zMin = zTemp;
            }

            bbr = ((xMax - xMin) * (zMax - zMin)) / (manager.GetMapSizeX() * manager.GetMapSizeZ());
        }
        return bbr;
    }


    private float AverageNeighborhood()
    {
        //Create a clone of the agents list, to manipulate it
        List<GameObject> agentsClone = new List<GameObject>(agents);

        float total = 0.0f;
        foreach(GameObject o in agentsClone)
        { 
            List<GameObject> temp = o.GetComponent<ReynoldsFlockingAgent>().GetNeighbors();
            total+=temp.Count;

        }
        total = total / agentsClone.Count;

        return total;
    }


    #endregion
}
