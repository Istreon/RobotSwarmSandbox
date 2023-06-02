using System.Collections.Generic;
using UnityEngine;
public class ModularityOptimisation
{
    /// <summary>
    /// This method compute agents' communities from a list of agents based on their links with other agents and a modularity score using the Louvain algorithm.
    /// </summary>
    /// <param name="agents"> The list of <see cref="Agent"/> from which communities will be computed</param>
    /// <returns> A <see cref="List{T}"/> of communities (<see cref="List{}"/> of <see cref="Agent"/>)</returns>
    public static List<List<Agent>> GetCommunities(List<Agent> agents)
    {
        //Get the adjacent matrix from the agents' list
        bool[,] adjacentMatrix = GetAdjacentMatrix(agents);
        //Get the total number of edges from the adjacent matrix
        int m = GetNumberOfEdge(adjacentMatrix);
        
        //In order to use the adjacent matrix whith integer indexes, replace Agent with their position in the list of agents
        List<List<int>> keyCommunities = new List<List<int>>();

        //In the Louvain algorithm, each agent is a community at the start
        for(int i=0; i<agents.Count; i++)
        {
            List<int> temp = new List<int>();
            temp.Add(i);
            keyCommunities.Add(temp);
        }


        bool done = false;
        while(!done)
        {
            //Search for the best modularity score between two communities
            float bestModularityScore = float.MinValue;
            List<int> bestCom1 = null;
            List<int> bestCom2 = null;

            //Test for each pair of communities
            for(int i = 0; i< keyCommunities.Count-1; i++)
            {
                for (int j = i+1; j < keyCommunities.Count; j++)
                {
                    float score = GetModularityScore(keyCommunities[i], keyCommunities[j], adjacentMatrix,m);
                    if(score > bestModularityScore)
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
        List<List<Agent>> communities = new List<List<Agent>>();
        foreach (List<int> c in keyCommunities)
        {
            List<Agent> agentCom = new List<Agent>();
            foreach(int i in c)
            {
                agentCom.Add(agents[i]);
            }
            communities.Add(agentCom);
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
            foreach(int j in community2)
            {
                int ki = GetNumberOfEdgeFromNode(i, adjacentMatrix);
                int kj = GetNumberOfEdgeFromNode(j, adjacentMatrix);
                float val = (ki * kj) / (2.0f * (float)m);
                
                //Check if the two agents are direclty linked or not
                if (adjacentMatrix[i,j])
                {
                    val = 1 - val;
                } else
                {
                    val = 0 - val;
                }

                //Add the score to the total
                score += val;
            }
        }
        return score;
    }


    /// <summary>
    /// Compute the adjacent matrix from a list of <see cref="Agent"/>.
    /// </summary>
    /// <param name="agents">The list of agents from which the adjacent matrix will be compute.</param>
    /// <returns>The adjacent matrix as a 2D array.</returns>
    public static bool[,] GetAdjacentMatrix(List<Agent> agents)
    {
        bool[,] adjacentMatrix = new bool[agents.Count,agents.Count];

        //For each pair of agents
        for(int i=0;i<agents.Count; i++)
        {
            for (int j = 0; j < agents.Count; j++)
            {
                //If both agents are linked
                if(i != j && SwarmAnalyserTools.Linked(agents[i].gameObject,agents[j].gameObject,agents[i].GetFieldOfViewSize(), agents[i].GetBlindSpotSize())) {
                    adjacentMatrix[i, j] = true;
                } else
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
}
