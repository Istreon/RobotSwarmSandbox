using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableLogClipFrame
{
    private int nbAgents;
    private List<SerializableLogAgentData> agentData;

    public SerializableLogClipFrame(int nbAgents, List<LogAgentData> agentData)
    {
        this.nbAgents = nbAgents;
        this.agentData = new List<SerializableLogAgentData>();
        foreach(LogAgentData l in agentData)
        {
            this.agentData.Add(l);
        }
    }

    public static implicit operator LogClipFrame(SerializableLogClipFrame rValue)
    {
        return new LogClipFrame(rValue.getNbAgents(), rValue.getAgentData());
    }


    public static implicit operator SerializableLogClipFrame(LogClipFrame rValue)
    {
        return new SerializableLogClipFrame(rValue.getNbAgents(), rValue.getAgentData());
    }

    public int getNbAgents()
    {
        return this.nbAgents;
    }

    public List<LogAgentData> getAgentData()
    {
        List<LogAgentData> temp = new List<LogAgentData>();
        foreach(SerializableLogAgentData l in agentData)
        {
            temp.Add(l);
        }
        return temp;
    }
}
