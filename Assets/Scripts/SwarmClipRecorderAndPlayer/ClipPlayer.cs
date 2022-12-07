using System;
using UnityEngine;

public class ClipPlayer : MonoBehaviour
{ 
    #region Serialized fields
    [SerializeField]
    private GameObject actorPrefab;
    #endregion

    #region Private fields - clip parameters
    private LogClip clip;

    private int fps;
    private int nbFrames;

    #endregion

    #region Private fields - player parameters

    private FrameDisplayer frameDisplayer;

    private bool playing = false;

    private bool loopClip = false;

    private float timer = 0.0f;

    public enum DisplayType
    {
        Simple,
        ColoredCluster
    }
    private static readonly int DisplayTypeCount = Enum.GetNames(typeof(DisplayType)).Length;

    private DisplayType displayType = DisplayType.Simple;

    private int frameNumber
    {
        get { return _frameNumber; }
        set
        {
            this._frameNumber = value;
            OnFrameChanged(null);
        }
    }

    private int _frameNumber = 0;

    #endregion

    #region Event - Frame number changed
    public event EventHandler FrameChanged;

    protected virtual void OnFrameChanged(EventArgs e)
    {
        // Raise the event
        if (FrameChanged != null)
            FrameChanged(this, e);
    }
    #endregion

    #region Methods - MonoBehaviour callbacks
    // Update is called once per frame
    void Update()
    {
        if (playing)
        {
            if (timer >= (1.0f / this.fps))
            {
                DisplayFrame();
                UpdateFrameNumber();

                timer = timer - (1.0f / fps);
            }
            timer += Time.deltaTime;
        }
    }
    #endregion

    #region Methods - Clip player core methods
    /// <summary>
    /// Passes the current frame number to the next frame number.
    /// If the clip must loop, restart the clip when the clip reached its end.
    /// Else, if the clip reached is end and does not loop, stop the player.
    /// </summary>
    private void UpdateFrameNumber()
    {
        if(loopClip)
        {
            frameNumber = (frameNumber + 1) % nbFrames;
        } 
        else
        {
            if(IsClipFinished())
            {
                Pause();
            }
            else
            {
                frameNumber++;
            }
        }
    }

 

    /// <summary>
    /// This method displays the current frame of the loaded <see cref="LogClip"/>.
    /// By displaying the position of saved agents in the clip using actors.
    /// There is differents possiblities, depending on <see cref="ClipPlayer.displayType"/> value : 
    /// <see cref="DisplayType.Simple"/> : displays the frame in the simpliest way possible, meaning that all actor are the same color using <see cref="FrameDisplayer.DisplaySimpleFrame"/>.
    /// <see cref="DisplayType.ColoredCluster"/> : displays the frame coloring actors from the same clusters in an unique color, allowing an user to identify groups visually using <see cref="FrameDisplayer.DisplayColoredClusterFrame"/>.
    /// </summary>
    private void DisplayFrame()
    {
        switch(displayType)
        {
            case DisplayType.Simple:
                frameDisplayer.DisplaySimpleFrame(clip.getClipFrames()[frameNumber]);
                break;
            case DisplayType.ColoredCluster:
                frameDisplayer.DisplayColoredClusterFrame(clip.getClipFrames()[frameNumber]);
                break;
        }
    }


    /// <summary>
    /// This method intialize a clip, allowing it to be play in the right conditions.
    /// It pause the player, get the clip parameter, set the camera position according to the clip parameters, and then display the first frame (and update the UI).
    /// </summary>
    private void InitializeClip()
    {
        //Pause clip player
        Pause();

        if(this.frameDisplayer == null) frameDisplayer = new FrameDisplayer(actorPrefab);

        //Reset values for a new clip
        this.frameNumber = 0;

        //Get clip informations
        this.fps = clip.getFps();
        this.nbFrames = clip.getClipFrames().Count;

        //Camera positionning
        Camera mainCamera = FindObjectOfType<Camera>();
        mainCamera.transform.position = new Vector3(clip.GetMapSizeX() / 2.0f, Mathf.Max(clip.GetMapSizeZ(), clip.GetMapSizeX()), clip.GetMapSizeZ() / 2.0f);
        mainCamera.transform.rotation = Quaternion.Euler(90, 0, 0);

        //Udpate UI
        DisplayFrame();
    }

