using System.Collections.Generic;

[System.Serializable]
public class LogClip
{
    #region Private fields
    private List<LogClipFrame> clipFrames;
    private int fps = 60; //frame per second

    //Map parameters
    private float mapSizeX;
    private float mapSizeZ;
    #endregion

    #region Methods - Constructor
    public LogClip(List<LogClipFrame> clipFrames, int fps, float mapSizeX, float mapSizeZ)
    {
        this.clipFrames = clipFrames;
        this.fps = fps;
        this.mapSizeX = mapSizeX;
        this.mapSizeZ = mapSizeZ;
    }
    #endregion

    #region Methods - Getter
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
    #endregion
}

