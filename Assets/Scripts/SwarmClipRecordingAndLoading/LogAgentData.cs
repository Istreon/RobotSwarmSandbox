using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LogAgentData 
{
    private Vector3 position;
    private Vector3 speed;

    public LogAgentData(Vector3 position, Vector3 speed)
    {
        this.position = position;
        this.speed = speed;
    }

    public Vector3 getPosition()
    {
        return this.position;
    }

    public Vector3 getSpeed()
    {
        return this.speed;
    }
}
