using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SwarmClipTools
{
    #region Methods - Load and Save

    /// <summary>
    /// This method load a clip from a specific file format (.dat), containing a <see cref="LogClip"/> instance.
    /// </summary>
    /// <param name="filePath"> A <see cref="string"/> value corresponding to the absolute path of the file to load</param>
    /// <returns>
    /// A <see cref="LogClip"/> instance from the file, null otherwise. 
    /// </returns>
    public static LogClip LoadClip(string filePath)
    {
        if (File.Exists(filePath))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(filePath, FileMode.Open);
            LogClip clip = null;
            try
            {
                clip = (LogClip)bf.Deserialize(file);
            }
            catch (Exception)
            {
                return null;
            }

            file.Close();
            return clip;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// This method save a <see cref="LogClip"/> into a .dat file.
    /// </summary>
    /// <param name="clip"> The <see cref="LogClip"/> to save.</param>
    /// <param name="filePath"> The absolute path of the file that will contain the clip.</param>
    public static void SaveClip(LogClip clip, string filePath)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(filePath, FileMode.OpenOrCreate);
        bf.Serialize(file, clip);
        file.Close();
    }
    #endregion

    #region Methods - Analyser
    /// <summary>
    /// Detect all neighbours of an agent based on its perception, and return them.
    /// </summary>
    /// <param name="agent"> A <see cref="LogAgentData"/> representing the agent searching its neighbours.</param>
    /// <param name="agentList"> A <see cref="List{T}"/>  of all the agent, containing the possible neighbours.</param>
    /// <param name="fieldOfViewSize"> A <see cref="float"/> value representing the distance of perception of the agent.</param>
    /// <param name="blindSpotSize"> A <see cref="float"/> value representing the blind angle behind the agent where neighbours are not perceived.</param>
    /// <returns> The <see cref="List{T}"/> of neighbours.</returns>
    public  static List<LogAgentData> GetNeighbours(LogAgentData agent, List<LogAgentData> agentList, float fieldOfViewSize, float blindSpotSize)
    {
        List<LogAgentData> detectedAgents = new List<LogAgentData>();
        foreach (LogAgentData g in agentList)
        {
            //Check if the current agent is compared with itself
            if (System.Object.ReferenceEquals(g, agent)) continue;

            //Check whether the agent from the list is close enough (at a distance shorter than the perception distance).
            if (Vector3.Distance(g.getPosition(), agent.getPosition()) <= fieldOfViewSize)
            {
                Vector3 dir = g.getPosition() - agent.getPosition();
                float angle = Vector3.Angle(agent.getSpeed(), dir);
                //Check whether the agent from the list is visible by the current agent (not in the blind spot of the current agent)
                if (angle <= 180 - (blindSpotSize / 2))
                {
                    //Agent is a neighbour, add to list
                    detectedAgents.Add(g);
                }
            }
        }
        return detectedAgents;
    }

    /// <summary>
    /// Analyse the loaded clip and get the différent groups based on agent perception and graph theory.
    /// An agent belong to only one cluster.
    /// </summary>
    /// <returns> A <see cref="List{T}"/> of clusters represented by a <see cref="List{T}"/> of <see cref="LogAgentData"/>.</returns>
    public static List<List<LogAgentData>> GetClusters(LogClipFrame frame)
    {
        //Reset clusters list
        List<List<LogAgentData>> clusters = new List<List<LogAgentData>>();

        //Create a clone of the log agent data list, to manipulate it
        List<LogAgentData> agentsClone = new List<LogAgentData>(frame.getAgentData());

        while (agentsClone.Count > 0)
        {
            //Add first agent in the new cluster
            List<LogAgentData> newCluster = new List<LogAgentData>();
            LogAgentData firstAgent = agentsClone[0];
            agentsClone.Remove(firstAgent);
            newCluster.Add(firstAgent);

            int i = 0;
            while (i < newCluster.Count)
            {
                List<LogAgentData> temp = GetNeighbours(newCluster[i], frame.getAgentData(), frame.GetParameters().GetFieldOfViewSize(),frame.GetParameters().GetBlindSpotSize());
                foreach (LogAgentData g in temp)
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
        return clusters;
    }
    #endregion
}
