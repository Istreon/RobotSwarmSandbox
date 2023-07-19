using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingFrameTransmitter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        this.gameObject.name = "FrameTransmitter";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
