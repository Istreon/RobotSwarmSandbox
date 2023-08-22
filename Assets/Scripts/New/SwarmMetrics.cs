using System.Collections.Generic;
using UnityEngine;

public class SwarmMetrics
{
    #region Methods - Clusters
    public static float ExpectedClusterSize(SwarmData swarmData)
    {
        List<List<AgentData>> clusters = SwarmTools.GetClusters(swarmData);

        int count = swarmData.GetAgentsData().Count;
        count /= clusters.Count;
        return count;
    }

    public static int LargestClusterSize(SwarmData swarmData)
    {
        List<List<AgentData>> clusters = SwarmTools.GetOrderedClusters(swarmData);
        if (clusters.Count > 0)
            return clusters[0].Count;
        else
            return 0;
    }

    public static float LargestClusterSizeRatio(SwarmData swarmData)
    {
        float res = (float) LargestClusterSize(swarmData) / swarmData.GetAgentsData().Count;
        return res;
    }

    public static int ClusterNumber(SwarmData swarmData)
    {
        List<List<AgentData>> clusters = SwarmTools.GetOrderedClusters(swarmData);
        return clusters.Count;
    }
    #endregion

    #region Methods - Swarm motion
    public static float EffectiveGroupMotion(SwarmData swarmData, SwarmData pastSwarmData)
    {
        Vector3 currentCenterOfMass = SwarmTools.GetCenterOfMass(swarmData);
        Vector3 pastCenterOfMass = SwarmTools.GetCenterOfMass(pastSwarmData);


        float distCM = Vector3.Distance(currentCenterOfMass, pastCenterOfMass);

        float meanDist = 0.0f;

        List<AgentData> currentAgents = swarmData.GetAgentsData();
        List<AgentData> pastAgents = pastSwarmData.GetAgentsData();
        for(int i=0; i<currentAgents.Count; i++)
        {
            meanDist += Vector3.Distance(pastAgents[i].GetPosition(), currentAgents[i].GetPosition());
        }

        meanDist /= currentAgents.Count;

        if (meanDist == 0.0f) return 0.0f; //Protect from division by 0
        else return (distCM / meanDist);
    }
    #endregion

    #region Methods - Swarm distance
    public static float TotalDistance(SwarmData swarmData)
    {
        List<AgentData> agents = swarmData.GetAgentsData();
        float total = 0;
        int i, j;
        for (i = 0; i < agents.Count; i++)
        {
            for (j = i; j < agents.Count; j++)
            {
                if (i != j) total += Vector3.Distance(agents[i].GetPosition(), agents[j].GetPosition());
            }
        }
        return total;
    }
    public static float MeanSquareDistanceFromCenterOfMass(SwarmData swarm)
    {
        float res = 0;
        Vector3 centerOfMass = SwarmTools.GetCenterOfMass(swarm);

        List<AgentData> agents = swarm.GetAgentsData();

        foreach (AgentData a in agents)
        {
            float val = Vector3.Distance(a.GetPosition(), centerOfMass);
            res += val * val;
        }

        res /= agents.Count;

        return res;
    }

    public static float BBR(SwarmData swarmData) //BoundingBoxRatio
    {
        List<AgentData> agents = swarmData.GetAgentsData();
        float bbr = 0.0f;

        if (agents.Count > 0)
        {
            float xMin = agents[0].GetPosition().x;
            float xMax = agents[0].GetPosition().x;
            float zMin = agents[0].GetPosition().z;
            float zMax = agents[0].GetPosition().z;
            int n = agents.Count;

            int i;
            for (i = 0; i < n; i++)
            {
                float xTemp = agents[i].GetPosition().x;
                float zTemp = agents[i].GetPosition().z;
                if (xTemp > xMax) xMax = xTemp;
                else if (xTemp < xMin) xMin = xTemp;

                if (zTemp > zMax) zMax = zTemp;
                else if (zTemp < zMin) zMin = zTemp;
            }

            bbr = ((xMax - xMin) * (zMax - zMin)) / (swarmData.GetParameters().GetMapSizeX() * swarmData.GetParameters().GetMapSizeZ());
        }
        return bbr;
    }
    #endregion

    #region Methods - Swarm speed

    public static float AverageSpeed(SwarmData swarmData)
    {
        List<AgentData> agents = swarmData.GetAgentsData();
        float averageSpeed = 0.0f;
        foreach (AgentData a in agents)
        {
            Vector3 speed = a.GetSpeed();
            averageSpeed += speed.magnitude;
        }

        averageSpeed /= agents.Count;
        return averageSpeed;
    }

