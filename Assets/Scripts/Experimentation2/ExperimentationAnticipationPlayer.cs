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

public class ExperimentationAnticipationPlayer : MonoBehaviour
{
    #region Serialize fields
    [SerializeField]
    private GameObject answerMenu;

    [SerializeField]
    private GameObject startingMenu;
    #endregion

    #region Private fields
    //--Clip loading--//
    string filePath = "/Clips/"; //The folder containing clip files

    string[] filePaths;

    private List<SwarmClip> clips = new List<SwarmClip>();

    Thread backgroundThread;

    //--Clip player--//

    private int currentClip = 0;

    private ClipPlayer clipPlayer;

    //--Results--//
    private Exp2AnticipationResult experimentationResult = new Exp2AnticipationResult();

    string resultFilePathCSV = "";
    string resultFilePathDat = "";

    private bool answered = false;

    private bool resultSaved = false;

    StringBuilder sb = new StringBuilder();
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        //Disable gameObject
        answerMenu.SetActive(false); //Answering menu

        //Enable gameObject
        startingMenu.SetActive(true);

        //Get the path of the clip files
        filePath = Application.dataPath + filePath;
        Debug.Log(filePath);

        //Get the files path from the clip folder
        filePaths = Directory.GetFiles(filePath, "*.dat",
                                         SearchOption.TopDirectoryOnly);

        //Prepare the name of the result files
        string date = System.DateTime.Now.ToString("yyyyMMddHHmmss"); 
        string resultFilename = "/" + "resultXP_" + date; //it uses the date to obtain a unique name
        string resultFolderPath = Application.dataPath + "/Results";
        resultFilePathDat = resultFolderPath + resultFilename + ".dat";
        resultFilePathCSV = resultFolderPath + resultFilename + ".csv";

        //Check if the result folder exists. If not, create one
        if (!Directory.Exists(resultFolderPath))
        {
            Directory.CreateDirectory(resultFolderPath);
        }

        //Prepare csv result file
        string line = "Filename,Result\r";
        sb.Append(line);

        //Find the clip player in the scene
        clipPlayer = FindObjectOfType<ClipPlayer>();

        //Shuffle the list of clip files
        var rnd = new System.Random();
        List<string> l = filePaths.ToList();
        l = l.OrderBy(item => rnd.Next()).ToList<string>();
        filePaths = l.ToArray();


        //Load the first clip before continue
        LoadFirstClips();
        
        //Prepare the thread that will load the other clips
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
        if (clipPlayer.IsClipFinished() && currentClip !=-1) //If the current clip ended
        {
            //Display answering menu
            answerMenu.SetActive(true);

            //Check if the experimentation is ended to save the results
            if (currentClip >= clips.Count - 1 && !resultSaved)
            {
                answerMenu.GetComponentInChildren<TMP_Text>().text = "Finish";
                SaveResult();
            }
        }
        else
        {
            answerMenu.SetActive(false);
        }
    }

    public void StartExperimentation()
    {
        if (currentClip == -1) //If no clip was display
        {
            //Disable starting menu
            startingMenu.SetActive(false);

            //Launch the first clip
            NextClip();
        } else
        {
            Debug.LogError("Can't start the experiment.",this);
        }
    }

    private void NextClip()
    {

            if (currentClip >= clips.Count - 1)
            {
                Debug.Log("Experimentation finished");
            }
            else
            {
                currentClip++;
                clipPlayer.SetClip(clips[currentClip]);
                Debug.Log("Next clip " + (currentClip + 1));
                clipPlayer.Play();
                answered = false;
            }
        
    }


    //Choice = true   => fracture
    public void GiveAnswer(bool choice)
    {
        if (!answered && clipPlayer.IsClipFinished())
        {
            //Get file name from file path
            string s = filePaths[currentClip];
            int pos = s.IndexOf("/");
            while (pos != -1)
            {
                s = s.Substring(pos + 1);
                pos = s.IndexOf("/");
            }

            Exp2AnticipationAnswer res = new Exp2AnticipationAnswer(s, choice);
            Debug.Log(res.filename + "    " + res.fracture);
            experimentationResult.AddClipResult(res);
        }
        answered = true;
        NextClip();
    }


    public void SaveResult()
    {
        //Save result : format .dat
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(resultFilePathDat, FileMode.OpenOrCreate);
        bf.Serialize(file, experimentationResult);
        file.Close();


        //Save result : format .csv
        foreach (Exp2AnticipationAnswer cr in experimentationResult.results)
        {
            string line;
            line = cr.filename +  "," + cr.fracture + "\r";
            sb.Append(line);
        }

        File.AppendAllText(resultFilePathCSV, sb.ToString());
        sb.Clear();

        resultSaved = true;
        Debug.Log("Results saved.");
    }

    #region Methods - Load clips

    public void LoadFirstClips()
    {

        string s = filePaths[0];

        //Loading clip from full file path
        SwarmClip clip = ClipTools.LoadClip(s);

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
        for (int i = 1; i < filePaths.Length; i++)
        {
            //Concatenation of file path and file name
            string s = this.filePaths[i];

            //Loading clip from full file path
            SwarmClip clip = ClipTools.LoadClip(s);

            if (clip != null)
            {
                clips.Add(clip); //Add the loaded clip to the list
                Debug.Log("Clip " + s + " Loaded.");
            }
            else
                Debug.LogError("Clip can't be load from " + s, this);
        }
    }

    #endregion
}
