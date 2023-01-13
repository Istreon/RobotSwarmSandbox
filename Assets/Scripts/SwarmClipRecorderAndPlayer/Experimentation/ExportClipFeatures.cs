using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ExportClipFeatures : MonoBehaviour
{
    string filePath = "/Clips/"; //The folder containing clip files


    string[] fileNames = {
        "F_1.dat",
        "F_2.dat",
        "F_3.dat",
        "F_4.dat",
        "F_5.dat",
        "F_6.dat",
        "F_7.dat",
        "F_8.dat",
        "F_9.dat",
        "F_10.dat",
        "F_11.dat",
        "F_12.dat",
        "F_13.dat",
        "F_14.dat",
        "F_15.dat",
        "F_16.dat",
        "F_17.dat",
        "F_18.dat",
        "F_19.dat",
        "F_20.dat",
        "F_21.dat",
        "F_22.dat",
        "F_23.dat",
        "F_24.dat",
        "F_25.dat",
        "F_26.dat",
        "F_27.dat",
        "F_28.dat",
        "SF_1.dat",
        "SF_2.dat",
        "SF_3.dat",
        "SF_4.dat",
        "SF_5.dat",
        "SF_6.dat",
        "SF_7.dat",
        "SF_8.dat",
        "SF_9.dat",
        "SF_10.dat",
        "SF_11.dat",
        "SF_12.dat",
        "SF_13.dat",
        "SF_14.dat",
        "SF_15.dat",
        "SF_16.dat",
        "SF_17.dat",
        "SF_18.dat",
        "SF_19.dat",
        "SF_20.dat",
        "SF_21.dat",
        "SF_22.dat",
        "SF_23.dat",
        "SF_24.dat",
        "SF_25.dat",
        "SF_26.dat",
        "SF_27.dat",
        "SF_28.dat",
        "SF_29.dat",
        "SF_30.dat",
    };


    int[] fractureFrames =
    {
        2080,
        60,
        1311,
        190,
        377,
        281,
        638,
        122,
        957,
        399,
        276,
        760,
        797,
        99,
        652,
        663,
        868,
        944,
        2169,
        2146,
        400,
        277,
        969,
        757,
        1148,
        648,
        708,
        339
    };

    private List<LogClip> clips = new List<LogClip>();

    private int currentClip = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (fractureFrames.Length != fileNames.Length) Debug.Log("Nonnonononono");
        filePath = Application.dataPath + filePath;
        Debug.Log(filePath);

        string resultFilePathCSV = Application.dataPath + "/Results/clip_features.csv";

        StringBuilder sb = new StringBuilder();

        //Prepare csv result file
        string line = "Filename;FPS;NbFrames;FractureFrame;DistMedianKNN;SignificantDist;FractureScore;BestScore;ExpansionScore;MeanTowardsCenterOfMass;MedianTowardsCenterOfMass;MeanEffectiveGroupMotion;MedianEffectiveGroupMotion;MedianOrder\r";
        sb.Append(line);

        //Load all clip
        for (int i = 0; i < fileNames.Length; i++)
        {
            string fname = fileNames[i];

            //Concatenation of file path and file name
            string s = this.filePath + fname;

            //Loading clip from full file path
            LogClip clip = ClipTools.LoadClip(s);

            if (clip != null)
            {
                clips.Add(clip); //Add the loaded clip to the list
                Debug.Log("Clip " + s + " Loaded.");
            }
            else
                Debug.LogError("Clip can't be load from " + s, this);
        }

        Debug.Log("Clips are loaded.");


      
        //Analyse part
        foreach (LogClip c in clips)
        {
            //int fractureFrame = GetFractureFrame(c);
            //int fractureFrame = fractureFrames[currentClip];
            int otherFractureFrame = GetFractureFrame(c);
            float knnMedian = -1;
            float significantDistance = -1;
            float score = -1;
            float best = -1;
        
        if (otherFractureFrame !=-1)
            {
                knnMedian = KNNDistanceMedianAt(c, otherFractureFrame, 3);
                significantDistance = SignificantDistanceBetweenClustersAtFrame(c, otherFractureFrame);
                score = significantDistance / knnMedian;
                best = BestFractureVisibilityScore(c, otherFractureFrame);
            }
            
            line = fileNames[currentClip] + ";" + c.getFps() + ";" + c.getClipFrames().Count + ";" /*+ fractureFrame + ";"*/ + GetFractureFrame(c) + ";" + knnMedian + ";" +  significantDistance + ";" + score + ";" + best +  ";" + ExpansionScore(c) + ";" + MeanTowardsCenterOfMass(c) + ";" + MedianTowardsCenterOfMass(c) + ";" + MeanEffectiveGroupMotion(c) + ";" + MedianEffectiveGroupMotion(c) + ";" + MedianOrder( c) + "\r";
            sb.Append(line);
         
            currentClip++;
            
        }


        //Save result
        File.AppendAllText(resultFilePathCSV, sb.ToString());
        sb.Clear();

        Debug.Log("Results saved.");
    }

    
    private int GetFractureFrame(LogClip c)
    {
        int res = -1;
        int i = 0;

        int validationTime = 120;
        int fractureDuration = 0;
        foreach (LogClipFrame f in c.getClipFrames())
        {
            List<List<LogAgentData>> clusters = ClipTools.GetClusters(f);
            if (clusters.Count > 1)
            {
                if(fractureDuration == 0)
                    res = i;

                fractureDuration++;

                if(fractureDuration > validationTime)
                    break;
            } else
            {
                fractureDuration = 0;
                res = -1;
            }
            i++;
        }
        return res;
    }

    private Vector3 CenterOfMass(List<LogAgentData> agents)
    {
        Vector3 centerOfMass = Vector3.zero;
        foreach (LogAgentData a in agents)
        {
            centerOfMass += a.getPosition();
        }
        centerOfMass /= agents.Count;
        return centerOfMass;
    }


    #region Methods - Expansion and total distance
    private float TotalDistanceExpansion(LogClip c)
    {
        float start = TotalDistance(c.getClipFrames()[0]);
        float end = TotalDistance(c.getClipFrames()[c.getClipFrames().Count - 1]);

        return end / start;
    }

    private float TotalDistance(LogClipFrame f)
    {
        List<LogAgentData> agents = f.getAgentData();
        float total = 0;
        int i, j;
        for (i = 0; i < agents.Count; i++)
        {
            for (j = i; j < agents.Count; j++)
            {
                if (i != j) total += Vector3.Distance(agents[i].getPosition(), agents[j].getPosition());
            }
        }
        return total;
    }

    private float ExpansionScore(LogClip c)
    {
        float startValue = KNNDistanceMedianAt(c, 0, 3);
        float endValue = KNNDistanceMedianAt(c, c.getClipFrames().Count - 1, 3);

        float res = (endValue / startValue);

        return res;
    }
    #endregion

    #region Methods - Towards center of mass
    private float MeanTowardsCenterOfMass(LogClip c)
    {
        float res = 0.0f;
        int i = 0;
        foreach(LogClipFrame f in c.getClipFrames())
        {
            if (ClipTools.GetClusters(f).Count > 1) break;

            res += TowardsCenterOfMass(f);
            i++;
        }
        res /= i;

        return res;
    }

    private float MedianTowardsCenterOfMass(LogClip c)
    {
        float median = 0.0f;
        List<float> l = new List<float>();
        foreach (LogClipFrame f in c.getClipFrames())
        {
            if (ClipTools.GetClusters(f).Count > 1) break;

            float val= TowardsCenterOfMass(f);
            l.Add(val);
        }
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


    private float TowardsCenterOfMass(LogClipFrame f)
    {
        List<LogAgentData> agents = f.getAgentData();
        int n = agents.Count;

        Vector3 centerOfMass = CenterOfMass(agents);
        float b = 0.0f;
        int i;
        for (i = 0; i < n; i++)
        {
            Vector3 speed = agents[i].getSpeed();
            Vector3 temp = centerOfMass - agents[i].getPosition();
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
    #endregion

    #region Methods - Fracture number
    private float RatioFractureNumberSize(LogClip c)
    {
        return ((float) MaxFractureNumber(c) / (float)MainSwarmSizeAtMaxFractureNumber(c));
    }

    private int MaxFractureNumber(LogClip c)
    {
        int max = 0;
        foreach(LogClipFrame f in c.getClipFrames())
        {
            int val = ClipTools.GetClusters(f).Count;
            if (max < val) max = val;
        }

        return max;
    }

    private int MainSwarmSizeAtMaxFractureNumber(LogClip c)
    {
        int max = 0;
        int frame = 0;
        int i = 0;
        foreach (LogClipFrame f in c.getClipFrames())
        {
            int val = ClipTools.GetClusters(f).Count;
            if (max < val)
            {
                max = val;
                frame = i;
            }
            i++;
        }

        List<LogAgentData> mainCluster =ClipTools.GetOrderedClusters(c.getClipFrames()[frame])[0];

        return mainCluster.Count;
    }
    #endregion
    
   private float MedianOrder(LogClip c)
   {
        List<float> l = new List<float>();
        foreach (LogClipFrame f in c.getClipFrames())
       {
           l.Add(Order(f));
       }

        return Median(l);
    }


    private float Order(LogClipFrame f)
   {
       List<LogAgentData> agents = f.getAgentData();
        float psi = Order(agents);
       return psi;
   }


    private float MeanLocalOrder(LogClip c)
    {
        float meanLocalOrder = 0.0f;
        foreach(LogClipFrame f in c.getClipFrames())
        {
            meanLocalOrder += MeanLocalOrder(f);
        }
        meanLocalOrder /= c.getClipFrames().Count;

        return meanLocalOrder;
    }

    private float MeanLocalOrder(LogClipFrame f)
    {
        List<LogAgentData> agents = f.getAgentData();
        int n = agents.Count;
        float fieldOfView = f.GetParameters().GetFieldOfViewSize();
        float blindSpotSize = f.GetParameters().GetBlindSpotSize();
        int i;
        float meanLocalOrder = 0.0f;
        for (i = 0; i < n; i++)
        {
            List<LogAgentData> l = new List<LogAgentData>();
            l.Add(NearestAgent(agents[i], agents));
            l.Add(agents[i]);
            meanLocalOrder += Order(l);
        }
        meanLocalOrder /= n;
        return meanLocalOrder;
    }

    private LogAgentData NearestAgent(LogAgentData agent, List<LogAgentData> l)
    {
        float min = float.MaxValue;
        LogAgentData minAgent = null;
        foreach(LogAgentData a in l)
        {
            if (System.Object.ReferenceEquals(a, agent)) continue;

            float dist = Vector3.Distance(a.getPosition(), agent.getPosition());

            if(dist<min)
            {
                min = dist;
                minAgent = a;
            }
        }

        return minAgent;
    }

    private float MeanOfMedianLocalOrder(LogClip c)
    {
        float meanLocalOrder = 0.0f;
        foreach (LogClipFrame f in c.getClipFrames())
        {
            meanLocalOrder += MedianLocalOrder(f);
        }
        meanLocalOrder /= c.getClipFrames().Count;

        return meanLocalOrder;
    }


    private float MedianOfMeanLocalOrder(LogClip c)
    {
        List<float> localOrder = new List<float>();
        foreach (LogClipFrame f in c.getClipFrames())
        {
            localOrder.Add(MeanLocalOrder(f));
        }
        return Median(localOrder);
    }

    private float MedianOfMedianLocalOrder(LogClip c)
    {
        List<float> localOrder = new List<float>();
        foreach (LogClipFrame f in c.getClipFrames())
        {
            localOrder.Add(MedianLocalOrder(f));
        }
        return Median(localOrder);
    }

    private float MedianLocalOrder(LogClipFrame f)
    {
        List<LogAgentData> agents = f.getAgentData();
        int n = agents.Count;
        float fieldOfView = f.GetParameters().GetFieldOfViewSize();
        float blindSpotSize = f.GetParameters().GetBlindSpotSize();
        int i;
        List<float> localOrder = new List<float>();
        for (i = 0; i < n; i++)
        {
            List<LogAgentData> neighbors = ClipTools.GetNeighbours(agents[i], agents, fieldOfView, blindSpotSize);
            neighbors.Add(agents[i]);
            localOrder.Add(Order(neighbors));
        }
        
        return Median(localOrder);
    }



    private float Order(List<LogAgentData> agents)
    {
        Vector3 b = Vector3.zero;
        int i;
        int n = agents.Count;
        for (i = 0; i < n; i++)
        {
            Vector3 speed = agents[i].getSpeed();
            Vector3 orientation = speed.normalized;
            b += orientation;
        }

        float psi = Vector3.Magnitude(b) / ((float)n);
        return psi;
    }


    #region Methods - effective group motion
    private float MeanEffectiveGroupMotion(LogClip c)
    {
        int nbFrames = c.getClipFrames().Count;
        float meanValue = 0.0f;
        int i = 1;
        while(true)
        {
            float temp = EffectiveGroupMotionAtFrame(c, i);
            if (temp == -1) break;

            meanValue += temp;
            i++;
        }

        meanValue /= (i - 1);

        return meanValue;
    }

    private float MedianEffectiveGroupMotion(LogClip c)
    {
        List<float> l = new List<float>();

        int nbFrames = c.getClipFrames().Count;
        int i = 1;
        while (true)
        {
            float temp = EffectiveGroupMotionAtFrame(c, i);
            if (temp == -1) break;
            l.Add(temp);
            i++;
        }

        return Median(l);
    }


    private float EffectiveGroupMotionAtFrame(LogClip c, int frame)
    {
        if (frame < 1) return -1;
        if (frame >= c.getClipFrames().Count) return -1;
        if (ClipTools.GetClusters(c.getClipFrames()[frame]).Count > 1) return -1;

        float distCM = Vector3.Distance(CenterOfMass(c.getClipFrames()[frame].getAgentData()), CenterOfMass(c.getClipFrames()[frame-1].getAgentData()));

        float meanDist = 0.0f;

        List<LogAgentData> currentPositions = c.getClipFrames()[frame].getAgentData();
        int nbAgent = currentPositions.Count;
        List<LogAgentData> pastPositions = c.getClipFrames()[frame-1].getAgentData();
        for (int i=0; i<nbAgent; i++)
        {
            meanDist += Vector3.Distance(pastPositions[i].getPosition(),currentPositions[i].getPosition());
        }

        meanDist /= nbAgent;

        if (meanDist == 0.0f) return 0.0f; //Protect from division by 0
        else return (distCM / meanDist);
    }
    #endregion

    #region Methods - Fracture visibility score
    private float BestFractureVisibilityScore(LogClip c, int startFrame)
    {
        float bestScore = -1;

        for (int i = startFrame; i < c.getClipFrames().Count; i++)
        {
            float score = FractureVisibilityScore(c, i);
            if (score > bestScore)
            {
                bestScore = score;
            }
        }

        return bestScore;
    }

    private float FractureVisibilityScore(LogClip c, int frame)
    {
        float meanDist = KNNDistanceMedianAt(c, frame, 3);

        float dist = SignificantDistanceBetweenClustersAtFrame( c, frame);
        float ratio = dist / meanDist;

        return ratio;
    }

    #endregion

    #region Methods - Distance inter cluster


    private float SignificantDistanceBetweenClustersAtFrame(LogClip c, int frameNumber)
    {
        List<List<LogAgentData>> clusters = ClipTools.GetOrderedClusters(c.getClipFrames()[frameNumber]);

        float dist = GetSignificantDistanceBetweenClusters(clusters);

        return dist;
    }

    private float GetSignificantDistanceBetweenClusters(List<List<LogAgentData>> clusters) 
    {
        //If there is not enough cluster, exit
        if (clusters.Count < 2) return -1;

        float significantDistance = -1;

        List<List<List<LogAgentData>>> superList = new List<List<List<LogAgentData>>>();
        //Creer des superlist des cluster
        foreach(List<LogAgentData> c in clusters)
        {
            List<List<LogAgentData>> temp = new List<List<LogAgentData>>();
            temp.Add(c);
            superList.Add(temp);
        }

        //Tant que le nombre de superlist est supérieur à 2
        while(superList.Count > 2)
        {
            //Calculer la distance plus petite entre deux clusters
            float minDist = float.MaxValue;
            List<List<LogAgentData>> minCs1 = null;
            List<List<LogAgentData>> minCs2 = null;
  
            for (int i=0; i< superList.Count; i++)
            {
                List<List<LogAgentData>> cs1 = superList[i];
                for (int j = i; j < superList.Count; j++)
                {
                    if (i == j) continue;

                    List<List<LogAgentData>> cs2 = superList[j];
                    
                    float dist = MinDistanceBetweenTwoClusterSets(cs1, cs2);
                    if(dist < minDist)
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

    private float MinDistanceBetweenTwoClusterSets(List<List<LogAgentData>> clusterSet1, List<List<LogAgentData>> clusterSet2)
    {
        float minDist = float.MaxValue;
        foreach(List<LogAgentData> c1 in clusterSet1)
        {
            foreach(List<LogAgentData> c2 in clusterSet2)
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
    private float MinDistanceBetweenTwoClusters(List<LogAgentData> cluster1,List<LogAgentData> cluster2)
    {
        float minDist = float.MaxValue;

        foreach(LogAgentData l1 in cluster1)
        {
            foreach(LogAgentData l2 in cluster2)
            {
                float dist = Vector3.Distance(l1.getPosition(), l2.getPosition());
                if (dist < minDist)
                    minDist = dist;
            }
        }
        return minDist;
    }
    #endregion

    #region Methods - Distance intra cluster
    private float KNNDistanceMedianAt(LogClip c, int frameNumber, int k)
    {


        List<List<LogAgentData>> clusters = ClipTools.GetOrderedClusters(c.getClipFrames()[frameNumber]);

        float dist = KNNDistanceMedian(clusters[0],k);

        return dist;
    }

    private float MeanKNNDistanceAt(LogClip c, int frameNumber, int k)
    {


        List<List<LogAgentData>> clusters = ClipTools.GetOrderedClusters(c.getClipFrames()[frameNumber]);

        float dist = MeanKNNDistance(clusters[0], k);

        return dist;
    }



    /// <summary>
    /// From a list of agent and for each agent, get the median of the k nearest distance to another agent. Returns the median distance.
    /// </summary>
    /// <param name="cluster"> The set of agents from which the k nearest distances of each agent will be calculated. </param>
    /// <param name="k"> The number of nearest distances for each agent.</param>
    /// <returns> The median of the k nearest distances. </returns>
    private float KNNDistanceMedian(List<LogAgentData> cluster, int k)
    {
        List<float> distances = new List<float>();

        foreach (LogAgentData g in cluster)
        {
            distances.AddRange(GetKNNDistances(cluster, g, k));
        }

        return Median(distances);
    }

    private float MeanKNNDistance(List<LogAgentData> cluster, int k)
    {
        float meanDist = 0.0f;
        List<float> distances = new List<float>();

        foreach (LogAgentData g in cluster)
        {
            distances.AddRange(GetKNNDistances(cluster, g, k));
        }

        foreach(float d in distances)
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
    private List<float> GetKNNDistances(List<LogAgentData> cluster, LogAgentData agent, int k)
    {
        //Compute every distance from parameter agent to other agents
        List<float> distances = new List<float>();

        //Compare current agent with all agents
        foreach (LogAgentData g in cluster)
        {
            //Check if the current agent is compared with itself
            if (System.Object.ReferenceEquals(g, agent)) continue;

            //Compute distance
            float dist = Vector3.Distance(g.getPosition(), agent.getPosition());

            distances.Add(dist);
        }


        //Sort list
        distances.Sort(new GFG());


        //Get the knn
        List<float> knnDistances = new List<float>();

        if (distances.Count < k) k = distances.Count;
        for (int i=0; i<k; i++)
        {
            knnDistances.Add(distances[i]);
        }
        
        return knnDistances;
    }

    #endregion



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
}
