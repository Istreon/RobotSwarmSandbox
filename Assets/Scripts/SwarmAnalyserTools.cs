using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmAnalyserTools : MonoBehaviour
{

    /// <summary>
    /// Check if an agent perceive another another, based on its field of view distance and its blind spot size.
    /// </summary>
    /// <param name="agent">The agent perceiving.</param>
    /// <param name="potentialNeighbour"> The agent potentially perceived.</param>
    /// <param name="fieldOfViewSize">The distance of perception of the agent.</param>
    /// <param name="blindSpotSize">The blind angle behind the agent where neigbours are not perceived.</param>
    /// <returns> A <see cref="bool"/> value set à True if the agent if perceived by the other agent. False ohterwise.</returns>
    private static bool Perceive(GameObject agent, GameObject potentialNeighbour, float fieldOfViewSize, float blindSpotSize)
    {
        //Check whether the potential neighbour is close enough (at a distance shorter than the perception distance).
        if (Vector3.Distance(potentialNeighbour.transform.position, agent.transform.position) <= fieldOfViewSize)
        {
            Vector3 dir = potentialNeighbour.transform.position - agent.transform.position;
            float angle = Vector3.Angle(agent.GetComponent<Agent>().GetSpeed(), dir);
            //Check whether the potential neighbour is visible by the current agent (not in the blind spot of the current agent)
            if (angle <= (180 - (blindSpotSize / 2)))
            {
                //Agent is a neighbour
                return true;
            }
        }
        return false;
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
    public static List<GameObject> GetNeighbours(GameObject agent, List<GameObject> agentList, float fieldOfViewSize, float blindSpotSize)
    {
        //Create a list that will store the perceived agent
        List<GameObject> neighbours = new List<GameObject>();

        //Compare current agent with all agents
        foreach (GameObject g in agentList)
        {
            //Check if the current agent is compared with itself
            if (System.Object.ReferenceEquals(g, agent)) continue;

            if (Perceive(agent, g, fieldOfViewSize, blindSpotSize) || Perceive(g, agent, fieldOfViewSize, blindSpotSize))
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
    public static List<List<GameObject>> GetClusters(List<GameObject> agents)
    {
        //Create the list that will store the different clusters
        List<List<GameObject>> clusters = new List<List<GameObject>>();

        //Create a clone of the log agent data list, to manipulate it
        List<GameObject> agentsClone = new List<GameObject>(agents);

        while (agentsClone.Count > 0)
        {
            //Create the list representing the first cluster
            List<GameObject> newCluster = new List<GameObject>();
            //The first agent will be choose by default
            GameObject firstAgent = agentsClone[0];
            //Remove the first agent from the list containing all agents (it now belongs to a cluster)
            agentsClone.Remove(firstAgent);
            //Add first agent in the new cluster
            newCluster.Add(firstAgent);

            int i = 0;
            while (i < newCluster.Count)
            {
                List<GameObject> temp = GetNeighbours(newCluster[i], agents, newCluster[i].GetComponent<Agent>().GetFieldOfViewSize(), newCluster[i].GetComponent<Agent>().GetBlindSpotSize());
                foreach (GameObject g in temp)
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


}
