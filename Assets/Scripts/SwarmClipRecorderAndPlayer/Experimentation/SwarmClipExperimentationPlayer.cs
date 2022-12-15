using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ClipPlayer))]
public class SwarmClipExperimentationPlayer : MonoBehaviour
{
    #region Serialize fields
    [SerializeField]
    private GameObject nextClipMenu;

    [SerializeField]
    private GameObject fractureButton;

    [SerializeField]
    private GameObject noFractureButton;

    [SerializeField]
    private Slider slider;
    #endregion

    #region Private fields
    string filePath = "C:/Users/hmaym/OneDrive/Bureau/UnityBuild/Clip/"; //The folder containing clip files
    string[] fileNames = {
        "F_1.dat",
        "F_2.dat",
        "F_3.dat",
        "F_4.dat",
        "F_5.dat",
        "F_6.dat",
        "F_7.dat",
        "F_8.dat",
        "F_9.dat",
        "F_10.dat",
        "F_11.dat",
        "F_12.dat",
        "F_13.dat",
        "F_14.dat",
        "F_15.dat",
        "F_16.dat",
        "F_17.dat",
        "F_18.dat",
        "SF_1.dat",
        "SF_2.dat",
        "SF_3.dat",
        "SF_4.dat",
        "SF_5.dat",
        "SF_6.dat",
        "SF_7.dat",
        "SF_8.dat",
        "SF_9.dat",
        "SF_10.dat",
        "SF_11.dat",
        "SF_12.dat",
        "SF_13.dat",
        "SF_14.dat",
        "SF_15.dat",
        "SF_16.dat",
        "SF_17.dat",
        "SF_18.dat",
        "SF_19.dat",
        "SF_20.dat",
        "SF_21.dat",
        "SF_22.dat",
        "SF_23.dat",
        "SF_24.dat"
    };



    private List<LogClip> clips = new List<LogClip>();

    private int currentClip = 0;

    private ClipPlayer clipPlayer;

    private ExperimentationResult experimentationResult = new ExperimentationResult();

    string resultFilePath = "";

    private bool answered = false;

    private bool resultSaved = false;

    Thread backgroundThread;
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        string date = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        string resultFilename = "/" + "resultXP_" + date + ".dat";
        resultFilePath = Application.persistentDataPath + resultFilename;

        fractureButton.SetActive(false);
        noFractureButton.SetActive(false);
        slider.gameObject.SetActive(false);

        clipPlayer = FindObjectOfType<ClipPlayer>();

        
        //Shuffle list
        var rnd = new System.Random();
        List<string> l = fileNames.ToList();
        l = l.OrderBy(item => rnd.Next()).ToList<string>();
        fileNames = l.ToArray();

        LoadFirstClips();
        backgroundThread = new Thread(new ThreadStart(LoadOtherClips));
        // Start thread  
        backgroundThread.Start();

       
        if (clips.Count > 0)
        {
           currentClip = -1;
        }
    }

    private void OnDisable()
    {
        backgroundThread.Abort();
        Debug.Log("Thread is abort.");
    }

    // Update is called once per frame
    void Update()
    {
        slider.value = clipPlayer.GetClipAdvancement();

        if (clipPlayer.IsClipFinished() || currentClip == -1)
        {
            //Display interclip menu and hide slider
            nextClipMenu.SetActive(true);
            slider.gameObject.SetActive(false);

            //Check if a response has been given
            if(!answered && !(currentClip ==-1))
            {
                GiveAnswer(false);
                answered = true;
            }

            //Check if the experimentation is ended to save the results
            if(currentClip >= clips.Count - 1 && !resultSaved)
            {
                nextClipMenu.GetComponentInChildren<TMP_Text>().text = "Finish";
                SaveResult();
                
            }
        }
        else
        {
            nextClipMenu.SetActive(false);
        }
    }

    public void NextClip()
    {
        if (currentClip >= clips.Count - 1)
        {
            Debug.Log("Experimentation finished");
        }
        else
        {
            currentClip++;
            clipPlayer.SetClip(clips[currentClip]);
            Debug.Log("Next clip" + (currentClip + 1));
            clipPlayer.Play();
            fractureButton.SetActive(true);
            noFractureButton.SetActive(true);
            slider.gameObject.SetActive(true);
            answered = false;
        }
    }


    public void GiveAnswer(bool choice)
    {
        fractureButton.SetActive(false);
        noFractureButton.SetActive(false);
        ClipResult res = new ClipResult(fileNames[currentClip], choice, clipPlayer.GetFrameNumber());
        Debug.Log(res.filename + "    " + res.fracture + "   " + res.frameNumber);
        experimentationResult.AddClipResult(res);
        answered = true;
    }


    public void SaveResult()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(resultFilePath, FileMode.OpenOrCreate);
        bf.Serialize(file, experimentationResult);
        file.Close();
        resultSaved = true;
        Debug.Log("Results saved.");
    }


    public void LoadFirstClips()
    {

        string fname = fileNames[0];

        //Concatenation of file path and file name
        string s = this.filePath + fname;

        //Loading clip from full file path
        LogClip clip = ClipTools.LoadClip(s);

        if (clip != null)
        {
            clips.Add(clip); //Add the loaded clip to the list
            Debug.Log("Clip " + s + " Loaded.");
        }
        else
            Debug.LogError("Clip can't be load from " + s, this);

    }

    public void LoadOtherClips()
    {
        //Load all clip
        for (int i = 1; i< fileNames.Length; i++)
        {
            string fname = fileNames[i];

            //Concatenation of file path and file name
            string s = this.filePath + fname;

            //Loading clip from full file path
            LogClip clip = ClipTools.LoadClip(s);

            if (clip != null)
            {
                clips.Add(clip); //Add the loaded clip to the list
                Debug.Log("Clip " + s + " Loaded.");
            }
            else
                Debug.LogError("Clip can't be load from " + s, this);

            //TO DO !!! (ajouter un visuel d'avancement du chargement en cas de nombreux clip, car cela peut être long)
        }
    }

}
