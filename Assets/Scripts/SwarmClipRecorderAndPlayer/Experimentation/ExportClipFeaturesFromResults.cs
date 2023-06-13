using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public class ExportClipFeaturesFromResults : MonoBehaviour
{
    string clipFilesPath = "/Clips/"; //The folder containing clip files
    string resultFilesPath = "/Results/"; //The folder containing clip files

    private List<LogClip> clips = new List<LogClip>();
    private List<ExperimentationResult> results = new List<ExperimentationResult>();

    private int currentClip = 0;



    int[] clipCategory = new int[] { 1, 4, 4, 3, 4, 2, 1, 1, 2, 2, 3, 1, 3, 3, 2, 2, 1, 4, 4, 4, 2, 3, 3, 1, 2, 3, 4, 3, 3, 2, 3, 2, 1, 1, 3, 1, 2, 1, 1, 4, 4, 4, 4, 4, 2, 2, 1, 3 };


    // Start is called before the first frame update
    void Start()
    {

        //Search for clip filepaths
        clipFilesPath = Application.dataPath + clipFilesPath;
        Debug.Log(clipFilesPath);

        string[] clipFilesPaths = Directory.GetFiles(clipFilesPath, "*.dat",
                                 SearchOption.TopDirectoryOnly);

        //Search for results filepaths
        resultFilesPath = Application.dataPath + resultFilesPath;
        Debug.Log(resultFilesPath);

        string[] resultFilesPaths = Directory.GetFiles(resultFilesPath, "*.dat",
                                         SearchOption.TopDirectoryOnly);



        string resultFilePathCSV = Application.dataPath + "/Results/detailed_results.csv";


        //=========================================================================
        //===========================Loading part==================================
        //=========================================================================
        
        //Load all clip
        for (int i = 0; i < clipFilesPaths.Length; i++)
        {
            //Loading clip from full file path
            LogClip clip = ClipTools.LoadClip(clipFilesPaths[i]);

            if (clip != null)
            {
                clips.Add(clip); //Add the loaded clip to the list
                Debug.Log("Clip " + clipFilesPaths[i] + " Loaded.");
            }
            else
                Debug.LogError("Clip can't be load from " + clipFilesPaths[i], this);
        }

        Debug.Log("Clips are loaded.");
        
        //Load all results
        for (int i = 0; i < resultFilesPaths.Length; i++)
        {
          
            if (File.Exists(resultFilesPaths[i]))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(resultFilesPaths[i], FileMode.Open);
                ExperimentationResult res = null;
                try
                {
                    res = (ExperimentationResult)bf.Deserialize(file);
                    results.Add(res); //Add the loaded clip to the list
                    Debug.Log("Result " + resultFilesPaths[i] + " Loaded.");
                }
                catch (Exception)
                {
                    Debug.LogError("Result can't be load from " + resultFilesPaths[i], this);
                }

                file.Close();
            }
                          
        }

        Debug.Log("Result are loaded.");


        //=========================================================================
        //===========================Compute part==================================
        //=========================================================================

        StringBuilder sb = new StringBuilder();

        //Prepare csv result file
        string line = "Filename"+
                       ";ClipCategory" +
                       ";FracturedClip" +               //Le clip contient-il une fracture? 1 : oui, 0 : non
                       ";Participant" +
                       ";Anticipation" +
                       ";CorrectAnswer" +               //Le participant à t-il donné la bonne réponse ? 1 : oui, 0 : non
                       ";AnswerFrame" +                 //Nombre de frames écoulées depuis le début du clip au moment de la réponse
                       ";TimeFromFracture" +            //Nombre de frames depuis la fracture que ce soit positif ou négatif (positif = après, négatif = avant)
                       ";FrameRemaining" +              //Nombre de frames restantes avant la fin du clip au moment de la réponse
                       ";ClipPercentageRemaining" +     //Pourcentage de clip restant au moment de la réponse
                       ";DistanceScoreAtAnswer" +
                       ";SepSpeedAtAnswer" +
                       ";SepSpeed2AtAnswer" +
                       ";SepSpeed3AtAnswer" +
                       ";BestDistanceScore" +
                       ";BestSepSpeed" +
                       ";BestSepSpeed2" +
                       ";BestSepSpeed3" +
                       ";DistanceScoreAtFracture" +
                       ";SepSpeedAtFracture" +
                       ";SepSpeed2AtFracture" +
                       ";SepSpeed3AtFracture" +
                       ";MeanTowardCenterOfMass" +
                       ";MeanEffectiveGroupMotion" +
                       ";StdKNNDirection" +
                       "\r";



        Debug.Log(line);

        sb.Append(line);



        //Analyse part
        foreach (LogClip c in clips)
        {
            int fractureFrame = GetFractureFrame(c);
            

            string s = clipFilesPaths[currentClip];
            int pos = s.IndexOf("/");
            while (pos != -1)
            {
                s = s.Substring(pos + 1);
                pos = s.IndexOf("/");
            }

            float bestDistanceScore = -1;
            float bestSepSpeed = -1;
            float bestSepSpeed2 = -1;
            float bestSepSpeed3 = -1;
            float distanceScoreAtFracture = -1;
            float sepSpeedAtFracture = -1;
            float sepSpeed2AtFracture = -1;
            float sepSpeed3AtFracture = -1;

            if (fractureFrame != -1)
            {
                bestDistanceScore = BestFractureVisibilityScore(c, fractureFrame);
                bestSepSpeed = BestSeparationSpeed(c);
                bestSepSpeed2 = BestSeparationSpeed2(c, 10);
                bestSepSpeed3 = BestSeparationSpeed3(c, 10);
                distanceScoreAtFracture = FractureVisibilityScore(c, fractureFrame);
                sepSpeedAtFracture = SeparationSpeed(c, fractureFrame);
                sepSpeed2AtFracture = SeparationSpeed2(c, fractureFrame,10);
                sepSpeed3AtFracture = SeparationSpeed3(c, fractureFrame,10);
            }


            float meanTowardCenterOfMass = MeanTowardsCenterOfMass(c);
            float meanEffectiveGroupMotion = MeanEffectiveGroupMotion(c);
            float STDKNNDirection = StandardDeviationOfKnnDirection(c);

            int currentParticipant = 0;
            foreach (ExperimentationResult result in results)
            {

                
                string ch = resultFilesPaths[currentParticipant];
                int pos2 = ch.IndexOf("/");
                while (pos2 != -1)
                {
                    ch = ch.Substring(pos2 + 1);
                    pos2 = ch.IndexOf("/");
                }
                pos2 = ch.IndexOf(".");
                ch = ch.Substring(0, pos2);

                float score = - 1;
                float sepSpeed = -1;
                float sepSpeed2 = -1;
                float sepSpeed3 = -1;
                float anticipation = 0;

                int frameNumber = GetAnswerFrameNumber(result, s);
                bool participantAnswer = GetAnswer(result, s);
                if (frameNumber < fractureFrame)
                {
                    anticipation = 1;

                }
                else
                {
                    if(fractureFrame != -1)
                    {
                        score = FractureVisibilityScore(c, frameNumber);
                        sepSpeed = SeparationSpeed(c, frameNumber);
                        sepSpeed2 = SeparationSpeed2(c, frameNumber, 10);
                        sepSpeed3 = SeparationSpeed3(c, frameNumber, 10);
                    } else
                    {
                        anticipation = -1;
                    }

                }

                //Does the participant have the correct answer ? 0 : no, 1 : yes
                int correctAnswer = 0;
                if ((participantAnswer && fractureFrame != -1) || (!participantAnswer && fractureFrame == -1))
                {
                    correctAnswer = 1;
                }

                string timeFromFracture = null;
                if (fractureFrame != -1)
                {
                    int delta = frameNumber - fractureFrame;
                    timeFromFracture = delta.ToString();
                }

                int frameRemaining = c.getClipFrames().Count - frameNumber - 1;
                float clipPercentageRemaining = (float) frameRemaining / (float) c.getClipFrames().Count;

                int fracture = 0;
                if (fractureFrame != -1)
                    fracture = 1;


                line = s
                    + ";" + clipCategory[currentClip]
                    + ";" + fracture
                    + ";" + ch
                    + ";" + anticipation
                    + ";" + correctAnswer
                    + ";" + frameNumber
                    + ";" + timeFromFracture
                    + ";" + frameRemaining
                    + ";" + clipPercentageRemaining
                    + ";" + score
                    + ";" + sepSpeed
                    + ";" + sepSpeed2
                    + ";" + sepSpeed3
                    + ";" + bestDistanceScore
                    + ";" + bestSepSpeed
                    + ";" + bestSepSpeed2
                    + ";" + bestSepSpeed3
                    + ";" + distanceScoreAtFracture
                    + ";" + sepSpeedAtFracture
                    + ";" + sepSpeed2AtFracture
                    + ";" + sepSpeed3AtFracture
                    + ";" + meanTowardCenterOfMass
                    + ";" + meanEffectiveGroupMotion
                    + ";" + STDKNNDirection
                    + "\r";
                //Debug.Log(line);
                sb.Append(line);
                
                
                currentParticipant++;
            }



            currentClip++;
        }


        //Save result
        File.AppendAllText(resultFilePathCSV, sb.ToString());
        sb.Clear();

        Debug.Log("Results saved.");
        
    }


    private int GetAnswerFrameNumber(ExperimentationResult result, String filename)
    {
        foreach (ClipResult c in result.results)
        {
            if (c.filename == filename)
            {
                return c.frameNumber;
            }
        }
        return -2;
    }

    private bool GetAnswer(ExperimentationResult result, String filename)
    {
        foreach (ClipResult c in result.results)
        {
            if (c.filename == filename)
            {
                return c.fracture;
            }
        }
        throw new Exception("Clip " + filename +" not found in participant results.");
    }

    private int GetFractureFrame(LogClip c)
    {
        int res = -1;
        int i = 0;

        int validationTime = 120;
        int fractureDuration = 0;
        foreach (LogClipFrame f in c.getClipFrames())
        {
            List<List<LogAgentData>> clusters = FrameTools.GetClusters(f);
            if (clusters.Count > 1)
            {
                if (fractureDuration == 0)
                    res = i;

                fractureDuration++;

                if (fractureDuration > validationTime)
                    break;
            }
            else
            {
                fractureDuration = 0;
                res = -1;
            }
            i++;
        }
        return res;
    }





    #region Methods - Fracture visibility score


    private float FractureVisibilityScore(LogClip c, int frame)
    {
        float meanDist = MeanKNNDistanceAt(c, frame, 3);

        float dist = SignificantDistanceBetweenClustersAtFrame(c, frame);
        float ratio = dist / meanDist;

        return ratio;
    }

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

    #endregion


    #region Methods - Distance inter cluster


    private float SignificantDistanceBetweenClustersAtFrame(LogClip c, int frameNumber)
    {
        List<List<LogAgentData>> clusters = FrameTools.GetOrderedClusters(c.getClipFrames()[frameNumber]);

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
        foreach (List<LogAgentData> c in clusters)
        {
            List<List<LogAgentData>> temp = new List<List<LogAgentData>>();
            temp.Add(c);
            superList.Add(temp);
        }

        //Tant que le nombre de superlist est supérieur à 2
        while (superList.Count > 2)
        {
            //Calculer la distance plus petite entre deux clusters
            float minDist = float.MaxValue;
            List<List<LogAgentData>> minCs1 = null;
            List<List<LogAgentData>> minCs2 = null;

            for (int i = 0; i < superList.Count; i++)
            {
                List<List<LogAgentData>> cs1 = superList[i];
                for (int j = i; j < superList.Count; j++)
                {
                    if (i == j) continue;

                    List<List<LogAgentData>> cs2 = superList[j];

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

    private float MinDistanceBetweenTwoClusterSets(List<List<LogAgentData>> clusterSet1, List<List<LogAgentData>> clusterSet2)
    {
        float minDist = float.MaxValue;
        foreach (List<LogAgentData> c1 in clusterSet1)
        {
            foreach (List<LogAgentData> c2 in clusterSet2)
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
    private float MinDistanceBetweenTwoClusters(List<LogAgentData> cluster1, List<LogAgentData> cluster2)
    {
        float minDist = float.MaxValue;

        foreach (LogAgentData l1 in cluster1)
        {
            foreach (LogAgentData l2 in cluster2)
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

    private float MeanKNNDistanceAt(LogClip c, int frameNumber, int k)
    {

        List<List<LogAgentData>> clusters = FrameTools.GetOrderedClusters(c.getClipFrames()[frameNumber]);

        float dist = MeanKNNDistance(clusters[0], k);

        return dist;
    }




    private float MeanKNNDistance(List<LogAgentData> cluster, int k)
    {
        float meanDist = 0.0f;
        List<float> distances = new List<float>();

        foreach (LogAgentData g in cluster)
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
        for (int i = 0; i < k; i++)
        {
            knnDistances.Add(distances[i]);
        }

        return knnDistances;
    }

    #endregion


    #region Methods - Separation speed

    private float BestSeparationSpeed(LogClip c)
    {
        int fracFrame = GetFractureFrame(c);
        if (fracFrame == -1) throw new System.Exception("Il n'y a pas de fracture dans ce clip");

        float maxSepSpeed = float.MinValue;

        for (int i = fracFrame; i < c.getClipFrames().Count; i++)
        {
            float res = SeparationSpeed(c, i);
            if (res > maxSepSpeed)
            {
                maxSepSpeed = res;
            }
        }

        return maxSepSpeed;

    }

    private float SeparationSpeed(LogClip c, int frame)
    {

        List<LogAgentData> agents = c.getClipFrames()[frame].getAgentData();

        List<List<LogAgentData>> clusters = FrameTools.GetOrderedClusters(c.getClipFrames()[frame]);

        float maxSepSpeed = float.MinValue;

        foreach (List<LogAgentData> cluster in clusters)
        {
            List<LogAgentData> temp = new List<LogAgentData>(agents);

            foreach (LogAgentData a in cluster)
            {
                temp.Remove(a);
            }

            LogAgentData a1 = null;
            LogAgentData a2 = null;
            float minDist = float.MaxValue;

            foreach (LogAgentData a in cluster)
            {
                foreach (LogAgentData b in temp)
                {
                    float dist = Vector3.Distance(a.getPosition(), b.getPosition());
                    if (dist < minDist)
                    {
                        minDist = dist;
                        a1 = a;
                        a2 = b;
                    }
                }
            }

            if (a1 == null || a2 == null) return -1;
            int id1 = agents.IndexOf(a1);
            int id2 = agents.IndexOf(a2);

            float sepSpeed = SeparationSpeed(c, frame, id1, id2);
            if (sepSpeed > maxSepSpeed)
            {
                maxSepSpeed = sepSpeed;
            }
        }
        return maxSepSpeed;

    }




    private float SeparationSpeed(LogClip c, int frame, int id1, int id2)
    {
        List<LogAgentData> agentsAtFrac = c.getClipFrames()[frame].getAgentData();
        float distAtFrac = Vector3.Distance(agentsAtFrac[id1].getPosition(), agentsAtFrac[id2].getPosition());

        int n = 10;

        if (frame < n) n = frame;

        List<LogAgentData> pastAgents = c.getClipFrames()[frame - n].getAgentData();
        float pastDist = Vector3.Distance(pastAgents[id1].getPosition(), pastAgents[id2].getPosition());


        float change = distAtFrac - pastDist;

        float res = (change / (float)n) * (float)c.getFps(); //To get a value per second

        return res;

    }

    private float DistanceClusterCMFromRemainingSwarmCM(List<LogAgentData> cluster, List<LogAgentData> agents)
    {
        //Remove cluster agents from the swarm agents list
        List<LogAgentData> temp = new List<LogAgentData>(agents);

        foreach (LogAgentData a in cluster)
        {
            temp.Remove(a);
        }

        Vector3 clusterCM = CenterOfMass(cluster);
        Vector3 remSwarmCM = CenterOfMass(temp);

        float dist = Vector3.Distance(clusterCM, remSwarmCM);

        return dist;
    }


    private List<LogAgentData> GetPreviousAgentsState(List<LogAgentData> cluster, List<LogAgentData> agents, List<LogAgentData> pastAgents)
    {
        List<LogAgentData> res = new List<LogAgentData>();

        foreach (LogAgentData l in cluster)
        {
            int pos = agents.IndexOf(l);
            res.Add(pastAgents[pos]);
        }

        return res;
    }


    private float SeparationSpeed2(LogClip c, int frame, int k)
    {
        if (frame < k) throw new System.Exception("Can't calculate on the k previous frame, because there is less past frame.");

        List<LogAgentData> agents = c.getClipFrames()[frame].getAgentData();

        List<LogAgentData> pastAgents = c.getClipFrames()[frame - k].getAgentData();

        //Identifying clusters
        List<List<LogAgentData>> clusters = FrameTools.GetOrderedClusters(c.getClipFrames()[frame]);

        float maxSepSpeed = float.MinValue;

        //For each cluster
        foreach (List<LogAgentData> cluster in clusters)
        {
            //Get the current distance between the cluster and the remaining of the swarm
            float currentDist = DistanceClusterCMFromRemainingSwarmCM(cluster, agents);

            List<LogAgentData> pastCluster = GetPreviousAgentsState(cluster, agents, pastAgents);

            float pastDist = DistanceClusterCMFromRemainingSwarmCM(pastCluster, pastAgents);

            float sepSpeed = ((currentDist - pastDist) / k) * c.getFps();

            if (sepSpeed > maxSepSpeed)
            {
                maxSepSpeed = sepSpeed;
            }
        }

        return maxSepSpeed;

    }


    private float BestSeparationSpeed2(LogClip c, int k)
    {
        int fracFrame = GetFractureFrame(c);
        if (fracFrame == -1) throw new System.Exception("Il n'y a pas de fracture dans ce clip");

        float maxSepSpeed = float.MinValue;

        for (int i = fracFrame; i < c.getClipFrames().Count; i++)
        {
            float res = SeparationSpeed2(c, i, k);
            if (res > maxSepSpeed)
            {
                maxSepSpeed = res;
            }
        }

        return maxSepSpeed;
    }

    private float SeparationSpeed3(LogClip c, int frame, int k)
    {
        if (frame < k) throw new System.Exception("Can't calculate on the k previous frame, because there is less past frame.");

        List<LogAgentData> agents = c.getClipFrames()[frame].getAgentData();

        List<LogAgentData> pastAgents = c.getClipFrames()[frame - k].getAgentData();

        //Identifying clusters
        List<List<LogAgentData>> clusters = FrameTools.GetOrderedClusters(c.getClipFrames()[frame]);

        if (clusters.Count < 2) return float.MinValue;

        float maxSepSpeed = float.MinValue;
        List<LogAgentData> bestCluster = null;
        List<LogAgentData> bestPastCluster = null;

        //For each cluster
        foreach (List<LogAgentData> cluster in clusters)
        {
            //Get the current distance between the cluster and the remaining of the swarm
            float currentDist = DistanceClusterCMFromRemainingSwarmCM(cluster, agents);

            List<LogAgentData> pastCluster = GetPreviousAgentsState(cluster, agents, pastAgents);

            float pastDist = DistanceClusterCMFromRemainingSwarmCM(pastCluster, pastAgents);

            float sepSpeed = ((currentDist - pastDist) / k) * c.getFps();

            if (sepSpeed > maxSepSpeed)
            {
                maxSepSpeed = sepSpeed;
                bestCluster = cluster;
                bestPastCluster = pastCluster;
            }
        }

        float densityChangeSpeed = DensityChangeSpeed(bestCluster, bestPastCluster, k, c.getFps());

        maxSepSpeed += densityChangeSpeed * 2; //fois 2 car on prend en compte le changement de densité du reste de l'essaim de façon simplifié (essaim homogène)


        return maxSepSpeed;

    }

    private float DensityChangeSpeed(List<LogAgentData> cluster, List<LogAgentData> pastCluster, int k, int fps)
    {
        float currentDist = MeanDistFromCM(cluster);
        float pastDist = MeanDistFromCM(pastCluster);

        float densityChangeSpeed = ((pastDist - currentDist) / (float)k) * (float)fps;

        return densityChangeSpeed;
    }

    private float MeanDistFromCM(List<LogAgentData> cluster)
    {
        Vector3 cm = CenterOfMass(cluster);

        float meanDist = 0.0f;

        foreach (LogAgentData a in cluster)
        {
            meanDist += Vector3.Distance(cm, a.getPosition());
        }

        meanDist /= cluster.Count;

        return meanDist;
    }

    private float BestSeparationSpeed3(LogClip c, int k)
    {
        int fracFrame = GetFractureFrame(c);
        if (fracFrame == -1) throw new System.Exception("Il n'y a pas de fracture dans ce clip");

        float maxSepSpeed = float.MinValue;

        for (int i = fracFrame; i < c.getClipFrames().Count; i++)
        {
            float res = SeparationSpeed3(c, i, k);
            if (res > maxSepSpeed)
            {
                maxSepSpeed = res;
            }
        }

        return maxSepSpeed;
    }

    #endregion


    #region Methods - Towards center of mass
    private float MeanTowardsCenterOfMass(LogClip c)
    {
        float res = 0.0f;
        int i = 0;
        foreach (LogClipFrame f in c.getClipFrames())
        {
            if (FrameTools.GetClusters(f).Count > 1) break;

            res += TowardsCenterOfMass(f);
            i++;
        }
        res /= i;

        return res;
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


    #region Methods - effective group motion
    private float MeanEffectiveGroupMotion(LogClip c)
    {
        int nbFrames = c.getClipFrames().Count;
        float meanValue = 0.0f;
        int i = 1;
        while (true)
        {
            float temp = EffectiveGroupMotionAtFrame(c, i);
            if (temp == -1) break;

            meanValue += temp;
            i++;
        }

        meanValue /= (i - 1);

        return meanValue;
    }


    private float EffectiveGroupMotionAtFrame(LogClip c, int frame)
    {
        if (frame < 1) return -1;
        if (frame >= c.getClipFrames().Count) return -1;
        if (FrameTools.GetClusters(c.getClipFrames()[frame]).Count > 1) return -1;

        float distCM = Vector3.Distance(CenterOfMass(c.getClipFrames()[frame].getAgentData()), CenterOfMass(c.getClipFrames()[frame - 1].getAgentData()));

        float meanDist = 0.0f;

        List<LogAgentData> currentPositions = c.getClipFrames()[frame].getAgentData();
        int nbAgent = currentPositions.Count;
        List<LogAgentData> pastPositions = c.getClipFrames()[frame - 1].getAgentData();
        for (int i = 0; i < nbAgent; i++)
        {
            meanDist += Vector3.Distance(pastPositions[i].getPosition(), currentPositions[i].getPosition());
        }

        meanDist /= nbAgent;

        if (meanDist == 0.0f) return 0.0f; //Protect from division by 0
        else return (distCM / meanDist);
    }

    #endregion


    #region Methods - STD KNN Direction


    private float StandardDeviationOfKnnDirection(LogClip c)
    {
        List<LogClipFrame> frames = c.getClipFrames();
        int n = GetFractureFrame(c);

        if (n == -1) n = frames.Count;

        float mean = 0.0f;
        for (int i = 0; i < n; i++)
        {
            mean += StandardDeviationOfKnnDirection(frames[i]);
        }

        return (mean / (float)n);


    }


    private float StandardDeviationOfKnnDirection(LogClipFrame f)
    {
        List<LogAgentData> agents = f.getAgentData();
        List<float> directionDiff = new List<float>();

        foreach (LogAgentData a in agents)
        {
            List<LogAgentData> knn = KNN(a, agents, 3);
            foreach (LogAgentData n in knn)
            {
                float angleDiff = Vector3.Angle(a.getSpeed(), n.getSpeed()) / 180.0f;
                directionDiff.Add(angleDiff);
            }
        }

        return StandardDeviation(directionDiff);
    }
    #endregion


    #region Methods - tools
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

    private LogAgentData NearestAgent(LogAgentData agent, List<LogAgentData> l)
    {
        float min = float.MaxValue;
        LogAgentData minAgent = null;
        foreach (LogAgentData a in l)
        {
            if (System.Object.ReferenceEquals(a, agent)) continue;

            float dist = Vector3.Distance(a.getPosition(), agent.getPosition());

            if (dist < min)
            {
                min = dist;
                minAgent = a;
            }
        }

        return minAgent;
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


    private List<LogAgentData> KNN(LogAgentData agent, List<LogAgentData> l, int n)
    {
        List<LogAgentData> agents = new List<LogAgentData>(l);
        List<LogAgentData> knn = new List<LogAgentData>();

        if (agents.Count < n) n = agents.Count;

        for (int i = 0; i < n; i++)
        {
            LogAgentData nearest = NearestAgent(agent, agents);
            knn.Add(nearest);
            agents.Remove(nearest);
        }

        return knn;
    }
    #endregion

}


