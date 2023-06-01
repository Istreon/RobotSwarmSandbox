using System.Collections.Generic;
using UnityEngine;
public class ModularityOptimisation
{
    public static List<List<Agent>> GetCommunities(List<Agent> agents)
    {
        bool[,] adjacentMatrix = GetAdjacentMatrix(agents);
        int m = GetNumberOfEdge(adjacentMatrix);
        
        List<List<int>> keyCommunities = new List<List<int>>();
        for(int i=0; i<agents.Count; i++)
        {
            List<int> temp = new List<int>();
            temp.Add(i);
            keyCommunities.Add(temp);
        }


        bool done = false;
        while(!done)
        {
            float bestModularityScore = float.MinValue;
            List<int> bestCom1 = null;
            List<int> bestCom2 = null;

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

            if (bestModularityScore < 0.0f) done = true;
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

    private static float GetModularityScore(List<int> community1, List<int> community2, bool[,] adjacentMatrix, int m)
    {
        float score = 0.0f; //https://www.youtube.com/watch?v=lG5hkAHo-zs
        foreach(int i in community1)
        {
            foreach(int j in community2)
            {
                int ki = GetNumberOfEdgeFromNode(i, adjacentMatrix);
                int kj = GetNumberOfEdgeFromNode(j, adjacentMatrix);
                float val = (ki * kj) / (2.0f * (float)m);
                
                if (adjacentMatrix[i,j])
                {
                    val = 1 - val;
                } else
                {
                    val = 0 - val;
                }
                score += val;
            }
        }
        return score;
    }



    public static bool[,] GetAdjacentMatrix(List<Agent> agents)
    {
        bool[,] adjacentMatrix = new bool[agents.Count,agents.Count];
        for(int i=0;i<agents.Count; i++)
        {
            for (int j = 0; j < agents.Count; j++)
            {
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
