using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AgentData
{
    private SerializableVector3 position;
    private SerializableVector3 direction;
    private SerializableVector3 speed;
    private SerializableVector3 acceleration;

    private List<SerializableVector3> forces;

    #region Methods - Contructor
    public AgentData(Vector3 position, Vector3 direction)
    {
        this.SetPosition(position);
        this.SetDirection(direction);
        this.SetSpeed(Vector3.zero);
        this.acceleration = Vector3.zero;
        this.forces = new List<SerializableVector3>();
    }

    public AgentData(Vector3 position, Vector3 direction, Vector3 speed, Vector3 acceleration, List<Vector3> forces)
    {
        this.SetPosition(position);
        this.SetDirection(direction);
        this.SetSpeed(speed);
        this.acceleration = acceleration;
        this.SetForces(forces);
    }

    #endregion

    #region Methods
    public Vector3 UdpateAcceleration()
    {
        this.acceleration = Vector3.zero;
        foreach(Vector3 f in forces)
        {
            this.acceleration += f;
        }
        return GetAcceleration();
    }

    public Vector3 UpdateSpeed(float elipsedTime)
    {
        Vector3 acc = this.acceleration;
        this.speed +=  acc * elipsedTime;

        return GetSpeed();
    }

    public void ClearForces()
    {
        this.forces.Clear();
    }

    #endregion

    #region Methods - Getter
    public Vector3 GetPosition()
    {
        return position;
    }

    public Vector3 GetDirection()
    {
        return direction;
    }

    public Vector3 GetSpeed()
    {
        return speed;
    }

    public Vector3 GetAcceleration()
    {
        return acceleration;
    }

    public List<Vector3> GetForces()
    {
        List<Vector3> res = new List<Vector3>();
        foreach(SerializableVector3 v in this.forces)
        {
            res.Add(v);
        }
        return res;
    }

    public AgentData Clone()
    {
        AgentData clone = new AgentData(GetPosition(), GetDirection(), GetSpeed(), GetAcceleration(), GetForces());
        return clone;
    }
    #endregion

    #region Methods - Setter
    public void SetPosition(Vector3 position)
    {
        this.position = position;
    }

    public void SetDirection(Vector3 direction)
    {
        this.direction = direction;
    }

    public void SetSpeed(Vector3 speed)
    {
        this.speed = speed;
    }

    public void SetForces(List<Vector3> newforces)
    {
        this.forces = new List<SerializableVector3>();
      
        foreach (Vector3 v in newforces)
        {
            this.forces.Add(v);
        }
    }
    #endregion
}