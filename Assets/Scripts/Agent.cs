using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    protected void InitializeAgent(bool isKinematic)
    {
        Rigidbody temp = this.gameObject.AddComponent<Rigidbody>();
        temp.isKinematic = isKinematic;
        if(!temp.isKinematic)
        {
            temp.constraints = RigidbodyConstraints.FreezePositionY  | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ; 
        }
    }
}
