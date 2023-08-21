using System.Collections.Generic;

[System.Serializable]
public class SwarmClip
{
    #region Private fields
    private List<SwarmData> frames;
    private int fps = 60; //frame per second
    #endregion

    #region Methods - Constructor
    public SwarmClip(List<SwarmData> frames, int fps)
    {
        this.frames = frames;
        this.fps = fps;
    }
    #endregion

    #region Methods - Getter
    public int getFps()
    {
        return this.fps;
    }

    public List<SwarmData> getFrames()
    {
        return frames;
    }
    #endregion
}
