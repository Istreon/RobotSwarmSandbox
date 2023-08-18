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

    #region Methods
    public void UdpateAcceleration()
    {
        this.acceleration = Vector3.zero;
        foreach(Vector3 f in forces)
        {
            this.acceleration += f;
        }
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
    #endregion
}
