using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class movementDatasetStorage : MonoBehaviour
{
    [SerializeField]
    private string movementName = "";

    int minPatternSize = 50;

    private List<movementDataset> movementDatasets;



    /// <summary>
    /// Those parameters must be change depending of the movement
    /// </summary>
    [SerializeField]
    private float distanceErrorSensibility = 0.1f; //0.015
    [SerializeField]
    private float speedErrorSensibility = 0.1f;  //0.02 
    // Start is called before the first frame update
    void Start()
    {
        movementDatasets = new List<movementDataset>();


        //////////////////////////////////////////////
        ///Try to load movementdatasets registered in a file
        /////////////////////////////////////////////
        ///
        if (Load()) Debug.Log("Existing data ... loaded ");
        else Debug.Log("Unexisting data ... ");

        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public string getMovementName()
    {
        return movementName;
    }


    public int getReferenceDataSetSize()
    {
        if (movementDatasets.Count == 0) return 0;
        return minPatternSize;
    }

    public void addNewDataSet(List<movementData> movementDataList)
    {
        if (movementDatasets.Count == 0)
        {
            minPatternSize = movementDataList.Count;
        }
        else
        {
            minPatternSize = Mathf.Min(movementDataList.Count, minPatternSize);
        }
        movementDataset temp = new movementDataset(movementDataList);
        movementDatasets.Add(temp);

        //////////////////////////////////////////////
        ///Register movementdatasets in a file
        /////////////////////////////////////////////
        Save();
    }


    public bool checkDatasets(List<movementData> newDataset)
    {
        foreach (movementDataset l in movementDatasets)
        {
            if (compareDatasets(newDataset, l)) return true;
        }
        return false;
    }

    private bool compareDatasets(List<movementData> newDataset, movementDataset referenceDataset)
    {

        if (newDataset.Count < minPatternSize)
        {
            Debug.Log("Data set too short");
            return false;
        }

        int nbAnalysis = Mathf.Min(newDataset.Count, referenceDataset.movementDataList.Count);


        movementData temp = movementData.getAverageMovementDataBasedOnSpeed(newDataset);

        float angleAverage = Vector3.Angle(temp.direction, referenceDataset.averageMovementData.direction);
        float speedAverage = Mathf.Abs(temp.speed - referenceDataset.averageMovementData.speed);

        if (speedAverage > speedErrorSensibility) return false;

        List<movementData> l1 = newDataset;
        List<movementData> l2 = referenceDataset.movementDataList;

        if (referenceDataset.movementDataList.Count < newDataset.Count)
        {
            l1 = referenceDataset.movementDataList;
            l2 = newDataset;
        }

        float bestRate = float.MaxValue;

        float bestMaxDist = 0;
        int bestNbError = 0;
        float bestMaxAngle = 0;
        float bestAngleRate = float.MaxValue;

        int offSet = l1.Count - minPatternSize;

        for (int k = 0; k < nbAnalysis; k++)
        {
            int l1Start = (offSet < 0) ? 0 : offSet;
            int l2Start = (-offSet < 0) ? 0 : -offSet;
            int analysisSize = Mathf.Min(l1.Count - l1Start, l2.Count - l2Start);
            float totalRate = 0;
            float maxDist = 0f;
            int nbError = 0;
            float totalAngle = 0;
            float maxAngle = 0;
            for (int i = 0; i < analysisSize; i++)
            {


                //Comparer vNew et vRef
                float angle = Vector3.Angle(l1[i + l1Start].position, l2[i + l2Start].position);
                float dist = Mathf.Abs(Vector3.Distance(l1[i + l1Start].position, l2[i + l2Start].position));
                if (angle > maxAngle) maxAngle = angle;
                if (dist > maxDist) maxDist = dist;
                if (dist > distanceErrorSensibility) nbError++;
                //additionner le pourcentage à une somme
                totalRate += dist;

                totalAngle += angle;
            }

            //calculer la moyenne des pourcentages d'erreur
            totalRate = totalRate / (float)analysisSize;
            totalAngle = totalAngle / (float)analysisSize;
            //si moins d'erreur (plus faible), 
            if (totalRate < bestRate)
            {

                //sauvegarder la moyenne d'erreur
                bestRate = totalRate;
                bestMaxDist = maxDist;
                bestNbError = nbError;
                bestMaxAngle = maxAngle;
                bestAngleRate = totalAngle;
            }


            offSet--;
        }





        if (bestRate <= 0.1 && speedAverage <= 0.09)
        {
            Debug.Log("Average ::: angle : " + angleAverage + "  speed : " + speedAverage);
            Debug.Log("Rate: " + bestRate + " ----  Max error : " + bestMaxDist + " ----  " + "Nb error : " + bestNbError + " Angle rate : " + bestAngleRate + " Max angle : " + bestMaxAngle);
        }

        return (bestRate <= distanceErrorSensibility);
    }


    private void Save()
    {
        string filename = "/" + movementName + ".dat";
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + filename, FileMode.OpenOrCreate);
        List<List<SerializableMovementData>> serializedDataset = new List<List<SerializableMovementData>>();
     
        foreach (movementDataset m in movementDatasets) {
            List<movementData> tempList = m.movementDataList;
            List<SerializableMovementData> resultList = new List<SerializableMovementData>();
            foreach(movementData md in tempList)
            {
                SerializableMovementData t = md;
                resultList.Add(t);
            }
            serializedDataset.Add(resultList);
        }
        bf.Serialize(file, serializedDataset);
        file.Close();
        //Debug.Log (SaveData.currentLevel);
    }

    private bool Load()
    {
        string filename = "/" + movementName + ".dat";
        if (File.Exists(Application.persistentDataPath + filename))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + filename, FileMode.Open);
            List<List<SerializableMovementData>> serializedDataset = (List<List<SerializableMovementData>>)bf.Deserialize(file);
            foreach (List<SerializableMovementData> m in serializedDataset)
            {
                List<movementData> resultList = new List<movementData>();
                foreach (SerializableMovementData md in m)
                {

                    movementData t = md;
                    resultList.Add(t);
                }
                movementDatasets.Add(new movementDataset(resultList));
            }
            file.Close();
            return (true);
        }
        else
        {
            return (false);
        }
    }
}
