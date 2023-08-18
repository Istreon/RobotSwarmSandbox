using System.Collections.Generic;

[System.Serializable]
public class SwarmData
{
    #region Private fields
    private List<AgentData> agentsData;
    private SwarmParameters parameters;
    #endregion

    #region Methods - Constructor
    public SwarmData(List<AgentData> agentsData, SwarmParameters parameters)
    {
        this.agentsData = agentsData;
        this.parameters = parameters;
    }
    #endregion

    #region Methods - Getter
    public List<AgentData> GetAgentsData()
    {
        return new List<AgentData>(this.agentsData);
    }

    public SwarmParameters GetParameters()
    {
        return this.parameters;
    }
    #endregion

    #region Methods - Setter
    public void SetParameters(SwarmParameters parameters)
    {
        this.parameters = parameters;
    }
    #endregion
}
