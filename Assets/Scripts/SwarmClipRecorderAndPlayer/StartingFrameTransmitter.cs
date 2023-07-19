using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameTransmitter : MonoBehaviour
{
    private LogClipFrame frame;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        this.gameObject.name = "FrameTransmitter";
    }

    public void SetFrame(LogClipFrame frame)
    {
        this.frame = frame;
    }

    public LogClipFrame GetFrame()
    {
        return this.frame;
    }

    public LogClipFrame GetFrameAndDestroy()
    {
        Destroy(this.gameObject);
        return GetFrame();
    }
}
