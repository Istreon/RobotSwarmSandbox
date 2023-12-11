using System;
using System.Collections.Generic;
using UnityEngine;

public class SwarmTools
{
    #region Methods - Agent perception
    /// <summary>
    /// Check if an agent perceive another another, based on its field of view distance and its blind spot size.
    /// </summary>
    /// <param name="agent">The agent perceiving.</param>
    /// <param name="potentialNeighbour"> The agent potentially perceived.</param>
    /// <param name="fieldOfViewSize">The distance of perception of the agent.</param>
    /// <param name="blindSpotSize">The blind angle behind the agent where neigbours are not perceived.</param>
    /// <returns> A <see cref="bool"/> value set à True if the agent if perceived by the other agent. False ohterwise.</returns>
    private static bool Perceive(AgentData agent, AgentData potentialNeighbour, float fieldOfViewSize, float blindSpotSize)
    {
        //Check whether the potential neighbour is close enough (at a distance shorter than the perception distance).
        if (Vector3.Distance(potentialNeighbour.GetPosition(), agent.GetPosition()) <= fieldOfViewSize)
        {
            Vector3 dir = potentialNeighbour.GetPosition() - agent.GetPosition();
            float angle = Vector3.Angle(agent.GetSpeed(), dir);
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
    /// <param name="fieldOfViewSize">The distance of perception of both agent. If different, use <see cref="Perceive(AgentData, AgentData, float, float)"/> instead </param>
    /// <param name="blindSpotSize">The blind angle behind the agent where neigbours are not perceived. If different, use <see cref="Perceive(AgentData, AgentData, float, float)"/> instead</param>
    /// <returns> A <see cref="bool"/> value set à True if the agent if perceived by the other agent. False ohterwise.</returns>
    public static bool Linked(AgentData agent1, AgentData agent2, float fieldOfViewSize, float blindSpotSize)
    {
        return (Perceive(agent1, agent2, fieldOfViewSize, blindSpotSize) || Perceive(agent2, agent1, fieldOfViewSize, blindSpotSize));
    }

    /// <summary>
    /// Detect all perceived agents of an agent based on its perception, and return them.
    /// </summary>
    /// <param name="agent"> A <see cref="AgentData"/> representing the agent searching its perceveid agents.</param>
    /// <param name="agentList"> A <see cref="List{T}"/>  of all the agent, containing the possible perceveid agents.</param>
    /// <param name="fieldOfViewSize"> A <see cref="float"/> value representing the distance of perception of the agent.</param>
    /// <param name="blindSpotSize"> A <see cref="float"/> value representing the blind angle behind the agent where neigbours are not perceived.</param>
    /// <returns> The <see cref="List{T}"/> of perceived agent.</returns>
    public static List<AgentData> GetPerceivedAgents(AgentData agent, List<AgentData> agentList, float fieldOfViewSize, float blindSpotSize)
    {
        //Create a list that will store the perceived agent
        List<AgentData> detectedAgents = new List<AgentData>();

        //Compare current agent with all agents
        foreach (AgentData g in agentList)
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
    /// <param name="agent"> A <see cref="AgentData"/> representing the agent searching its neighbours.</param>
    /// <param name="agentList"> A <see cref="List{T}"/>  of all the agent, containing the possible neighbours.</param>
    /// <param name="fieldOfViewSize"> A <see cref="float"/> value representing the distance of perception of the agent.</param>
    /// <param name="blindSpotSize"> A <see cref="float"/> value representing the blind angle behind the agent where neigbours are not perceived.</param>
    /// <returns> The <see cref="List{T}"/> of neighbours.</returns>
    public static List<AgentData> GetNeighbours(AgentData agent, List<AgentData> agentList, float fieldOfViewSize, float blindSpotSize)
    {
        //Create a list that will store the perceived agent
        List<AgentData> neighbours = new List<AgentData>();

        //Compare current agent with all agents
        foreach (AgentData g in agentList)
        {
            //Check if the current agent is compared with itself
            if (System.Object.ReferenceEquals(g, agent)) continue;

            if (Linked(agent, g, fieldOfViewSize, blindSpotSize))
            {
                neighbours.Add(g);
            }
        }
        return neighbours;
    }


    /// <summary>
    /// Detect all neighbours of an agent based on its perception, and return their position in the list.
    /// A neighbour here mean that the agent perceived, or is perceived by the neighbour.
    /// </summary>
    /// <param name="agent"> A <see cref="AgentData"/> representing the agent searching its neighbours.</param>
    /// <param name="agentList"> A <see cref="List{T}"/>  of all the agent, containing the possible neighbours.</param>
    /// <param name="fieldOfViewSize"> A <see cref="float"/> value representing the distance of perception of the agent.</param>
    /// <param name="blindSpotSize"> A <see cref="float"/> value representing the blind angle behind the agent where neigbours are not perceived.</param>
    /// <returns> The <see cref="List{T}"/> of the position of agents in the agents list.</returns>
    public static List<int> GetNeighboursIDs(AgentData agent, List<AgentData> agentList, float fieldOfViewSize, float blindSpotSize)
    {
        //Create a list that will store the perceived agent
        List<int> neighboursIDs = new List<int>();

        //Compare current agent with all agents
        for (int i=0; i<agentList.Count; i++)
        {
            //Check if the current agent is compared with itself
            if (System.Object.ReferenceEquals(agentList[i], agent)) continue;

            if (Linked(agent, agentList[i], fieldOfViewSize, blindSpotSize))
            {
                neighboursIDs.Add(i);
            }
        }
        return neighboursIDs;
    }

    #endregion

    #region Methods - Agents links
    /// <summary>
    /// From a swarm, get all unique pairs of agents based on their perception.
    /// This method uses <see cref="GetLinksList(List{AgentData}, float, float)"/> method.
    /// </summary>
    /// <param name="swarm">The analysed swarm</param>
    /// <returns>The <see cref="List{T}"/> of pairs of agents.</returns>
    public static List<Tuple<AgentData, AgentData>> GetLinksList(SwarmData swarm)
    {
        List<Tuple<AgentData, AgentData>> links = GetLinksList(swarm.GetAgentsData(), swarm.GetParameters().GetFieldOfViewSize(), swarm.GetParameters().GetBlindSpotSize());
        return links;
    }

    /// <summary>
    /// From a list of agents and their parameters, get all unique pairs of agents based on their perception.
    /// </summary>
    /// <param name="agents">The list of agents</param>
    /// <param name="fovSize">The field of view size in meters</param>
    /// <param name="bsSize">The blind spot size in degree</param>
    /// <returns>The <see cref="List{T}"/> of pairs of agents.</returns>
    public static List<Tuple<AgentData, AgentData>> GetLinksList(List<AgentData> agents, float fovSize, float bsSize)
    {
        List<Tuple<AgentData, AgentData>> links = new List<Tuple<AgentData, AgentData>>();

        bool[,] adjacentMatrix = GetAdjacentMatrix(agents, fovSize, bsSize);

        for (int i = 0; i < agents.Count; i++)
        {
            for (int j = i; j < agents.Count; j++)
            {
                if (adjacentMatrix[i, j])
                {
                    Tuple<AgentData, AgentData> link = new Tuple<AgentData, AgentData>(agents[i], agents[j]);
                    links.Add(link);
                }
            }
        }
        return links;
    }
    #endregion

    #region Methods - Clusters (connected components)
    /// <summary>
    /// From a swarm, get the différent groups based on agent perception and graph theory.
    /// An agent belong to only one cluster.
    /// </summary>
    /// <returns> A <see cref="List{T}"/> of clusters represented by a <see cref="List{T}"/> of <see cref="AgentData"/>.</returns>
    public static List<List<AgentData>> GetClusters(SwarmData swarm)
    {
        //Create the list that will store the different clusters
        List<List<AgentData>> clusters = new List<List<AgentData>>();

        //Create a clone of the log agent data list, to manipulate it
        List<AgentData> agentsClone = new List<AgentData>(swarm.GetAgentsData());

        while (agentsClone.Count > 0)
        {
            //Create the list representing the first cluster
            List<AgentData> newCluster = new List<AgentData>();
            //The first agent will be choose by default
            AgentData firstAgent = agentsClone[0];
            //Remove the first agent from the list containing all agents (it now belongs to a cluster)
            agentsClone.Remove(firstAgent);
            //Add first agent in the new cluster
            newCluster.Add(firstAgent);

            int i = 0;
            while (i < newCluster.Count)
            {
                List<AgentData> temp = GetNeighbours(newCluster[i], swarm.GetAgentsData(), swarm.GetParameters().GetFieldOfViewSize(), swarm.GetParameters().GetBlindSpotSize());
                foreach (AgentData g in temp)
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
    /// From a swarm get the different groups based on agent perception and graph theory. Those groups are sorted by size.
    /// An agent belongs to only one cluster.
    /// </summary>
    /// <returns> 
    /// A <see cref="List{T}"/> of clusters represented by a <see cref="List{T}"/> of <see cref="AgentData"/>. 
    /// They are sorted by size, from the largest group to the smallest.
    /// </returns>
    public static List<List<AgentData>> GetOrderedClusters(SwarmData swarm)
    {
        List<List<AgentData>> orderedClusters = new List<List<AgentData>>();

        List<List<AgentData>> clusters = GetClusters(swarm);

        while (clusters.Count > 0)
        {
            int maxSize = -1;
            List<AgentData> biggerCluster = null;

            foreach (List<AgentData> c in clusters)
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
    private List<List<AgentData>> GetClustersFromLinks(List<Tuple<AgentData, AgentData>> linksList, List<AgentData> agents)
    {
        //Clone the links list to modify it
        List<Tuple<AgentData, AgentData>> clone = new List<Tuple<AgentData, AgentData>>(linksList);


        //Create the list that will store the different clusters
        List<List<AgentData>> clusters = new List<List<AgentData>>();


        while (clone.Count > 0)
        {
            //Take the first link to start a new cluster
            List<AgentData> newCluster = new List<AgentData>();

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
                List<Tuple<AgentData, AgentData>> temp = new List<Tuple<AgentData, AgentData>>();
                foreach (Tuple<AgentData, AgentData> l in clone)
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

                foreach (Tuple<AgentData, AgentData> t in temp)
                {
                    clone.Remove(t);
                }
                i++;
            }
            clusters.Add(newCluster);
        }

        //Now, all the clusters are defined from the links. However, some agents are missing because they are isolated.
        //Complete the list of cluster by adding isolated agents
        foreach (AgentData a in agents)
        {
            List<AgentData> temp = new List<AgentData>();
            temp.Add(a);
            clusters.Add(temp);
        }

        return clusters;
    }


    #endregion

    #region Methods - Communities
    /// <summary>
    /// This method compute agents' communities from a list of agents (from a swarm) based on their links with other agents and a modularity score using the Louvain algorithm.
    /// </summary>
    /// <param name="swarm"> Le current swarm analysed.</param>
    /// <returns> A <see cref="List{T}"/> of communities (<see cref="List{}"/> of <see cref="AgentData"/>)</returns>
    public static List<List<AgentData>> GetCommunities(SwarmData swarm)
    {
        List<AgentData> agents = swarm.GetAgentsData();

        //Get the adjacent matrix from the agents' list
        bool[,] adjacentMatrix = GetAdjacentMatrix(swarm);
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
        List<List<AgentData>> communities = new List<List<AgentData>>();
        foreach (List<int> c in keyCommunities)
        {
            List<AgentData> agentCom = new List<AgentData>();
            foreach (int i in c)
            {
                agentCom.Add(agents[i]);
            }
            communities.Add(agentCom);
        }

        return communities;
    }

    /// <summary>
    /// This method compute agents' communities from a list of agents (from a swarm) based on their links with other agents and a modularity score using the Louvain algorithm.
    /// </summary>
    /// <param name="swarm"> Le current swarm analysed.</param>
    /// <returns> A <see cref="List{T}"/> of communities (<see cref="List{}"/> of <see cref="AgentData"/>), ordered by community size.</returns>
    public static List<List<AgentData>> GetOrderedCommunities(SwarmData swarm)
    {
        List<List<AgentData>> communities = GetCommunities(swarm);
        for (int i = 1; i < communities.Count; i++)
        {
            for (int j = 0; j < communities.Count - i; j++)
            {
                if (communities[j].Count > communities[j + 1].Count)
                {
                    List<AgentData> temp = communities[j];
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

    #region Methods - Leaves, branches and trunk

    /// <summary>
    /// From a graph of agents, separate agents into 3 categories.
    /// Leaves : agents that share no link with other agents
    /// Branches :  agents from branches of the graph (only one way to reach them, and the graph end at the end of the branch)
    /// Trunk : the lasting agents
    /// </summary>
    /// <param name="swarm"> The swarm analysed.</param>
    /// <returns>A <see cref="Tuple"/> containing leaves, branches and trunk agents.</returns>
    public static Tuple<List<AgentData>, List<AgentData>, List<AgentData>> SeparateLeavesBranchesAndTrunk(SwarmData swarm)
    {
        return SeparateLeavesBranchesAndTrunk(swarm.GetAgentsData(), GetAdjacentMatrix(swarm));
    }


    /// <summary>
    /// From a graph of agents, separate agents into 3 categories.
    /// Leaves : agents that share no link with other agents
    /// Branches :  agents from branches of the graph (only one way to reach them, and the graph end at the end of the branch)
    /// Trunk : the lasting agents
    /// </summary>
    /// <param name="agents"> The agents of the graph.</param>
    /// <param name="adjacentMatrix">The corresponding adjacent matrix of the graph.</param>
    /// <returns>A <see cref="Tuple"/> containing leaves, branches and trunk agents.</returns>
    public static Tuple<List<AgentData>, List<AgentData>, List<AgentData>> SeparateLeavesBranchesAndTrunk(List<AgentData> agents, bool[,] adjacentMatrix)
    {
        List<AgentData> leaves = new List<AgentData>();
        List<AgentData> branches = new List<AgentData>();
        List<AgentData> trunk = new List<AgentData>(agents);

        Tuple<List<AgentData>, List<AgentData>, List<AgentData>> res = new Tuple<List<AgentData>, List<AgentData>, List<AgentData>>(leaves, branches, trunk);

        bool finished = false;
        while (!finished)
        {
            finished = true;
            int[] degreeMatrix = GetDegreeMatrix(adjacentMatrix);
            for (int i = 0; i < degreeMatrix.GetLength(0); i++)
            {
                if (degreeMatrix[i] == 0)
                {
                    leaves.Add(trunk[i]);
                    trunk.RemoveAt(i);
                    adjacentMatrix = RemoveIndexInSquareMatrix(adjacentMatrix, i);
                    finished = false;
                    break;
                }
            }
        }


        finished = false;
        while (!finished)
        {
            finished = true;
            int[] degreeMatrix = GetDegreeMatrix(adjacentMatrix);
            for (int i = 0; i < degreeMatrix.GetLength(0); i++)
            {
                if (degreeMatrix[i] <= 1)
                {
                    branches.Add(trunk[i]);
                    trunk.RemoveAt(i);
                    finished = false;
                    adjacentMatrix = RemoveIndexInSquareMatrix(adjacentMatrix, i);
                    break;
                }
            }
        }

        int count = leaves.Count + branches.Count + trunk.Count;
        if (count != agents.Count) Debug.LogError("Cette méthode duplique ou oublie des agents!");

        return res;

    }



    #endregion

    #region Methods - Graph matrix

    /// <summary>
    /// Compute the adjacent matrix from the agents of the swarm (undirected graph). 
    /// This method uses <see cref="GetAdjacentMatrix(List{AgentData}, float, float)"/> method.
    /// </summary>
    /// <param name="swarm">The analysed swarm.</param>
    /// <returns>The adjacent matrix as a 2D array.</returns>    
    public static bool[,] GetAdjacentMatrix(SwarmData swarm)
    {
        List<AgentData> agents = swarm.GetAgentsData();

        float fovSize = swarm.GetParameters().GetFieldOfViewSize();
        float bsSize = swarm.GetParameters().GetBlindSpotSize();

        bool[,] adjacentMatrix = GetAdjacentMatrix(agents, fovSize, bsSize);

        return adjacentMatrix;
    }

    /// <summary>
    /// Compute the adjacent matrix from the agents, based on the field of view size and the blind spot size set in parameters
    /// </summary>
    /// <param name="agents">The list of agents</param>
    /// <param name="fovSize">The field of view size in meters</param>
    /// <param name="bsSize">The blind spot size in degree</param>
    /// <returns>The adjacent matrix as a 2D array.</returns>
    public static bool[,] GetAdjacentMatrix(List<AgentData> agents, float fovSize, float bsSize)
    {
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
    /// Compute the degree matrix from the corresponding adjacent matrix.
    /// </summary>
    /// <param name="adjacentMatrix">The adjacent matrix of the graph.</param>
    /// <returns>The corresponding degree matrix.</returns>
    public static int[] GetDegreeMatrix(bool[,] adjacentMatrix)
    {
        int[] degreeMatrix = new int[adjacentMatrix.GetLength(0)];

        for (int i = 0; i < adjacentMatrix.GetLength(0); i++)
        {
            int count = 0; //Count the number of links of the current node
            for (int j = 0; j < adjacentMatrix.GetLength(1); j++)
            {
                if (adjacentMatrix[i, j])
                {
                    count++;
                }
            }
            degreeMatrix[i] = count;
        }
        return degreeMatrix;
    }

    /// <summary>
    /// Compute the Laplacian matrix from the adjacent matrix (undirected graph)
    /// </summary>
    /// <param name="adjacentMatrix">The adjacent matrix of the analysed graph.</param>
    /// <returns>The Laplacian matrix as a 2D array.</returns>    
    public static int[,] GetLaplacianMatrix(bool[,] adjacentMatrix)
    {

        int height = adjacentMatrix.GetLength(0);
        int width = adjacentMatrix.GetLength(1);
        int[,] laplacianMatrix = new int[height, width];

        for (int i = 0; i < height; i++)
        {
            int count = 0; //Count the number of links of the current node
            for (int j = 0; j < width; j++)
            {
                if (adjacentMatrix[i, j])
                {
                    count++;
                    laplacianMatrix[i, j] = -1;
                }
            }
            laplacianMatrix[i, i] = count;
        }
        return laplacianMatrix;
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

    /// <summary>
    /// Remove a specified entry from a square matrix. Consequently remove a line and a column.
    /// </summary>
    /// <param name="squareMatrix">The square matrix whose size must be reduced.</param>
    /// <param name="index">The index of the line/column that will be removed.</param>
    /// <returns>The reduced square matrix (of 1).</returns>
    private static bool[,] RemoveIndexInSquareMatrix(bool[,] squareMatrix, int index)
    {
        //Check possible issues
        if (squareMatrix.GetLength(0) != squareMatrix.GetLength(1)) throw new Exception("The specified matrix is not square");
        if (index >= squareMatrix.GetLength(0) || index < 0) throw new ArgumentOutOfRangeException("The specified index is out of the bound of the matrix.");
        if (squareMatrix.GetLength(0) == 1) return new bool[0, 0];

        //Compute the size of the resulting square matrix
        int newSize = squareMatrix.GetLength(0) - 1;

        //Create the new square matrix
        bool[,] newMatrix = new bool[newSize, newSize];

        //Fill the new square matrix, using the original matrix
        int i2 = 0;
        for (int i = 0; i < newSize; i++)
        {
            //Index line must be avoided
            if (i2 == index) i2++;
            int j2 = 0;
            for (int j = 0; j < newSize; j++)
            {
                //Index column must be avoided
                if (j2 == index) j2++;
                newMatrix[i, j] = squareMatrix[i2, j2];
                j2++;

            }
            i2++;
        }

        return newMatrix;
    }

    #endregion

    #region Methods - Center of mass
    /// <summary>
    /// Compute the center of mass of the swarm.
    /// </summary>
    /// <param name="swarmData"> The swarm data.</param>
    /// <returns>The center of mass of the swarm.</returns>
    public static Vector3 GetCenterOfMass(SwarmData swarmData)
    {
        List<AgentData> agents = swarmData.GetAgentsData();

        return GetCenterOfMass(agents);
    }

    public static Vector3 GetCenterOfMass(List<AgentData> agents)
    {
        Vector3 centerOfMass = Vector3.zero;
        foreach (AgentData a in agents)
        {
            centerOfMass += a.GetPosition();
        }
        centerOfMass /= agents.Count;
        return centerOfMass;
    }
    #endregion

    #region Methods - KNN
    public static List<AgentData> KNN(AgentData agent, List<AgentData> l, int n)
    {
        List<AgentData> agents = new List<AgentData>(l);
        List<AgentData> knn = new List<AgentData>();

        if (agents.Count < n) n = agents.Count;

        for (int i = 0; i < n; i++)
        {
            AgentData nearest = NearestAgent(agent, agents);
            knn.Add(nearest);
            agents.Remove(nearest);
        }

        return knn;
    }

    public static AgentData NearestAgent(AgentData agent, List<AgentData> l)
    {
        float min = float.MaxValue;
        AgentData minAgent = null;
        foreach (AgentData a in l)
        {
            if (System.Object.ReferenceEquals(a, agent)) continue;

            float dist = Vector3.Distance(a.GetPosition(), agent.GetPosition());

            if (dist < min)
            {
                min = dist;
                minAgent = a;
            }
        }
        return minAgent;
    }
    #endregion

    #region Methods - Convex hul

    /// <summary>
    /// Get the convex hul of each cluster. 
    /// </summary>
    /// <param name="swarmData">The analysed swarm</param>
    /// <returns>The list of each cluster's convex hul.</returns>
    public static List<List<Vector3>> GetConvexHul(SwarmData swarmData)
    {
        List<List<Vector3>> convexHuls = new List<List<Vector3>>();
        List<List<AgentData>> clusters = SwarmTools.GetOrderedClusters(swarmData);

        foreach (List<AgentData> c in clusters)
        {
            if (c.Count < 3) continue;
            List<Vector3> positions = new List<Vector3>();

            foreach (AgentData g in c)
            {
                positions.Add(g.GetPosition());
            }

            //Calcul du point pivot
            float ordinate = float.MaxValue;
            float abcissa = float.MaxValue;
            Vector3 pivot = Vector3.zero;
            foreach (Vector3 p in positions)
            {
                if (p.z < ordinate || (p.z == ordinate && p.x < abcissa))
                {
                    pivot = p;
                    ordinate = pivot.z;
                    abcissa = pivot.x;
                }
            }
            positions.Remove(pivot);

            //Calcul des angles pour tri
            List<float> angles = new List<float>();
            Vector3 abissaAxe = new Vector3(1, 0, 0);
            foreach (Vector3 p in positions)
            {
                Vector3 temp = p - pivot;
                angles.Add(Vector3.Angle(temp, abissaAxe));
            }

            //Tri des points
            for (int i = 1; i < positions.Count; i++)
            {
                for (int j = 0; j < positions.Count - i; j++)
                {
                    if (angles[j] > angles[j + 1])
                    {
                        float temp = angles[j + 1];
                        angles[j + 1] = angles[j];
                        angles[j] = temp;

                        Vector3 tempPos = positions[j + 1];
                        positions[j + 1] = positions[j];
                        positions[j] = tempPos;
                    }
                }
            }
            angles.Clear();
            positions.Insert(0, pivot);

            //Itérations
            List<Vector3> pile = new List<Vector3>();
            pile.Add(positions[0]);
            pile.Add(positions[1]);

            for (int i = 2; i < positions.Count; i++)
            {
                while ((pile.Count >= 2) && VectorialProduct(pile[pile.Count - 2], pile[pile.Count - 1], positions[i]) <= 0 || pile[pile.Count - 1] == positions[i])
                {
                    pile.RemoveAt(pile.Count - 1);
                }
                pile.Add(positions[i]);
            }

            convexHuls.Add(pile);
        }
        return convexHuls;
    }


    /// <summary>
    /// Compute the area of the convex hul of the biggest cluster of the swarm.
    /// </summary>
    /// <param name="swarmData">The analysed swarm</param>
    /// <returns>The area of the convex hul of the biggest cluster.</returns>
    public static float GetConvexHulArea(SwarmData swarm)
    {
        //Get convex huls of the swarm
        List<List<Vector3>> convexHuls = SwarmTools.GetConvexHul(swarm);

        //Keep only the convex hul of the biggest cluster
        List<Vector3> convexHul = convexHuls[0];

        //Compute the mean position of the vertices composing the convex hull
        Vector3 pointC = Vector3.zero;

        foreach (Vector3 v in convexHul)
        {
            pointC += v;
        }

        pointC /= convexHul.Count;


        float totalArea = 0.0f;

        //Split the convex hull into multiple triangles to compute triangles area independently using Héron formula
        for (int i = 0; i < convexHul.Count; i++)
        {
            Vector3 pointA = convexHul[i];
            int j = (i + 1) % convexHul.Count;
            Vector3 pointB = convexHul[j];

            float a = Vector3.Distance(pointA, pointB);
            float b = Vector3.Distance(pointC, pointB);
            float c = Vector3.Distance(pointA, pointC);

            float d = (a + b + c) / 2.0f;

            float A = Mathf.Sqrt(d * (d - a) * (d - b) * (d - c));

            totalArea += A;
        }

        return totalArea;
    }



    public static float VectorialProduct(Vector3 a, Vector3 b, Vector3 c)
    {
        return ((b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z));
    }
    #endregion
}
