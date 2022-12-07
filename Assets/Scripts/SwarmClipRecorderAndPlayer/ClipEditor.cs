using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SFB;
using System;

[RequireComponent(typeof(ClipPlayer))]
public class ClipEditor : MonoBehaviour
{
    #region Serialized fields

    [SerializeField]
    private GameObject buttonsIfClipLoaded;

    [SerializeField]
    private GameObject saveButton;

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private Button playButton;

    #endregion

    #region Private fields - clip parameters
    

    private LogClip clip;

    private string filePath = "";
    #endregion

    #region Private fields - clip editor parameters
    private ClipPlayer clipPlayer;

    private bool loaded = false;
    private bool sliderValueChanged = false;
    private bool modifiedClip = false;

    #endregion

    #region Methods - MonoBehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {
        clipPlayer = FindObjectOfType<ClipPlayer>();
        if(clipPlayer == null)
        {
            Debug.LogError("There is no ClipPlayer in the scene.", this);
        }

        //Set clip player in loop mode
        clipPlayer.SetLoopMode(true);

        //Get value change (current frame number) of clip player event 
        clipPlayer.FrameChanged += UpdateSliderValue;
    }

    // Update is called once per frame
    void Update()
    {
        //Display or hide UI button
        buttonsIfClipLoaded.SetActive(loaded);
        saveButton.SetActive(modifiedClip);
    }
    #endregion

    #region Methods - Clip player methods

    /// <summary>
    /// This method udpates the visual state of the slider in the UI.
    /// In doing so, it prevent the call of the "on slider change" by setting a bool value to true.
    /// </summary>
    private void UpdateSliderValue(object sender, EventArgs e)
    {
        sliderValueChanged = true;
        slider.value = clipPlayer.GetClipAdvancement();
    }

    /// <summary>
    /// This methods allow to select a specific frame moment in the clip.
    /// This selection is based on the slider value  (set in parameter in the editor).
    /// Reload the display to show the actual frame of the clip.
    /// </summary>
    public void SelectFrame()
    {
        if (!sliderValueChanged)
        {
            clipPlayer.SetClipAtPercentage(slider.value);
        }
        sliderValueChanged = false;
    }

    /// <summary> 
    /// This method reverses the state of the clip player.
    /// If the clip player was playing, then this method changes it state to pause.
    /// Else, if the clip player was paused, this method changes it state to play
    /// </summary>
    public void PlayOrPauseClip()
    {
        if (loaded)
        {
            if (clipPlayer.IsPlaying()) 
            {
                clipPlayer.Pause();
            }
            else
            {
                clipPlayer.Play();
            }  
            playButton.GetComponentInChildren<TMP_Text>().text = clipPlayer.IsPlaying() ? "Pause" : "Play";
        }
    }

    /// <summary>
    /// Change the visualisation type of the clip. 
    /// Each call of this methods will pass the visualisation to the next one as define in <see cref="ClipPlayer"/>.
    /// </summary>
    public void ChangeVisualization()
    {
        clipPlayer.NextDisplayType();
    }

    #endregion

    #region Methods - Clip editor

    /// <summary>
    /// This method allows to remove a part of the clip, to reduce it size. The removed part is lost"/>
    /// </summary>
    /// <param name="firstPart">
    /// A <see cref="bool"/> value that decides whether the first or the last part of the clip which is deleted. 
    /// True, the first part is deleted. 
    /// False, the last part is deleted.
    /// </param>
    public void RemoveClipPart(bool firstPart)
    {
        //If there are enough frame to cut the clip
        if (clip.getClipFrames().Count > 2)
        {
            if (firstPart) //Remove the first part of the clip
            {
                clip.getClipFrames().RemoveRange(0,clipPlayer.GetFrameNumber());
                clipPlayer.SetClip(clip);
                clipPlayer.SetFrameNumber(0);
                
            }
            else //Remove the last part of the clip
            {
                clip.getClipFrames().RemoveRange(clipPlayer.GetFrameNumber(), clip.getClipFrames().Count - clipPlayer.GetFrameNumber());
                clipPlayer.SetClip(clip);
                clipPlayer.SetFrameNumber(clip.getClipFrames().Count - 1);
            }
            modifiedClip = true;
        }
    }

    /// <summary>
    /// Save a modified clip, under the original filename adding "_mod", in the same folder as the original clip. 
    /// A clip can't be saved using this method if it wasn't modified.
    /// </summary>
    public void SaveUpdatedClip()
    {
        if (modifiedClip)
        {
            int pos = filePath.LastIndexOf('.');
            string newFilePath = filePath.Remove(pos);
            newFilePath += "_mod.dat";
            Debug.Log(newFilePath);
            SwarmClipTools.SaveClip(clip, newFilePath);
        }
    }





    /// <summary>
    /// Open a windows explorer to select a clip and load it.
    /// Allowed files have .dat extension.
    /// Once the clip is load, it's intialized.
    /// </summary>
    public void ShowExplorer()
    {
        var extensions = new[] {
        new ExtensionFilter("Data files", "dat" ),
        };
        var paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, true);
        filePath = paths[0];

        if (filePath != string.Empty)
        {
            clip = SwarmClipTools.LoadClip(filePath);
            loaded = (clip != null);

            if (loaded)
            {
                Debug.Log("Clip loaded");
                clipPlayer.SetClip(clip);
            }
        }
    }

    public void ExitApp()
    {
        Application.Quit();
    }
    #endregion
}