using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableLogAgentData
{
    private SerializableVector3 position;
    private SerializableVector3 speed;

    public SerializableLogAgentData(Vector3 position, Vector3 speed)
    {
        this.position = position;
        this.speed = speed;
    }

    public static implicit operator LogAgentData(SerializableLogAgentData rValue)
    {
        return new LogAgentData(rValue.getPosition(), rValue.getSpeed());
    }


    public static implicit operator SerializableLogAgentData(LogAgentData rValue)
    {
        return new SerializableLogAgentData(rValue.getPosition(), rValue.getSpeed());
    }

    public SerializableVector3 getPosition()
    {
        return this.position;
    }

    public SerializableVector3 getSpeed()
    {
        return this.speed;
    }
}
