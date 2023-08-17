using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class LogAgentData
{
    #region Private fields
    public enum AgentType
    {
        None,
        Reynolds,
        Couzin
    }

    private AgentType agentType;

    private SerializableVector3 position;
    private SerializableVector3 speed;
    private SerializableVector3 acceleration;

    private List<SerializableVector3> forces;
    #endregion

    #region Methods - Constructor
    public LogAgentData(Vector3 position, Vector3 speed, Vector3 acceleration, List<Vector3> forces, AgentType type) //Il faut ajouter l'acceleration
    {
        this.position = position;
        this.speed = speed;
        this.acceleration = acceleration;

        this.forces = new List<SerializableVector3>();
        foreach(Vector3 v in forces)
        {
            this.forces.Add(v);
        }

        this.agentType = type;
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

    
    public Vector3 getAcceleration()
    {
        return this.acceleration;
    }

    public List<Vector3> getForces()
    {
        List<Vector3> resForces = new List<Vector3>();
        foreach(SerializableVector3 v in this.forces)
        {
            resForces.Add(v);
        }
        return resForces;
    }
     
    #endregion
}

