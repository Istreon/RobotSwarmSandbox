using System.Collections.Generic;


[System.Serializable]
public class LogClipFrame
{
    #region Private fields
    private List<LogAgentData> agentData;
    private LogParameters parameters;
    #endregion

    #region Methods - Constructor
    public LogClipFrame(List<LogAgentData> agentData, LogParameters parameters)
    {
        this.agentData = agentData;
        this.parameters = parameters;
    }
    #endregion

    #region Methods - Getter
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


