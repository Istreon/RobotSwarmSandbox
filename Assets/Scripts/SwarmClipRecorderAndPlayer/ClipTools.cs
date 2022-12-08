using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ClipTools
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
    /// Check if an agent perceive another another, based on its field of view distance and its blind spot size.
    /// </summary>
    /// <param name="agent">The agent perceiving.</param>
    /// <param name="potentialNeighbour"> The agent potentially perceived.</param>
    /// <param name="fieldOfViewSize">The distance of perception of the agent.</param>
    /// <param name="blindSpotSize">The blind angle behind the agent where neigbours are not perceived.</param>
    /// <returns> A <see cref="bool"/> value set à True if the agent if perceived by the other agent. False ohterwise.</returns>
    private static bool Perceive(LogAgentData agent, LogAgentData potentialNeighbour, float fieldOfViewSize, float blindSpotSize)
    {
        //Check whether the potential neighbour is close enough (at a distance shorter than the perception distance).
        if (Vector3.Distance(potentialNeighbour.getPosition(), agent.getPosition()) <= fieldOfViewSize)
        {
            Vector3 dir = potentialNeighbour.getPosition() - agent.getPosition();
            float angle = Vector3.Angle(agent.getSpeed(), dir);
            //Check whether the potential neighbour is visible by the current agent (not in the blind spot of the current agent)
            if (angle <= 180 - (blindSpotSize / 2))
            {
                //Agent is a neighbour
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Detect all perceived agents of an agent based on its perception, and return them.
    /// </summary>
    /// <param name="agent"> A <see cref="LogAgentData"/> representing the agent searching its perceveid agents.</param>
    /// <param name="agentList"> A <see cref="List{T}"/>  of all the agent, containing the possible perceveid agents.</param>
    /// <param name="fieldOfViewSize"> A <see cref="float"/> value representing the distance of perception of the agent.</param>
    /// <param name="blindSpotSize"> A <see cref="float"/> value representing the blind angle behind the agent where neigbours are not perceived.</param>
    /// <returns> The <see cref="List{T}"/> of perceived agent.</returns>
    public static List<LogAgentData> GetPerceivedAgents(LogAgentData agent, List<LogAgentData> agentList, float fieldOfViewSize, float blindSpotSize)
    {
        //Create a list that will store the perceived agent
        List<LogAgentData> detectedAgents = new List<LogAgentData>();

        //Compare current agent with all agents
        foreach (LogAgentData g in agentList)
        {
            //Check if the current agent is compared with itself
            if (System.Object.ReferenceEquals(g, agent)) continue;

            if (Perceive(agent, g, fieldOfViewSize, blindSpotSize))
            {
                detectedAgents.Add(g);
            }
        }
        return detectedAgents;
    }



    /// <summary>
    /// Detect all neighbours of an agent based on its perception, and return them.
    /// A neighbours here mean that the agent perceived, or is perceived by the neighbour.
    /// </summary>
    /// <param name="agent"> A <see cref="LogAgentData"/> representing the agent searching its neighbours.</param>
    /// <param name="agentList"> A <see cref="List{T}"/>  of all the agent, containing the possible neighbours.</param>
    /// <param name="fieldOfViewSize"> A <see cref="float"/> value representing the distance of perception of the agent.</param>
    /// <param name="blindSpotSize"> A <see cref="float"/> value representing the blind angle behind the agent where neigbours are not perceived.</param>
    /// <returns> The <see cref="List{T}"/> of neighbours.</returns>
    public static List<LogAgentData> GetNeighbours(LogAgentData agent, List<LogAgentData> agentList, float fieldOfViewSize, float blindSpotSize)
    {
        //Create a list that will store the perceived agent
        List<LogAgentData> neighbours = new List<LogAgentData>();

        //Compare current agent with all agents
        foreach (LogAgentData g in agentList)
        {
            //Check if the current agent is compared with itself
            if (System.Object.ReferenceEquals(g, agent)) continue;

            if(Perceive(agent, g, fieldOfViewSize, blindSpotSize) || Perceive(g, agent, fieldOfViewSize, blindSpotSize))
            {
                neighbours.Add(g);
            }
        }
        return neighbours;
    }




    /// <summary>
    /// Analyse the loaded clip and get the différent groups based on agent perception and graph theory.
    /// An agent belong to only one cluster.
    /// </summary>
    /// <returns> A <see cref="List{T}"/> of clusters represented by a <see cref="List{T}"/> of <see cref="LogAgentData"/>.</returns>
    public static List<List<LogAgentData>> GetClusters(LogClipFrame frame)
    {
        //Create the list that will store the different clusters
        List<List<LogAgentData>> clusters = new List<List<LogAgentData>>();

        //Create a clone of the log agent data list, to manipulate it
        List<LogAgentData> agentsClone = new List<LogAgentData>(frame.getAgentData());

        while (agentsClone.Count > 0)
        {
            //Create the list representing the first cluster
            List<LogAgentData> newCluster = new List<LogAgentData>();
            //The first agent will be choose by default
            LogAgentData firstAgent = agentsClone[0];
            //Remove the first agent from the list containing all agents (it now belongs to a cluster)
            agentsClone.Remove(firstAgent);
            //Add first agent in the new cluster
            newCluster.Add(firstAgent);

            int i = 0;
            while (i < newCluster.Count)
            {
                List<LogAgentData> temp = GetNeighbours(newCluster[i], frame.getAgentData(), frame.GetParameters().GetFieldOfViewSize(),frame.GetParameters().GetBlindSpotSize());
                foreach (LogAgentData g in temp)
                {
                    //Check if the neighbour does not already belong to the current cluster
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
