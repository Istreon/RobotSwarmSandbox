using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;

[RequireComponent(typeof(ClipPlayer))]
public class SwarmClipExperimentationPlayer : MonoBehaviour
{
    #region Serialize fields
    [SerializeField]
    private GameObject nextClipMenu;

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private TMP_Text feedback;
    #endregion

    #region Private fields
    //string filePath = "C:/Users/hmaym/OneDrive/Bureau/UnityBuild/Clip/"; //The folder containing clip files
    string filePath = "/Clips/"; //The folder containing clip files
    

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
        "F_19.dat",
        "F_20.dat",
        "F_21.dat",
        "F_22.dat",
        "F_23.dat",
        "F_24.dat",
        "F_25.dat",
        "F_26.dat",
        "F_27.dat",
        "F_28.dat",
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
        "SF_24.dat",
        "SF_25.dat",
        "SF_26.dat",
        "SF_27.dat",
        "SF_28.dat",
        "SF_29.dat",
        "SF_30.dat",
    };


    private List<LogClip> clips = new List<LogClip>();

    private int currentClip = 0;

    private ClipPlayer clipPlayer;

    private ExperimentationResult experimentationResult = new ExperimentationResult();

    string resultFilePathCSV = "";
    string resultFilePathDat = "";

    private bool answered = false;

    private bool resultSaved = false;

    Thread backgroundThread;

    StringBuilder sb = new StringBuilder();
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        filePath = Application.dataPath + filePath;
        Debug.Log(filePath);
        string date = System.DateTime.Now.ToString("yyyyMMddHHmmss");
        string resultFilename = "/" + "resultXP_" + date;
        resultFilePathDat = Application.dataPath + "/Results" + resultFilename + ".dat";
        resultFilePathCSV = Application.dataPath + "/Results" + resultFilename + ".csv";

        //Prepare csv result file
        string line = "Filename,Framenumber,Result\r";
        sb.Append(line);

        slider.gameObject.SetActive(false);

        clipPlayer = FindObjectOfType<ClipPlayer>();




        //Shuffle list
        var rnd = new System.Random();
        List<string> l = fileNames.ToList();
        l = l.OrderBy(item => rnd.Next()).ToList<string>();
        fileNames = l.ToArray();

        LoadFirstClips();
        backgroundThread = new Thread(new ThreadStart(LoadOtherClips));
        // Start thread loading the other clips
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
        if (clipPlayer.IsClipFinished())
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
                slider.gameObject.SetActive(true);
                answered = false;
                feedback.text = "";
            }
        }     
    }


    //Choice = true   => fracture
    public void GiveAnswer(bool choice)
    {   if(!answered)
        {
            feedback.text = choice ? "You answered \"Fracture\"" : "You answered \"No fracture\"";
            ClipResult res = new ClipResult(fileNames[currentClip], choice, clipPlayer.GetFrameNumber());
            Debug.Log(res.filename + "    " + res.fracture + "   " + res.frameNumber);
            experimentationResult.AddClipResult(res);
        }
        answered = true;
    }


    public void SaveResult()
    {
        //Save result : format .dat
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(resultFilePathDat, FileMode.OpenOrCreate);
        bf.Serialize(file, experimentationResult);
        file.Close();


        //Save result : format .csv
        foreach (ClipResult cr in experimentationResult.results)
        {
            string line;
            line = cr.filename + "," + cr.frameNumber + "," + cr.fracture + "\r";
            sb.Append(line);
        }

        File.AppendAllText(resultFilePathCSV, sb.ToString());
        sb.Clear();

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
        }
    }

}
