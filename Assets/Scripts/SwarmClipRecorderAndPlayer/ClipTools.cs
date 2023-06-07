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
    /// Check if at least one of the two agent perceive the other one.
    /// </summary>
    /// <param name="agent1">The first agent tested.</param>
    /// <param name="agent2"> The second agent tested.</param>
    /// <param name="fieldOfViewSize">The distance of perception of both agent. If different, use <see cref="Perceive(LogAgentData, LogAgentData, float, float)"/> instead </param>
    /// <param name="blindSpotSize">The blind angle behind the agent where neigbours are not perceived. If different, use <see cref="Perceive(LogAgentData, LogAgentData, float, float)"/> instead</param>
    /// <returns> A <see cref="bool"/> value set à True if the agent if perceived by the other agent. False ohterwise.</returns>
    public static bool Linked(LogAgentData agent1, LogAgentData agent2, float fieldOfViewSize, float blindSpotSize)
    {
        return (Perceive(agent1, agent2, fieldOfViewSize, blindSpotSize) || Perceive(agent2, agent1, fieldOfViewSize, blindSpotSize));
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
    /// A neighbour here mean that the agent perceived, or is perceived by the neighbour.
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
    /// From a clip frame, get all unique pairs of agents based on their perception.
    /// </summary>
    /// <param name="frame">The analysed frame</param>
    /// <returns>The <see cref="List{T}"/> of pairs of agents.</returns>
    public static List<Tuple<LogAgentData, LogAgentData>> GetLinksList(LogClipFrame frame)
    {
        List<Tuple<LogAgentData, LogAgentData>> links = new List<Tuple<LogAgentData, LogAgentData>>();

        List<LogAgentData> agents = frame.getAgentData();

        foreach (LogAgentData a in agents)
        {
            //Get agent neigbours
            List<LogAgentData> neigbours = GetNeighbours(a, agents, frame.GetParameters().GetFieldOfViewSize(), frame.GetParameters().GetBlindSpotSize());

            foreach (LogAgentData n in neigbours)
            {
                //Check if the list already contains this pair
                if (!ContainsLink(a, n, links))
                {
                    Tuple<LogAgentData, LogAgentData> link = new Tuple<LogAgentData, LogAgentData>(a,n);
                    links.Add(link);
                }
            }
        }

        return links;
    }


    /// <summary>
    /// Check if the list of links in parameter contains or not the pair formed by the two agents in parameter.
    /// </summary>
    /// <param name="agent">The first agent of the pair.</param>
    /// <param name="neighbour">The second agent of the pair.</param>
    /// <param name="links">The list of existing links</param>
    /// <returns>True if the list already contains this pair, False otherwise.</returns>
    private static bool ContainsLink(LogAgentData agent, LogAgentData neighbour, List<Tuple<LogAgentData, LogAgentData>> links)
    {
        bool exists = false;
        foreach (Tuple<LogAgentData, LogAgentData> t in links)
        {
            if (System.Object.ReferenceEquals(t.Item1, agent) && System.Object.ReferenceEquals(t.Item2, neighbour)) exists = true;
            if (System.Object.ReferenceEquals(t.Item1, neighbour) && System.Object.ReferenceEquals(t.Item2, agent)) exists = true;
        }

        return exists;
    }

    /// <summary>
    /// Compute the adjacent matrix from the agents of the frame
    /// </summary>
    /// <param name="frame">The analysed frame.</param>
    /// <returns>The adjacent matrix as a 2D array.</returns>    
    public static bool[,] GetAdjacentMatrix(LogClipFrame frame)
    {
        List<LogAgentData> agents = frame.getAgentData();

        float fovSize = frame.GetParameters().GetFieldOfViewSize();
        float bsSize = frame.GetParameters().GetBlindSpotSize();

        bool[,] adjacentMatrix = new bool[agents.Count, agents.Count];

        //For each pair of agents
        for (int i = 0; i < agents.Count; i++)
        {
            for (int j = 0; j < agents.Count; j++)
            {
                //If both agents are linked
                if (i != j && Linked(agents[i], agents[j], fovSize, bsSize))
                {
                    adjacentMatrix[i, j] = true;
                }
                else
                {
                    adjacentMatrix[i, j] = false;
                }
            }
        }
        return adjacentMatrix;
    }


    /// <summary>
    /// Calculate the total number of edges of a graph from the adjacent matrix.
    /// </summary>
    /// <param name="adjacentMatrix">The adjacent matrix of a graph.</param>
    /// <returns> The number of edges.</returns>
    private static int GetNumberOfEdge(bool[,] adjacentMatrix)
    {
        int count = 0;

        int width = adjacentMatrix.GetLength(0);
        int height = adjacentMatrix.GetLength(1);

        for (int i = 0; i < width - 1; i++)
        {
            for (int j = i + 1; j < height; j++)
            {
                if (adjacentMatrix[i, j]) count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Calculate the number of edges of one node from the adjacent matrix.
    /// </summary>
    /// <param name="node">The id of the node, referring to its line in the adjacent matrix.</param>
    /// <param name="adjacentMatrix">The adejacent matrix of a graph.</param>
    /// <returns>The number of edges of the node.</returns>
    private static int GetNumberOfEdgeFromNode(int node, bool[,] adjacentMatrix)
    {
        int count = 0;

        int height = adjacentMatrix.GetLength(1);

        for (int i = 0; i < height; i++)
        {
            if (adjacentMatrix[node, i]) count++;
        }

        return count;

    }

    #endregion

    #region Methods - Clusters (connected components)
    /// <summary>
    /// Analyse the loaded clip and get the différent groups based on agent perception and graph theory from a single frame.
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
                List<LogAgentData> temp = GetNeighbours(newCluster[i], frame.getAgentData(), frame.GetParameters().GetFieldOfViewSize(), frame.GetParameters().GetBlindSpotSize());
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

    /// <summary>
    /// Analyse the loaded clip and get the différent groups based on agent perception and graph theory from a single frame. Those groups are sorted by size.
    /// An agent belong to only one cluster.
    /// </summary>
    /// <returns> 
    /// A <see cref="List{T}"/> of clusters represented by a <see cref="List{T}"/> of <see cref="LogAgentData"/>. 
    /// They are sorted by size, from the largest group to the smallest.
    /// </returns>
    public static List<List<LogAgentData>> GetOrderedClusters(LogClipFrame frame)
    {
        List<List<LogAgentData>> orderedClusters = new List<List<LogAgentData>>();

        List<List<LogAgentData>> clusters = GetClusters(frame);

        while (clusters.Count > 0)
        {
            int maxSize = -1;
            List<LogAgentData> biggerCluster = null;

            foreach (List<LogAgentData> c in clusters)
            {
                if (c.Count > maxSize)
                {
                    maxSize = c.Count;
                    biggerCluster = c;
                }
            }
            orderedClusters.Add(biggerCluster);
            clusters.Remove(biggerCluster);
        }

        return orderedClusters;
    }


    /// <summary>
    /// From a list of linked agents, compute the different cluster (non-connected graphs) and return it.
    /// </summary>
    /// <param name="linksList">The list of connected agents.</param>
    /// <param name="agents">The list of agents.</param>
    /// <returns>The list containing all clusters.</returns>
    private List<List<LogAgentData>> GetClustersFromLinks(List<Tuple<LogAgentData, LogAgentData>> linksList, List<LogAgentData> agents)
    {
        //Clone the links list to modify it
        List<Tuple<LogAgentData, LogAgentData>> clone = new List<Tuple<LogAgentData, LogAgentData>>(linksList);


        //Create the list that will store the different clusters
        List<List<LogAgentData>> clusters = new List<List<LogAgentData>>();


        while (clone.Count > 0)
        {
            //Take the first link to start a new cluster
            List<LogAgentData> newCluster = new List<LogAgentData>();

            //Add the two agents of the link
            newCluster.Add(clone[0].Item1);
            newCluster.Add(clone[0].Item2);

            //Remove the two agents from the list
            agents.Remove(clone[0].Item1);
            agents.Remove(clone[0].Item2);

            //Remove the link from the list
            clone.RemoveAt(0);

            int i = 0;
            while (i < newCluster.Count)
            {
                List<Tuple<LogAgentData, LogAgentData>> temp = new List<Tuple<LogAgentData, LogAgentData>>();
                foreach (Tuple<LogAgentData, LogAgentData> l in clone)
                {
                    if (System.Object.ReferenceEquals(l.Item1, newCluster[i]))
                    {
                        temp.Add(l);
                        if (!newCluster.Contains(l.Item2))
                        {
                            newCluster.Add(l.Item2);
                            agents.Remove(l.Item2);
                        }
                    }

                    if (System.Object.ReferenceEquals(l.Item2, newCluster[i]))
                    {
                        temp.Add(l);
                        if (!newCluster.Contains(l.Item1))
                        {
                            newCluster.Add(l.Item1);
                            agents.Remove(l.Item1);
                        }
                    }
                }

                foreach (Tuple<LogAgentData, LogAgentData> t in temp)
                {
                    clone.Remove(t);
                }
                i++;
            }
            clusters.Add(newCluster);
        }

        //Now, all the clusters are defined from the links. However, some agents are missing because they are isolated.
        //Complete the list of cluster by adding isolated agents
        foreach (LogAgentData a in agents)
        {
            List<LogAgentData> temp = new List<LogAgentData>();
            temp.Add(a);
            clusters.Add(temp);
        }

        return clusters;
    }


    #endregion

    #region Methods - Communities
    /// <summary>
    /// This method compute agents' communities from a list of agents (from a frame) based on their links with other agents and a modularity score using the Louvain algorithm.
    /// </summary>
    /// <param name="frame"> Le current frame analysed.</param>
    /// <returns> A <see cref="List{T}"/> of communities (<see cref="List{}"/> of <see cref="LogAgentData"/>)</returns>
    public static List<List<LogAgentData>> GetCommunities(LogClipFrame frame)
    {
        List<LogAgentData> agents = frame.getAgentData();

        //Get the adjacent matrix from the agents' list
        bool[,] adjacentMatrix = GetAdjacentMatrix(frame);
        //Get the total number of edges from the adjacent matrix
        int m = GetNumberOfEdge(adjacentMatrix);

        //In order to use the adjacent matrix whith integer indexes, replace Agent with their position in the list of agents
        List<List<int>> keyCommunities = new List<List<int>>();

        //In the Louvain algorithm, each agent is a community at the start
        for (int i = 0; i < agents.Count; i++)
        {
            List<int> temp = new List<int>();
            temp.Add(i);
            keyCommunities.Add(temp);
        }


        bool done = false;
        while (!done)
        {
            //Search for the best modularity score between two communities
            float bestModularityScore = float.MinValue;
            List<int> bestCom1 = null;
            List<int> bestCom2 = null;

            //Test for each pair of communities
            for (int i = 0; i < keyCommunities.Count - 1; i++)
            {
                for (int j = i + 1; j < keyCommunities.Count; j++)
                {
                    float score = GetModularityScore(keyCommunities[i], keyCommunities[j], adjacentMatrix, m);
                    if (score > bestModularityScore)
                    {
                        bestModularityScore = score;
                        bestCom1 = keyCommunities[i];
                        bestCom2 = keyCommunities[j];
                    }
                }
            }

            //If the modularity score is positive, merge the two corresponding communities and repeat. Else, stop.
            if (bestModularityScore <= 0.0f) done = true;
            else
            {
                bestCom1.AddRange(bestCom2);
                keyCommunities.Remove(bestCom2);
            }
        }

        //Last step, convert communities of key into communities of agents
        List<List<LogAgentData>> communities = new List<List<LogAgentData>>();
        foreach (List<int> c in keyCommunities)
        {
            List<LogAgentData> agentCom = new List<LogAgentData>();
            foreach (int i in c)
            {
                agentCom.Add(agents[i]);
            }
            communities.Add(agentCom);
        }

        return communities;
    }

    /// <summary>
    /// This method compute agents' communities from a list of agents (from a frame) based on their links with other agents and a modularity score using the Louvain algorithm.
    /// </summary>
    /// <param name="frame"> Le current frame analysed.</param>
    /// <returns> A <see cref="List{T}"/> of communities (<see cref="List{}"/> of <see cref="LogAgentData"/>), ordered by community size.</returns>
    public static List<List<LogAgentData>> GetOrderedCommunities(LogClipFrame frame)
    {
        List<List<LogAgentData>> communities = GetCommunities(frame);
        for (int i = 1; i < communities.Count; i++)
        {
            for (int j = 0; j < communities.Count - i; j++)
            {
                if (communities[j].Count > communities[j + 1].Count)
                {
                    List<LogAgentData> temp = communities[j];
                    communities[j] = communities[j + 1];
                    communities[j + 1] = temp;
                }
            }
        }
        return communities;
    }


    /// <summary>
    /// From two agents communities, computes the modularity score as if they were merged; and return it.
    /// </summary>
    /// <param name="community1">The first community containing <see cref="int"/> keys referring to the indexes of the adjacent matrix (each key is an agent).</param>
    /// <param name="community2">The second community containing <see cref="int"/> keys referring to the indexes of the adjacent matrix (each key is an agent).</param>
    /// <param name="adjacentMatrix"> The adjacent matrix of the graph. Can be obtained using <see cref="GetAdjacentMatrix(List{Agent})"/>.</param>
    /// <param name="m"> The number of edge of the graph. Can be obtained using <see cref="GetNumberOfEdge(bool[,])"/> from the adjacent matrix.</param>
    /// <returns> The modularity score from the two merged communities.</returns>
    private static float GetModularityScore(List<int> community1, List<int> community2, bool[,] adjacentMatrix, int m)
    {
        float score = 0.0f; //https://www.youtube.com/watch?v=lG5hkAHo-zs

        //Compute for each pair of agents a score
        foreach (int i in community1)
        {
            foreach (int j in community2)
            {
                int ki = GetNumberOfEdgeFromNode(i, adjacentMatrix);
                int kj = GetNumberOfEdgeFromNode(j, adjacentMatrix);
                float val = (ki * kj) / (2.0f * (float)m);

                //Check if the two agents are direclty linked or not
                if (adjacentMatrix[i, j])
                {
                    val = 1 - val;
                }
                else
                {
                    val = 0 - val;
                }

                //Add the score to the total
                score += val;
            }
        }
        return score;
    }

    #endregion
}
