using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class MetricsAnalyzer : MonoBehaviour
{
    [SerializeField]
    private Slider sliderEGM;

    [SerializeField]
    private Slider sliderOrder;

    [SerializeField]
    private Slider sliderDist;

    [SerializeField]
    private Slider sliderTowardCenterOfMass;

    [SerializeField]
    private bool allowDisplayOfSubAttractor = false;

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



    Gradient gradient;
    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;


    public GameObject prefab;
    private List<GameObject> temp = new List<GameObject>();

    [Range(0.05f, 0.3f)]
    public float thresholdDistance = 0.1f;


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


        gradient = new Gradient();

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        colorKey = new GradientColorKey[3];
        colorKey[0].color = Color.green;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.white;
        colorKey[1].time = 0.5f;
        colorKey[2].color = Color.blue;
        colorKey[2].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);
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

        time += Time.deltaTime;
        totalTime += Time.deltaTime;

        sliderEGM.value = EffectiveGroupMotion();
        sliderDist.value = DistanceWeightDistributionQuality();
        sliderOrder.value = Order();
        sliderTowardCenterOfMass.value = TowardsCenterOfMass();
        //Example to change slider color (it only need now to adapt the color depending on the metric value)
        Image temp = sliderTowardCenterOfMass.fillRect.gameObject.GetComponent<Image>();
        temp.color = gradient.Evaluate(sliderTowardCenterOfMass.value);

        //FindSubAttractors();

        if (time >= 0.5f)
        {

            if(allowDisplayOfSubAttractor) FindSubAttractors();
            //Logs
            AddLogLine();
            WriteLogLines();
            time = 0.0f;


            //Update saved position
            foreach (GameObject g in agents)
            {
                g.GetComponent<Agent>().SavePosition();
            }
            savedCenterOfMassPosition = centerOfMass;
        }

        //Debug.Log(NumberOfAgentsIsolated());
        //Debug.Log(TowardsCenterOfMass());
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
                List<GameObject> temp = newCluster[i].GetComponent<Agent>().GetNeighbors();
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

    private float DistanceWeightDistributionQuality() //Il manque la densité
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
            Vector3 temp = g.GetComponent<Agent>().GetSavedPosition();
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
            Vector3 speed = g.GetComponent<Agent>().GetSpeed();
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
                Vector3 speed = g.GetComponent<Agent>().GetSpeed();
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
        float res = speed / agents[0].GetComponent<Agent>().GetMaxSpeed();
        return res;
    }

    private float AverageOrientation()
    {
        Vector3 averageOrientation = Vector3.zero;
        foreach (GameObject g in agents)
        {
            averageOrientation += g.GetComponent<Agent>().GetSpeed();
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
            Vector3 speed = agents[i].GetComponent<Agent>().GetSpeed();
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
            List<GameObject> temp = o.GetComponent<Agent>().GetNeighbors();
            total+=temp.Count;

        }
        total = total / agentsClone.Count;

        return total;
    }

    private int NumberOfAgentsIsolated()
    {
        //Create a clone of the agents list, to manipulate it
        List<GameObject> agentsClone = new List<GameObject>(agents);

        int total = 0;
        foreach (GameObject o in agentsClone)
        {
            List<GameObject> temp = o.GetComponent<Agent>().GetNeighbors();
            if (temp.Count == 0)
            {
                total ++;
            }
        }
        
        return total;
    }


    private float TowardsCenterOfMass()
    {
        int n = agents.Count;

        float b = 0.0f;
        int i;
        for (i = 0; i < n; i++)
        {
            Vector3 speed = agents[i].GetComponent<Agent>().GetSpeed(); 
            Vector3 temp = centerOfMass - agents[i].transform.position;
            float angle = 0.0f;
            if (speed.magnitude == 0.0f)
            {
                angle = 90; //Represent the neutral angle, if the agent isn't moving.
            } else
            {
                angle = Vector3.Angle(speed, temp);
            }

                
            b += angle;
        }
        float res = 1-((b / n)/180);
        return res;
    }

    private void FindSubAttractors()
    {
        List<Vector3> intersectionPoints = GetIntersectionPoints();
        intersectionPoints = RemovingPointOutsideMapBoundaries(intersectionPoints);
        //intersectionPoints = IdentifySpatialPointClusters(intersectionPoints, thresholdDistance);
        MassCentersAndWeights res= MergePoints(intersectionPoints, thresholdDistance);
        intersectionPoints = res.massCenters;

        //(Temporaire) afficher les points pour vérifier que ça fonctionne
        foreach (GameObject t in this.temp)
        {
            Destroy(t);
        }

        temp.Clear();

        int j = 0;
        foreach (Vector3 p in intersectionPoints)
        {
            GameObject t = Instantiate(prefab);
            t.transform.parent = null;
            Vector3 pt = new Vector3(p.x, 0.2f, p.z);

            //Vector3 st = new Vector3(Mathf.Clamp(res.weights[j]* 0.01f, 0.01f , 0.3f), 0.01f, Mathf.Clamp(res.weights[j] * 0.01f, 0.01f, 0.3f));
            Vector3 st = new Vector3(0.05f, 0.01f, 0.05f);

            t.transform.position = pt;
            t.transform.localScale = st;
            temp.Add(t);
            j++;
        }
        //--
    }

    private List<Vector3> GetIntersectionPoints()
    {
        int n = agents.Count;

        List<Vector3> intersectionPoints = new List<Vector3>();

        for (int i = 0; i < n; i++)
        {
            Vector3 speedI = agents[i].GetComponent<Agent>().GetSpeed();
            if (speedI.magnitude == 0.0f) continue; //If the agent doesn't move, go to the next

            Vector3 positionI = agents[i].transform.position;

            //Calculate the coefficients of the equation of the first line
            float a1 = speedI.z / speedI.x;
            float b1 = positionI.z - a1 * positionI.x;
            //--

            for (int j = i + 1; j < n; j++)
            {
                Vector3 speedJ = agents[j].GetComponent<Agent>().GetSpeed();
                if (speedJ.magnitude == 0.0f) continue; //If the agent doesn't move, go to the next

                Vector3 positionJ = agents[j].transform.position;

                //Calculate the coefficients of the equation of the second line
                float a2 = speedJ.z / speedJ.x;
                float b2 = positionJ.z - a2 * positionJ.x;
                //--

                if ((a1 == a2) && (b1 != b2)) continue; //If both line are parallel but distinct, continue

                if (a1 != a2)
                {
                    float x = (b2 - b1) / (a1 - a2);

                    float z = a1 * x + b1;

                    if (float.IsNaN(x) || float.IsNaN(z)) continue;

                    if ((Mathf.Abs(x) > manager.GetMapSizeX()) || (Mathf.Abs(z) > manager.GetMapSizeZ())) continue;

                    Vector3 newIntersection = new Vector3(x, 0.0f, z);

                    if (Vector3.Distance(positionI, newIntersection) > parameterManager.GetFieldOfViewSize()) continue;
                    if (Vector3.Distance(positionI, positionJ) > parameterManager.GetFieldOfViewSize()) continue;

                    Vector3 tempI = newIntersection - positionI;
                    Vector3 tempJ = newIntersection - positionJ;
                    float angleI = Vector3.Angle(speedI, tempI);
                    float angleJ = Vector3.Angle(speedJ, tempJ);

                    if (angleI > 90 || angleJ > 90) continue;

                    intersectionPoints.Add(newIntersection);
                }
            }
        }
        return intersectionPoints;
    }

    private List<Vector3> RemovingPointOutsideMapBoundaries(List<Vector3> positions)
    {
        List<Vector3> clearedPositions = new List<Vector3>();
        foreach(Vector3 p in positions)
        {
            if (p.x > manager.GetMapSizeX() || p.x < 0.0f || p.z > manager.GetMapSizeZ() || p.z < 0.0f) continue;
            clearedPositions.Add(p);
        }
        return clearedPositions;
    }

    private MassCentersAndWeights MergePoints(List<Vector3> points, float threshold)
    {
        //--Part 1, identify neighbourhood
        List<List<int>> allNeighbourhoods = new List<List<int>>();

        for (int i = 0; i < points.Count; i++)
        {
            List<int> neighbourhood = new List<int>();
            for (int j = 0; j < points.Count; j++)
            {
                //Check if we are comparing the same two points
                if (i == j) continue;

                //Check distance between the two points based on a threshold value
                float dist = Vector3.Distance(points[i], points[j]);
                if (dist <= threshold)
                {
                    //That's a neighbour
                    neighbourhood.Add(j);
                }
            }
            //Adding the point i' neighbourhood at the neighbourhood list
            allNeighbourhoods.Add(neighbourhood);
        }


        if (allNeighbourhoods.Count != points.Count) Debug.LogError("Il y a une erreur de la recherche de voisinnage. Il n'y a pas autant de voisinnage que d'agents");

        List<Vector3> massCenters = new List<Vector3>();
        List<int> weights = new List<int>();
        for(int i=0; i<allNeighbourhoods.Count; i++)
        {
            Vector3 massCenter = points[i];
            int weight = allNeighbourhoods[i].Count;
            foreach (int n in allNeighbourhoods[i])
            {
                massCenter += points[n]*allNeighbourhoods[n].Count;
                weight += allNeighbourhoods[n].Count;
            }
            if (weight == 0.0f) continue;
            massCenter /= weight;
            massCenters.Add(massCenter);
            weights.Add(weight);
        }
        MassCentersAndWeights res =  new MassCentersAndWeights();
        res.massCenters = massCenters;
        res.weights = weights;

        return res;


    }

    private List<Vector3> IdentifySpatialPointClusters(List<Vector3> points, float threshold)
    {
        //--Part 1, identify neighbourhood
        List<List<int>> allNeighbourhoods = new List<List<int>>();

        for(int i=0; i<points.Count; i++)
        {
            List<int> neighbourhood = new List<int>();
            for (int j = 0; j < points.Count; j++)
            {
                //Check if we are comparing the same two points
                if (i == j) continue; 

                //Check distance between the two points based on a threshold value
                float dist = Vector3.Distance(points[i], points[j]);
                if(dist<=threshold)
                {
                    //That's a neighbour
                    neighbourhood.Add(j);
                }
            }
            //Adding the point i' neighbourhood at the neighbourhood list
            allNeighbourhoods.Add(neighbourhood);
        }


        if (allNeighbourhoods.Count != points.Count) Debug.LogError("Il y a une erreur de la recherche de voisinnage. Il n'y a pas autant de voisinnage que d'agents");



        //--Part 2, identify clusters
        List<List<int>> pointClusters = new List<List<int>>();

        //Create a clone of the points list, to manipulate it
        List<Vector3> pointsClone = new List<Vector3>(points);

        while (pointsClone.Count > 0)
        {
            //Add first agent in the new cluster
            List<int> newCluster = new List<int>();
            Vector3 firstPoint = pointsClone[0];
            pointsClone.Remove(firstPoint);
            newCluster.Add(points.IndexOf(firstPoint));  

            int i = 0;
            while (i < newCluster.Count)
            {
                List<int> temp = allNeighbourhoods[i];
                foreach (int g in temp)
                {
                    if (!newCluster.Contains(g))
                    {
                        bool res = pointsClone.Remove(points[g]);
                        if (res) newCluster.Add(g);
                    }
                }
                i++;

            }
            pointClusters.Add(newCluster);
        }


        int count = 0;
        foreach(List<int> l in pointClusters)
        {
            count += l.Count;
        }

        if (count != points.Count) Debug.LogError("Erreur dans l'identification des clusters, nb de point attendus: " + points.Count + " et nombre de point obtenus: " + count);


        //--Part 3, identify mass center of clusters
        List<Vector3> clusterMassCenters = new List<Vector3>();

        foreach (List<int> l in pointClusters)
        {
            Vector3 massCenter = new Vector3();
            float total = 0.0f;
            foreach(int v in l)
            {
                massCenter += points[v] * allNeighbourhoods[v].Count;
                total += allNeighbourhoods[v].Count;
            }
            if (total == 0.0f) continue;
            massCenter /= total;
            clusterMassCenters.Add(massCenter);
        }

        return clusterMassCenters;
    }



    #endregion
}


public class MassCentersAndWeights
{
    public List<Vector3> massCenters = new List<Vector3>();
    public List<int> weights = new List<int>();
}