    /// <summary>
    /// Set the clip that will be played by this player. That method also initialise the clip.
    /// </summary>
    /// <param name="clip">
    /// The clip to play. A <see cref="NullReferenceException"/> will be raised if the value is null.
    /// </param>
    /// <exception cref="NullReferenceException"> Raised if the clip in parameter is null.</exception>
    public void SetClip(LogClip clip)
    {
        if (clip != null)
        {
            this.clip = clip;
            InitializeClip();
        }
        else
        {
            Debug.LogError("The clip set in parameter can't be null.", this);
            throw new NullReferenceException();
        }
    }
    #endregion 

    #region Methods - Clip control - Player state
    /// <summary> 
    /// Set the player to play, if there is a clip
    /// </summary>
    /// <exception cref="Exception"> If no clip is loaded in this clip player.</exception>
    public void Play()
    {
        if (clip != null)
        {
            //If the clip is not finished, play it
            if (!IsClipFinished() || this.loopClip)
            {
                playing = true;
            }
        }
        else
        {
            Debug.LogError("There is no clip in the clip player, it's impossible to change its play state.", this);
            throw new Exception();
        }
    }

    /// <summary> 
    /// Pause the clip player, if there is a clip
    /// </summary>
    /// <exception cref="Exception"> If no clip is loaded in this clip player.</exception>
    public void Pause()
    {
        if (clip != null)
        {
            playing = false;
        }
        else
        {
            Debug.LogError("There is no clip in the clip player, it's impossible to change its play state.", this);
            throw new Exception();
        }
    }

    /// <summary>
    /// Set the loop mode of the player. 
    /// If <paramref name="looping"/> at true, the clip will be played on a loop. 
    /// At false, the clip will be played once.
    /// </summary>
    /// <param name="looping"> True : clip will be played on a loop. False : clip will be played once. </param>
    public void SetLoopMode(bool looping)
    {
        this.loopClip = looping;
    }
    #endregion

    #region Methods - Clip control - Display
    public DisplayType NextDisplayType()
    {

        int i = (int)displayType;
        i = (i + 1) % DisplayTypeCount;
        displayType = (DisplayType)i;
        DisplayFrame();

        return displayType;
    }

    /// <summary>
    /// Set the display type. Several possibilities are defined by <see cref="DisplayType"/>. 
    /// See <see cref="DisplayFrame"/> method for more informations.
    /// </summary>
    /// <param name="type"> The visualisation mode to display.</param>
    public void SetDisplayMode(DisplayType type)
    {
        this.displayType = type;
        DisplayFrame();
    }
    #endregion

    #region Methods - Clip control - Frame
    /// <summary>
    /// Change the current frame of the clip and display it. The clip will continue from this frame. 
    /// </summary>
    /// <param name="value"> 
    /// A <see cref="float"/> value representing a percentage of the clip. 
    /// This value must be between 0.0f and 1.0f. 1.0f correspond the end of the clip and 0.0f to its start.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException"> If <paramref name="value"/> doesn't respect the allowed range [0.0f, 1.0f]. </exception>
    public void SetClipAtPercentage(float value)
    {
        if (value >= 0.0f && value <= 1.0f)
        {
            this.frameNumber = (int)((this.nbFrames - 1) * value);
            DisplayFrame();
        }
        else
        {
            Debug.LogError("The value set in parameter is out of range (only [0.0f,1.0f] value allowed).", this);
            throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Change the current frame of the clip and display it. The clip will continue from this frame. 
    /// </summary>
    /// <param name="number"> 
    /// A <see cref="int"/> value corresponding to the wanted frame. 
    /// If the value is less than 0 or greater or equal to the number of frame, an <see cref="ArgumentOutOfRangeException"/> will be raised.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException"> If no clip is loaded in this clip player.</exception>
    public void SetFrameNumber(int number)
    {
        if (number >= 0 && number < this.nbFrames)
        {
            this.frameNumber = number;
            DisplayFrame();
        }
        else
        {
            Debug.LogError("The frame number set in parameter is out of range.", this);
            throw new ArgumentOutOfRangeException();
        }
    }
    #endregion

    #region Methods - Getter
    public int GetFrameNumber()
    {
        return this.frameNumber;
    }

    public bool IsPlaying()
    {
        return playing;
    }

    public bool IsClipFinished()
    {
        if (frameNumber >= (nbFrames - 1))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public float GetClipAdvancement()
    {
        float res = (float)this.frameNumber / (float)(this.nbFrames - 1);
        return res;
    }
    #endregion

}
