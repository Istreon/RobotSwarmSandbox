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
        /*"SF_1.dat",
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
        "SF_30.dat",*/
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
        string line = "Filename;FPS;NbFrames;FractureFrame;FracturedFrameV2;DistMedianKNN;MeanKNNDistance;SignificantDist;FractureScore; BestScore; \r";
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
            int fractureFrame = fractureFrames[currentClip];
            int otherFractureFrame = GetFractureFrame(c);
            float knnMedian = KNNDistanceMedianAt(c, otherFractureFrame, 3);
            float significantDistance = SignificantDistanceBetweenClustersAtFrame(c, otherFractureFrame);
            float score = significantDistance / knnMedian;
            float best = BestFractureVisibilityScore(c, otherFractureFrame);
            line = fileNames[currentClip] + ";" + c.getFps() + ";" + c.getClipFrames().Count + ";" + fractureFrame + ";" + GetFractureFrame(c) + ";" + knnMedian + ";" + MeanKNNDistanceAt(c, otherFractureFrame, 3) + ";" + significantDistance + ";" + score + ";" + best +  "\r";
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
        float meanDist = 0.0f;
        List<float> distances = new List<float>();

        foreach (LogAgentData g in cluster)
        {
            distances.AddRange(GetKNNDistances(cluster, g, k));
        }

        distances.Sort(new GFG());

        int n = distances.Count;
        if (n % 2 !=0)
        {
            n = n + 1;
            meanDist = distances[n/2-1];
        }
        else
        {
            meanDist = (distances[n / 2 - 1] + distances[n / 2]) / 2;
        }

       
        return meanDist;
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
