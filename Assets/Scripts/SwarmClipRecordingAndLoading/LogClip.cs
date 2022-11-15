using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogClip
{
    private List<LogClipFrame> clipFrames;
    private int fps = 60; //frame per second



    public LogClip(List<LogClipFrame> clipFrames, int fps)
    {
        this.clipFrames = clipFrames;
        this.fps = fps;
    }

    public int getFps()
    {
        return this.fps;
    }

    public List<LogClipFrame> getClipFrames()
    {
        return clipFrames;
    }
}
