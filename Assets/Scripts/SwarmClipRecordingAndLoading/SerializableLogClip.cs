using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SerializableLogClip
{
    private List<SerializableLogClipFrame> clipFrames;
    private int fps; //frame per second

    public SerializableLogClip(List<LogClipFrame> clipFrames, int fps)
    {
        this.fps = fps;
        this.clipFrames = new List<SerializableLogClipFrame>();
        foreach(LogClipFrame l in clipFrames)
        {
            this.clipFrames.Add(l);
        }
    }

    public static implicit operator LogClip(SerializableLogClip rValue)
    {
        return new LogClip(rValue.getClipFrames(), rValue.getFps());
    }


    public static implicit operator SerializableLogClip(LogClip rValue)
    {
        return new SerializableLogClip(rValue.getClipFrames(), rValue.getFps());
    }

    public int getFps()
    {
        return this.fps;
    }

    public List<LogClipFrame> getClipFrames()
    {
        List<LogClipFrame> temp = new List<LogClipFrame>();
        foreach (SerializableLogClipFrame l in clipFrames)
        {
            temp.Add(l);
        }
        return temp;
    }


}
