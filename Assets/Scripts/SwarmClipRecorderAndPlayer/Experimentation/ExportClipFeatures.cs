using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ExportClipFeatures : MonoBehaviour
{
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
        "SF_31.dat",
    };

    private List<LogClip> clips = new List<LogClip>();

    private int currentClip = 0;
    // Start is called before the first frame update
    void Start()
    {
        filePath = Application.dataPath + filePath;
        Debug.Log(filePath);

        string resultFilePathCSV = Application.dataPath + "/Results/clip_features.csv";

        StringBuilder sb = new StringBuilder();

        //Prepare csv result file
        string line = "Filename;FPS;NbFrames;FractureFrame\r";
        sb.Append(line);

        //Load all clip
        for (int i = 0; i < fileNames.Length; i++)
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

        Debug.Log("Clips are loaded.");

        //Analyse part
        
        foreach(LogClip c in clips)
        {
            line = fileNames[currentClip] + ";" + c.getFps() + ";" + c.getClipFrames().Count + ";" + GetFractureFrame(c) + "\r";
            sb.Append(line);

            currentClip++;
        }

        //Save result
        File.AppendAllText(resultFilePathCSV, sb.ToString());
        sb.Clear();

        Debug.Log("Results saved.");


    }


    private int GetFractureFrame(LogClip c)
    {
        int res = -1;

        int i = 0;
        foreach(LogClipFrame f in c.getClipFrames())
        {

            List<List<LogAgentData>> clusters = ClipTools.GetClusters(f);
            if (clusters.Count>1)
            {
                res = i;
                break;
            }

            i++;
        }

        return res;
    }

}