    public static float LargestClusterAverageSpeed(SwarmData swarmData)
    {
        List<List<AgentData>> clusters = SwarmTools.GetOrderedClusters(swarmData);

        if(clusters.Count>0)
        {
            List<AgentData> largestCluster = clusters[0];
            //Get average speed of the largest cluster
            float averageSpeed = 0.0f;
            foreach (AgentData a in largestCluster)
            {
                Vector3 speed = a.GetSpeed();
                averageSpeed += speed.magnitude;
            }

            averageSpeed /= largestCluster.Count;
            return averageSpeed;
        } else
        {
            return -1;
        }
    }

    public static float RescaledSpeed(SwarmData swarmData, float speed)
    {
        float res = speed / swarmData.GetParameters().GetMaxSpeed();
        return res;
    }

    #endregion

    #region Methods - Neighbourhood
    public static float AverageNeighbourhood(SwarmData swarmData)
    {
        //Create a clone of the agents list, to manipulate it
        List<AgentData> agents = swarmData.GetAgentsData();

        float total = 0.0f;
        foreach (AgentData a in agents)
        {
            List<AgentData> temp = SwarmTools.GetNeighbours(a,agents,swarmData.GetParameters().GetFieldOfViewSize(), swarmData.GetParameters().GetBlindSpotSize());
            total += temp.Count;

        }
        total = total / agents.Count;

        return total;
    }
    #endregion

    #region Methods - Swarm direction

    public static float TowardsCenterOfMass(SwarmData swarmData)
    {
        List<AgentData> agents = swarmData.GetAgentsData();
        int n = agents.Count;

        Vector3 centerOfMass = SwarmTools.GetCenterOfMass(swarmData);
        float b = 0.0f;
        
        foreach (AgentData a in agents)
        {
            Vector3 speed = a.GetSpeed();
            Vector3 temp = centerOfMass - a.GetPosition();
            float angle = 0.0f;
            if (speed.magnitude == 0.0f)
            {
                angle = 90; //Represent the neutral angle, if the agent isn't moving.
            }
            else
            {
                angle = Vector3.Angle(speed, temp);
            }


            b += angle;
        }
        float res = 1 - ((b / n) / 180);
        return res;
    }

    public static float Order(SwarmData swarmData)
    {
        List<AgentData> agents = swarmData.GetAgentsData();
        int n = agents.Count;

        Vector3 b = Vector3.zero;
        foreach (AgentData a in agents)
        {
            Vector3 speed = a.GetSpeed();
            Vector3 direction = speed.normalized;
            b += direction;
        }

        float psi = Vector3.Magnitude(b) / ((float)n);
        return psi;
    }

    public static float AverageSpeedDirection(SwarmData swarmData)
    {
        Vector3 averageOrientation = Vector3.zero;
        foreach (AgentData a in swarmData.GetAgentsData())
        {
            averageOrientation += a.GetSpeed();
        }
        return (Mathf.Atan2(averageOrientation.z, averageOrientation.x) / Mathf.PI) * 180;
    }
    #endregion

    #region Methods - Distance intra cluster
    public static float MeanKNNDistanceBiggerCluster(SwarmData swarmData, int k)
    {
        List<List<AgentData>> clusters = SwarmTools.GetOrderedClusters(swarmData);

        float dist = MeanKNNDistance(clusters[0], k);

        return dist;
    }

    public static float MeanKNNDistance(List<AgentData> cluster, int k)
    {
        float meanDist = 0.0f;
        List<float> distances = new List<float>();

        foreach (AgentData g in cluster)
        {
            distances.AddRange(GetKNNDistances(cluster, g, k));
        }

        foreach (float d in distances)
        {
            meanDist += d;
        }

        meanDist /= distances.Count;

        return meanDist;
    }

    /// <summary>
    /// Calculate the k nearest distances from the agent set in parameter, to the other agents from the list set in paramter.
    /// </summary>
    /// <param name="cluster"> The set of agents from which the k nearest distances of the agent set in parameter will be calculated. </param>
    /// <param name="agent"> The agent reference to calculate the distances.</param>
    /// <param name="k">The maximum number of distances returned. </param>
    /// <returns>The k nearest distances to other agents, possibly less if there is not enough other agents.</returns>
    private static List<float> GetKNNDistances(List<AgentData> cluster, AgentData agent, int k)
    {
        //Compute every distance from parameter agent to other agents
        List<float> distances = new List<float>();

        //Compare current agent with all agents
        foreach (AgentData g in cluster)
        {
            //Check if the current agent is compared with itself
            if (System.Object.ReferenceEquals(g, agent)) continue;

            //Compute distance
            float dist = Vector3.Distance(g.GetPosition(), agent.GetPosition());

            distances.Add(dist);
        }


        //Sort list
        distances.Sort(new GFG());


        //Get the knn
        List<float> knnDistances = new List<float>();

        if (distances.Count < k) k = distances.Count;
        for (int i = 0; i < k; i++)
        {
            knnDistances.Add(distances[i]);
        }

        return knnDistances;
    }

