using UnityEngine;


[System.Serializable]
public class LogAgentData
{
    #region Private fields
    private SerializableVector3 position;
    private SerializableVector3 speed;
    // private SerializableVector3 acceleration;
    #endregion

    #region Methods - Constructor
    public LogAgentData(Vector3 position, Vector3 speed) //Il faut ajouter l'acceleration
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

    /*
    public Vector3 getAcceleration()
    {
        return this.acceleration
    }
     */
    #endregion
}

