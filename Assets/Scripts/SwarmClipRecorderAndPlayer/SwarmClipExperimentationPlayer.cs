using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ClipPlayer))]
public class SwarmClipExperimentationPlayer : MonoBehaviour
{

    #region Private fields
    string filePath = "C:/Users/hmaym/OneDrive/Bureau/UnityBuild/Clip/"; //The folder containing clip files
    string[] fileNames = {
        "clip_20221121140352.dat",
        "clip_20221122101754.dat",
        "clip_20221121140545.dat",
        "clip_20221121140431.dat"
    };

    private List<LogClip> clips = new List<LogClip>();

    private int currentClip = 0;

    private ClipPlayer clipPlayer;

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        
        clipPlayer = FindObjectOfType<ClipPlayer>();

        //Load all clip
        foreach (string fname in fileNames)
        {
            //Concatenation of file path and file name
            string s = this.filePath + fname;

            //Loading clip from full file path
            LogClip clip = ClipTools.LoadClip(s);

            if (clip != null)
                clips.Add(clip); //Add the loaded clip to the list
            else
                Debug.LogError("Clip can't be load from " + s,this);

            //TO DO !!! (ajouter un visuel d'avancement du chargement en cas de nombreux clip, car cela peut être long)
        }
        if (clips.Count > 0)
        {
            Debug.Log("Loading complete, " + clips.Count + " clips have been loaded");

            clipPlayer.SetClip(clips[0]);
            clipPlayer.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if (clipPlayer.IsClipFinished())
        {
            if(currentClip > clips.Count - 1)
            {
                Debug.Log("Experimentation finished");
            } else
            {
                currentClip++;
                clipPlayer.SetClip(clips[currentClip]);
                Debug.Log("Next clip" + (currentClip + 1));
                clipPlayer.Play();
            }
        }
    }

}