    #endregion

    #region Methods - Distance inter cluster


    public static float GetSignificantDistanceBetweenClusters(SwarmData swarmData)
    {
        List<List<AgentData>> clusters = SwarmTools.GetOrderedClusters(swarmData);

        //If there is not enough cluster, exit
        if (clusters.Count < 2) return -1;

        float significantDistance = -1;

        List<List<List<AgentData>>> superList = new List<List<List<AgentData>>>();
        //Creer des superlist des cluster
        foreach (List<AgentData> c in clusters)
        {
            List<List<AgentData>> temp = new List<List<AgentData>>();
            temp.Add(c);
            superList.Add(temp);
        }

        //Tant que le nombre de superlist est supérieur à 2
        while (superList.Count > 2)
        {
            //Calculer la distance plus petite entre deux clusters
            float minDist = float.MaxValue;
            List<List<AgentData>> minCs1 = null;
            List<List<AgentData>> minCs2 = null;

            for (int i = 0; i < superList.Count; i++)
            {
                List<List<AgentData>> cs1 = superList[i];
                for (int j = i; j < superList.Count; j++)
                {
                    if (i == j) continue;

                    List<List<AgentData>> cs2 = superList[j];

                    float dist = MinDistanceBetweenTwoClusterSets(cs1, cs2);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        minCs1 = cs1;
                        minCs2 = cs2;
                    }
                }
            }

            //Fusionner les superlists des clusters concernés
            minCs1.AddRange(minCs2);
            //Supprimer de la liste le cluster fusionné
            superList.Remove(minCs2);
        }

        //Normalement ici, il ne reste que deux clusters
        //Calculer la min dist entre les deux
        significantDistance = MinDistanceBetweenTwoClusterSets(superList[0], superList[1]);
        //la retourner


        return significantDistance;
    }

    private static float MinDistanceBetweenTwoClusterSets(List<List<AgentData>> clusterSet1, List<List<AgentData>> clusterSet2)
    {
        float minDist = float.MaxValue;
        foreach (List<AgentData> c1 in clusterSet1)
        {
            foreach (List<AgentData> c2 in clusterSet2)
            {
                float dist = MinDistanceBetweenTwoClusters(c1, c2);
                if (dist < minDist)
                    minDist = dist;
            }
        }
        return minDist;
    }


    /// <summary>
    /// Compute the min distance between two cluster, and return it.
    /// </summary>
    /// <param name="cluster1">The first cluster</param>
    /// <param name="cluster2">The other cluster</param>
    /// <returns>The min distance between the two clusters set in parameter.</returns>
    private static float MinDistanceBetweenTwoClusters(List<AgentData> cluster1, List<AgentData> cluster2)
    {
        float minDist = float.MaxValue;

        foreach (AgentData l1 in cluster1)
        {
            foreach (AgentData l2 in cluster2)
            {
                float dist = Vector3.Distance(l1.GetPosition(), l2.GetPosition());
                if (dist < minDist)
                    minDist = dist;
            }
        }
        return minDist;
    }
    #endregion

    #region Methods - Others

    private float StandardDeviation(List<float> l)
    {
        //Calcul de la moyenne
        float mean = 0.0f;

        foreach (float v in l)
        {
            mean += v;
        }

        mean /= l.Count;

        float variance = 0.0f;
        foreach (float v in l)
        {
            float val = (v - mean);
            val = Mathf.Pow(val, 2);
            variance += val;
        }

        variance /= (l.Count - 1);

        float standardDeviation = Mathf.Sqrt(variance);
        return standardDeviation;
    }


    private float Median(List<float> l)
    {
        float median;
        l.Sort(new GFG());

        int n = l.Count;
        if (n % 2 != 0)
        {
            n = n + 1;
            median = l[n / 2 - 1];
        }
        else
        {
            median = (l[n / 2 - 1] + l[n / 2]) / 2;
        }


        return median;
    }

    //Class allowing to sort float in a list using : list.Sort(new GFG());  
    class GFG : IComparer<float>
    {
        public int Compare(float x, float y)
        {
            if (x == 0 || y == 0)
            {
                return 0;
            }

            // CompareTo() method
            return x.CompareTo(y);
        }
    }
    #endregion
}
