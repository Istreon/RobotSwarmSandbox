using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ExportClipFeatures : MonoBehaviour
{
    string filePath = "/Clips/"; //The folder containing clip files

    private List<LogClip> clips = new List<LogClip>();

    private int currentClip = 0;
    // Start is called before the first frame update
    void Start()
    {
        

        filePath = Application.dataPath + filePath;
        Debug.Log(filePath);

        string[] filePaths = Directory.GetFiles(filePath, "*.dat",
                                         SearchOption.TopDirectoryOnly);



        string resultFilePathCSV = Application.dataPath + "/Results/clip_features.csv";

        StringBuilder sb = new StringBuilder();

        //Prepare csv result file
        string line = "Filename;" +
            "FPS;" +
            "NbFrames;" +
            "FractureFrame;" +
            "FractureScore;" +
            "BestScore;" +
            "SpeedScoreAtFracture;" +
            "BestSpeedScore;" +
            "SpeedScoreAtFracture2;" +
            "BestSpeedScore2;" +
            "SpeedScoreAtFracture3;" +
            "BestSpeedScore3;" +
            "ExpansionScore;" +
            "MeanTowardsCenterOfMass;" +
            //"MedianTowardsCenterOfMass;" +
            "MeanEffectiveGroupMotion;" +
            "MeanEffectiveDirectionMotion;" +
            //"MedianEffectiveGroupMotion;" +
            "MedianOrder;" +
            "MeanOrder;" +
            "MeanTowardsCenterOfMassStandardDeviation;" +
            "StandardDeviationOfKnnDirection\r";
        sb.Append(line);

        //Load all clip
        for (int i = 0; i < filePaths.Length; i++)
        {
            //Loading clip from full file path
            LogClip clip = ClipTools.LoadClip(filePaths[i]);

            if (clip != null)
            {
                clips.Add(clip); //Add the loaded clip to the list
                Debug.Log("Clip " + filePaths[i] + " Loaded.");
            }
            else
                Debug.LogError("Clip can't be load from " + filePaths[i], this);
        }

        Debug.Log("Clips are loaded.");


      
        //Analyse part
        foreach (LogClip c in clips)
        {
            //int fractureFrame = GetFractureFrame(c);
            //int fractureFrame = fractureFrames[currentClip];
            int fractureFrame = GetFractureFrame(c);
            float score = -1;
            float best = -1;
            float speedScoreAtFracture = -1;
            float bestSpeedScore = -1;
            float speedScoreAtFracture2 = -1;
            float bestSpeedScore2 = -1;            
            float speedScoreAtFracture3 = -1;
            float bestSpeedScore3 = -1;

            if (fractureFrame !=-1)
            {              
                best = BestFractureVisibilityScore(c, fractureFrame);
                score = FractureVisibilityScore(c, fractureFrame);
                speedScoreAtFracture = SeparationSpeed(c,fractureFrame);
                bestSpeedScore = BestSeparationSpeed(c);
                speedScoreAtFracture2 = SeparationSpeed2(c, fractureFrame, 10);
                bestSpeedScore2 = BestSeparationSpeed2(c, 10);
                speedScoreAtFracture3 = SeparationSpeed3(c, fractureFrame, 10);
                bestSpeedScore3 = BestSeparationSpeed3(c, 10);
            }

            string s = filePaths[currentClip];
            int pos = s.IndexOf("/");
            while(pos!=-1)
            {
                s = s.Substring(pos + 1);
                pos = s.IndexOf("/");
            } 
            line =  s + ";" 
                + c.getFps() + ";" 
                + c.getClipFrames().Count + ";" 
                + fractureFrame + ";" 
                + score + ";"  
                + best +  ";"
                + speedScoreAtFracture + ";"
                + bestSpeedScore + ";"
                + speedScoreAtFracture2 + ";"
                + bestSpeedScore2 + ";"
                + speedScoreAtFracture3 + ";"
                + bestSpeedScore3 + ";"
                + ExpansionScore(c) + ";" 
                + MeanTowardsCenterOfMass(c) + ";" 
                //+ MedianTowardsCenterOfMass(c) + ";" 
                + MeanEffectiveGroupMotion(c) + ";"
                + MeanEffectiveDirectionMotion(c) + ";"
                //+ MedianEffectiveGroupMotion(c) + ";" 
                + MedianOrder( c) + ";"  
                + MeanOrder(c) + ";" 
                + MeanTowardsCenterOfMassStandardDeviation(c) + ";"
                + StandardDeviationOfKnnDirection(c) + "\r";
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



    //Trouver la frame de la fracture
    //Récuperer le premier groupe de la fracture
    //Mesurer la distance la plus petite entre les deux groupes
    //Identifier les deux agents les plus proches par leur ID
    //Mesurer sur les 10 frames précédentes la vitesse d'éloignement
    //retourner cette vitesse
    //Faire ça sur tous les groupes?

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

        List<List<LogAgentData>> clusters = ClipTools.GetOrderedClusters(c.getClipFrames()[frame]);

        float maxSepSpeed = float.MinValue;

        foreach(List<LogAgentData> cluster in clusters)
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
            if(sepSpeed>maxSepSpeed)
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

        List<LogAgentData> pastAgents = c.getClipFrames()[frame-n].getAgentData();
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

        foreach(LogAgentData l in cluster)
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

        List<LogAgentData> pastAgents = c.getClipFrames()[frame-k].getAgentData();

        //Identifying clusters
        List<List<LogAgentData>> clusters = ClipTools.GetOrderedClusters(c.getClipFrames()[frame]);

        float maxSepSpeed = float.MinValue;

        //For each cluster
        foreach(List<LogAgentData> cluster in clusters)
        {
            //Get the current distance between the cluster and the remaining of the swarm
            float currentDist = DistanceClusterCMFromRemainingSwarmCM(cluster, agents);

            List<LogAgentData> pastCluster = GetPreviousAgentsState(cluster, agents, pastAgents);

            float pastDist = DistanceClusterCMFromRemainingSwarmCM(pastCluster, pastAgents);

            float sepSpeed = ((currentDist - pastDist) / k ) * c.getFps();

            if(sepSpeed > maxSepSpeed)
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

        for(int i = fracFrame; i < c.getClipFrames().Count; i++)
        {
            float res = SeparationSpeed2(c, i, k);
            if(res > maxSepSpeed)
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
        List<List<LogAgentData>> clusters = ClipTools.GetOrderedClusters(c.getClipFrames()[frame]);

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

    private float DensityChangeSpeed(List<LogAgentData> cluster,  List<LogAgentData> pastCluster, int k, int fps)
    {
        float currentDist = MeanDistFromCM(cluster);
        float pastDist = MeanDistFromCM(pastCluster);

        float densityChangeSpeed = ((pastDist - currentDist) /(float) k) * (float)fps;

        return densityChangeSpeed;
    }

    private float MeanDistFromCM(List<LogAgentData> cluster)
    {
        Vector3 cm = CenterOfMass(cluster);

        float meanDist = 0.0f;

        foreach(LogAgentData a in cluster)
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


    private float StandardDeviationOfKnnDirection(LogClip c)
    {
        List<LogClipFrame> frames = c.getClipFrames();
        int n = GetFractureFrame(c);

        if (n == -1) n = frames.Count;

        float mean = 0.0f;
        for (int i=0; i<n; i++)
        {
            mean += StandardDeviationOfKnnDirection(frames[i]);
        }

        return (mean / (float)n);


    }


    private float StandardDeviationOfKnnDirection(LogClipFrame f)
    {
        List<LogAgentData> agents = f.getAgentData();
        List<float> directionDiff = new List<float>();

        foreach(LogAgentData a in agents)
        {
            List<LogAgentData> knn = KNN(a, agents, 3);
            foreach(LogAgentData n in knn)
            {
                float angleDiff = Vector3.Angle(a.getSpeed(), n.getSpeed()) / 180.0f;
                directionDiff.Add(angleDiff);
            }
        }

        return StandardDeviation(directionDiff);
    }



    private float StandardDeviationOfKnnDistance(LogClip c)
    {
        List<LogClipFrame> frames = c.getClipFrames();
        int n = GetFractureFrame(c);

        if (n == -1) n = frames.Count;

        float mean = 0.0f;
        for (int i = 0; i < n; i++)
        {
            mean += StandardDeviationOfKnnDistance(frames[i]);
        }

        return (mean / (float)n);
    }


    private float StandardDeviationOfKnnDistance(LogClipFrame f)
    {
        List<LogAgentData> agents = f.getAgentData();
        List<float> directionDiff = new List<float>();

        foreach (LogAgentData a in agents)
        {
            List<LogAgentData> knn = KNN(a, agents, 3);
            foreach (LogAgentData n in knn)
            {
                float dist = Vector3.Distance(a.getPosition(), n.getPosition());
                directionDiff.Add(dist);
            }
        }

        return StandardDeviation(directionDiff);
    }


    /*

    private float CenterOfMassDirectionVariation(LogClip c)
    {
        List<LogClipFrame> frames = c.getClipFrames();
        int n;
        int fractureFrame = GetFractureFrame(c);

        if (fractureFrame == -1) n = frames.Count;
        else n = fractureFrame;

        float res = 0.0f;
        for (int i = 0; i < n - 2; i++)
        {
            res += Vector3.Angle(CenterOfMassDirection(frames[i], frames[i + 1]), CenterOfMassDirection(frames[i+1], frames[i + 2]))/180;
        }

        res /= (n - 1);
        return res;
    }
    */
    private Vector3 CenterOfMassDirection(LogClipFrame f1,LogClipFrame f2)
    {
        List<LogAgentData> pastAgents = f1.getAgentData();
        List<LogAgentData> agents = f2.getAgentData();

        Vector3 pastCenterOfMass = CenterOfMass(pastAgents);
        Vector3 centerOfMass = CenterOfMass(agents);

        Vector3 direction = centerOfMass - pastCenterOfMass;

        return direction;
    }

    /*
    private float IndividualDirectionVariation(LogClip c)
    {
        List<LogClipFrame> frames = c.getClipFrames();
        int n;
        int fractureFrame = GetFractureFrame(c);

        if (fractureFrame == -1) n = frames.Count;
        else n = fractureFrame;

        float res = 0.0f;
        for(int i=0;i<n-1; i++)
        {
            res += IndividualDirectionVariation(frames[i], frames[i+1]);
        }

        res /= (n - 1);
        return res;
    }


    private float IndividualDirectionVariation(LogClipFrame f1, LogClipFrame f2)
    {
        List<LogAgentData> pastAgents = f1.getAgentData();
        List<LogAgentData> agents = f2.getAgentData();

        int n = agents.Count;

        float res = 0.0f;

        for(int i = 0; i<n; i++)
        {
            res += Vector3.Angle(agents[i].getSpeed(), pastAgents[i].getSpeed())/180;
        }
        res /= n;

        return res;
    }*/


    #region Methods - Expansion and total distance

    /*
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
    }*/

    private float ExpansionScore(LogClip c)
    {
        float startValue = MeanKNNDistanceAt(c, 0, 3);
        float endValue = MeanKNNDistanceAt(c, c.getClipFrames().Count - 1, 3);

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

    private float MeanTowardsCenterOfMassStandardDeviation(LogClip c)
    {
        float res = 0.0f;
        int i = 0;
        foreach (LogClipFrame f in c.getClipFrames())
        {
            if (ClipTools.GetClusters(f).Count > 1) break;

            res += TowardsCenterOfMassStandardDeviation(f);
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

    private float TowardsCenterOfMassStandardDeviation(LogClipFrame f)
    {
        List<LogAgentData> agents = f.getAgentData();
        int n = agents.Count;

        Vector3 centerOfMass = CenterOfMass(agents);
        List<float> l = new List<float>();
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


            l.Add(1-(angle/180));
        }

        
        return StandardDeviation(l);
    }
    #endregion

    #region Methods - Fracture number

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

    private float MeanOrder(LogClip c)
    {
        float meanOrder = 0.0f;
        foreach (LogClipFrame f in c.getClipFrames())
        {
            meanOrder += Order(f);
        }
        meanOrder /= c.getClipFrames().Count;

        return meanOrder;
    }



    private float Order(LogClipFrame f)
    {
        List<LogAgentData> agents = f.getAgentData();
        float psi = Order(agents);
        return psi;
    }

    /*
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
    */


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

    private float MeanEffectiveDirectionMotion(LogClip c)
    {
        int nbFrames = c.getClipFrames().Count;
        float meanValue = 0.0f;
        int i = 1;
        while (true)
        {
            float temp = EffectiveGroupDirectionAtFrame(c, i);
            if (temp == -1) break;

            meanValue += temp;
            i++;
        }

        meanValue /= (i - 1);

        return meanValue;
    }

    private float EffectiveGroupDirectionAtFrame(LogClip c, int frame)
    {
        if (frame < 1) return -1;
        if (frame >= c.getClipFrames().Count) return -1;
        if (ClipTools.GetClusters(c.getClipFrames()[frame]).Count > 1) return -1;

        Vector3 dirCM = CenterOfMass(c.getClipFrames()[frame].getAgentData()) - CenterOfMass(c.getClipFrames()[frame - 1].getAgentData());

        float res = 0.0f;

        List<LogAgentData> currentPositions = c.getClipFrames()[frame].getAgentData();
        int nbAgent = currentPositions.Count;
        List<LogAgentData> pastPositions = c.getClipFrames()[frame - 1].getAgentData();
        for (int i = 0; i < nbAgent; i++)
        {
            Vector3 dir = currentPositions[i].getPosition() - pastPositions[i].getPosition(); 
            res += Vector3.Angle(dir, dirCM) /180;
        }

        res /= nbAgent;

        return 1 - res;
    }
    #endregion

    #region Methods - Fracture visibility score
    /* private float BestFractureVisibilityScore(LogClip c, int startFrame)
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
     }    */


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
        float meanDist = MeanKNNDistanceAt(c, frame, 3);

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
    /*
    private float KNNDistanceMedianAt(LogClip c, int frameNumber, int k)
    {


        List<List<LogAgentData>> clusters = ClipTools.GetOrderedClusters(c.getClipFrames()[frameNumber]);

        float dist = KNNDistanceMedian(clusters[0],k);

        return dist;
    }*/

    private float MeanKNNDistanceAt(LogClip c, int frameNumber, int k)
    {


        List<List<LogAgentData>> clusters = ClipTools.GetOrderedClusters(c.getClipFrames()[frameNumber]);

        float dist = MeanKNNDistance(clusters[0], k);

        return dist;
    }


    /*
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
    }*/

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


    private List<LogAgentData> KNN(LogAgentData agent, List<LogAgentData> l, int n)
    {
        List<LogAgentData> agents = new List<LogAgentData>(l);
        List<LogAgentData> knn = new List<LogAgentData>();

        if (agents.Count < n) n = agents.Count;

        for(int i=0; i<n; i++)
        {
            LogAgentData nearest = NearestAgent(agent, agents);
            knn.Add(nearest);
            agents.Remove(nearest);
        }

        return knn;
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
}
