using UnityEngine;


[System.Serializable]
public class LogAgentData
{
    #region Private fields
    private SerializableVector3 position;
    private SerializableVector3 speed;
    #endregion

    #region Methods - Constructor
    public LogAgentData(Vector3 position, Vector3 speed)
    {
        this.position = position;
        this.speed = speed;
    }
    #endregion

    #region Methods - Getter
    public Vector3 getPosition()
    {
        return this.position;
    }

    public Vector3 getSpeed()
    {
        return this.speed;
    }
    #endregion
}

