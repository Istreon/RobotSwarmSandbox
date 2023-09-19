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
    private ClipPlayer clipPlayer;

    [SerializeField]
    private GameObject answerMenu;

    [SerializeField]
    private GameObject startingMenu;

    [SerializeField]
    private GameObject finishMenu;

    [SerializeField]
    private Displayer defaultVisualisation;

    [SerializeField]
    private List<Displayer> testedVisualisation;
    #endregion

    #region Private fields
    //--Clip loading--//
    string filePath = "/Clips/"; //The folder containing clip files

    string[] filePaths;

    Thread backgroundThread;

    List<Tuple<int, int>> experimentalConditions; //Tuple<idClip, idVisu> = an experimental condition, with a clip and the associated visualisation

    List<int> loadOrder;

    private SwarmClip[] clips;


    //--Clip player--//

    private int currentCondition = 0;

    //--Results--//
    private Exp2AnticipationResult experimentationResult = new Exp2AnticipationResult();

    string resultFilePathCSV = "";
    string resultFilePathDat = "";

    private bool answered = false;

    private bool resultSaved = false;

    StringBuilder sb = new StringBuilder();
    #endregion


    int totalCount = 0;

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

        if (clipPlayer == null)
        {
            Debug.LogError("There is no ClipPlayer in the scene.", this);
        }

        //Cr�er une liste d'identifiant qui font r�f�rences aux clips qui vont �tre charg�s
        
        experimentalConditions = new List<Tuple<int, int>>();
        for(int v =0; v < testedVisualisation.Count +1; v++)
        {
            for (int c = 0; c < filePaths.Length; c++)
            {
                experimentalConditions.Add(new Tuple<int, int>(c, v));
            }
        }


        //Shuffle the list of clip files
        var rnd = new System.Random();
        experimentalConditions = experimentalConditions.OrderBy(item => rnd.Next()).ToList<Tuple<int,int>>();


        //Obtenir l'ordre de chargement des clips
        this.loadOrder = new List<int>();
        foreach (Tuple<int,int> cond in experimentalConditions)
        {
            int clipId = cond.Item1;
            if(!loadOrder.Contains(clipId))
            {
                loadOrder.Add(clipId);
            }
        }


        clips = new SwarmClip[filePaths.Length];


        //Load the first clip before continue
        bool succes = LoadFirstClips();
        
        //Prepare the thread that will load the other clips
        backgroundThread = new Thread(new ThreadStart(LoadOtherClips));

        //�Start�thread�loading the other clips
        backgroundThread.Start();

 
        if (succes)
        {
            currentCondition = -1;
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
        if (clipPlayer.IsClipFinished() && currentCondition != -1) //If the current clip ended
        {
            //Check if the experimentation is ended to save the results
            if (currentCondition >= experimentalConditions.Count - 1 && !resultSaved)
            {
                //Display finish menu
                finishMenu.SetActive(true);
                SaveResult();
                Debug.Log("Total count = " + totalCount);
            } else
            {
                //Display answering menu
                answerMenu.SetActive(true);
            }
        }
        else
        {
            answerMenu.SetActive(false);
        }
    }

    public void StartExperimentation()
    {
        if (currentCondition == -1) //If no clip was display
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

            if (currentCondition >= experimentalConditions.Count - 1)
            {
                Debug.Log("Experimentation finished");
            }
            else
            {
                currentCondition++;
                clipPlayer.SetClip(clips[experimentalConditions[currentCondition].Item1]);
                Debug.Log("Next clip : " + (experimentalConditions[currentCondition].Item1) + " and visu number : "+ (experimentalConditions[currentCondition].Item2));

                List<Displayer> newDisplay = new List<Displayer>();
                newDisplay.Add(defaultVisualisation);
                if(experimentalConditions[currentCondition].Item2!=0)
                {
                newDisplay.Add(testedVisualisation[experimentalConditions[currentCondition].Item2-1]);
                }
                clipPlayer.SetUsedDisplayers(newDisplay);
                clipPlayer.Play();
                answered = false;
                totalCount++;
            }
        
    }


    //Choice = true   => fracture
    public void GiveAnswer(bool choice)
    {
        if (!answered && clipPlayer.IsClipFinished())
        {
            //Get file name from file path
            string s = filePaths[experimentalConditions[currentCondition].Item1];
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

    public bool LoadFirstClips()
    {

        string s = filePaths[loadOrder[0]];

        //Loading clip from full file path
        SwarmClip clip = ClipTools.LoadClip(s);

        if (clip != null)
        {
            clips[loadOrder[0]]= clip; //Add the loaded clip to the list
            Debug.Log("Clip " + s + " Loaded.");
            return true;
        }
        else
        {
            Debug.LogError("Clip can't be load from " + s, this);
            return false;
        }


    }

    public void LoadOtherClips()
    {
        //Load all clip
        for (int i = 1; i < filePaths.Length; i++)
        {
            //Concatenation of file path and file name
            string s = this.filePaths[loadOrder[i]];

            //Loading clip from full file path
            SwarmClip clip = ClipTools.LoadClip(s);

            if (clip != null)
            {
                clips[loadOrder[i]] = clip; //Add the loaded clip to the list
                Debug.Log("Clip " + s + " Loaded.");
            }
            else
                Debug.LogError("Clip can't be load from " + s, this);
        }
    }

    #endregion
}
