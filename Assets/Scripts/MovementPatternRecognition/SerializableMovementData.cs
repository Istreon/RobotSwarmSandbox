using UnityEngine;

[System.Serializable]
public class SerializableMovementData
{
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
    public float speed;
    public SerializableVector3 direction;

    public SerializableMovementData(Vector3 pos, Quaternion rot, float s, Vector3 dir)
    {
        position = pos;
        rotation = rot;
        speed = s;
        direction = dir;
    }

    /// <summary>
    /// Automatic conversion from SerializableMovementData to MovementData
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator movementData(SerializableMovementData rValue)
    {
        return new movementData(rValue.position,rValue.rotation,rValue.speed,rValue.direction);
    }

    /// <summary>
    /// Automatic conversion from MovementData to SerializableMovementData
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator SerializableMovementData(movementData rValue)
    {
        return new SerializableMovementData(rValue.position, rValue.rotation, rValue.speed, rValue.direction);
    }

}
