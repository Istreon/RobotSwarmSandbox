using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogClipFrame
{
    private int nbAgents;
    private List<LogAgentData> agentData;


    public LogClipFrame(int nbAgents, List<LogAgentData> agentData)
    {
        this.nbAgents = nbAgents;
        this.agentData = agentData;
    }

    public int getNbAgents()
    {
        return this.nbAgents;
    }

    public List<LogAgentData> getAgentData()
    {
        return this.agentData;
    }
}
