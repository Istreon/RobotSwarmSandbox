using System.Collections.Generic;


[System.Serializable]
public class LogClipFrame
{

    private List<LogAgentData> agentData;
    private LogParameters parameters;


    public LogClipFrame(List<LogAgentData> agentData, LogParameters parameters)
    {
        this.agentData = agentData;
        this.parameters = parameters;
    }

    #region Getter


    public List<LogAgentData> getAgentData()
    {
        return this.agentData;
    }

    public LogParameters GetParameters()
    {
        return this.parameters;
    }
    #endregion
}


