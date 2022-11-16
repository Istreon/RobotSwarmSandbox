using System.Collections.Generic;


[System.Serializable]
public class LogClip
{
    private List<LogClipFrame> clipFrames;
    private int fps = 60; //frame per second

    private int nbAgents;


    //Map parameters
    private float mapSizeX;
    private float mapSizeZ;




    //Stocker les infos map et paramètre!!!


    public LogClip(List<LogClipFrame> clipFrames, int fps, float mapSizeX, float mapSizeZ)
    {
        this.clipFrames = clipFrames;
        this.fps = fps;
        this.mapSizeX = mapSizeX;
        this.mapSizeZ = mapSizeZ;
    }

    public int getFps()
    {
        return this.fps;
    }

    public List<LogClipFrame> getClipFrames()
    {
        return clipFrames;
    }

    public float GetMapSizeX()
    {
        return mapSizeX;
    }

    public float GetMapSizeZ()
    {
        return mapSizeZ;
    }
}

