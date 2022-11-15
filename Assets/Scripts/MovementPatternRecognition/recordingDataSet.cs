using System.Collections.Generic;
using UnityEngine;

public class recordingDataSet : MonoBehaviour
{


    [SerializeField]
    private movementDatasetStorage store = null;

    [SerializeField]
    private GameObject cube = null;

    [SerializeField]
    private GameObject cube2 = null;

    [SerializeField]
    private GameObject cube3 = null;

    [SerializeField]
    private GameObject cube4 = null;


    private bool recording = false;
    private bool update = false;

    private List<movementData> movementDataList;
    private List<movementData> normalizedMovementDataList;

    private int datasetSize;


    private List<movementData> continuousMovementDataList;
    private List<movementData> normalizedCountinuousMovementDataList;


    int framePassed = 0;
    private int index = 0;
    private Vector3 previousPosition;
    // Start is called before the first frame update
    void Start()
    {
        resetRecording();
        previousPosition = transform.position;
        continuousMovementDataList = new List<movementData>();
        normalizedCountinuousMovementDataList = new List<movementData>();
    }

    // Update is called once per frame
    void Update()
    {

        //Start recording
        if(!recording && Input.GetKeyDown(KeyCode.Space) && !update)
        {
            Debug.Log("Recording ... ");
            resetRecording();
            recording = true;
        } else if(recording && Input.GetKeyDown(KeyCode.Space))
        {
            //Stop recording
            Debug.Log("Recording ended ... ");
            recording = false;
            update = true;
            normalizedMovementDataList = movementData.normalizeMovementDataset(movementDataList);
            index = 0;
        }
        if(update) 
        {
            //Delete first data registered from the list
            if (Input.GetKeyDown(KeyCode.O))
            {
                if (movementDataList.Count > 0)
                {
                    movementDataList.RemoveAt(0);
                    normalizedMovementDataList = movementData.normalizeMovementDataset(movementDataList);
                    index = (index + movementDataList.Count - 1) % movementDataList.Count;
                }
                
            }
            //Delete Last data registered from the list
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (movementDataList.Count > 0)
                {
                    movementDataList.RemoveAt(movementDataList.Count-1);
                    normalizedMovementDataList = movementData.normalizeMovementDataset(movementDataList);
                    index = (index + movementDataList.Count - 1) % movementDataList.Count;
                }
                
            }
            //Validate registered data
            if (Input.GetKeyDown(KeyCode.V))
            {
                update = false;
                normalizedMovementDataList = movementData.normalizeMovementDataset(movementDataList);
                store.addNewDataSet(normalizedMovementDataList);



                //PRINT
                string speedMessage = "Speeds : [";
                string positionMessage = "Positions : [";
                foreach (movementData m in movementDataList)
                {
                    speedMessage += " " + m.speed;
                }
                speedMessage += " ]";

                foreach (movementData m in movementDataList)
                {
                    positionMessage += " " + m.position;
                }
                positionMessage += " ]";
                Debug.Log(positionMessage);
                Debug.Log(speedMessage);
            }
            //Ignore registered data
            if (Input.GetKeyDown(KeyCode.N))
            {
                update = false;

                Debug.Log("deleted ...");
            }

        }
    }

    void FixedUpdate()
    {
        float dist = Mathf.Abs(Vector3.Distance(previousPosition, transform.position));
        float speed = dist / Time.deltaTime;
        Vector3 direction = transform.position - previousPosition;

        movementData newData = new movementData(transform.position, transform.rotation, speed,direction);

        continuousMovementDataList.Add(newData);
        datasetSize = store.getReferenceDataSetSize();
        if (continuousMovementDataList.Count>=datasetSize)
        {
            while(continuousMovementDataList.Count > datasetSize)
            {
                continuousMovementDataList.RemoveAt(0);
            }
            if(!wait() && (datasetSize != 0))
            {
                normalizedCountinuousMovementDataList = movementData.normalizeMovementDataset(continuousMovementDataList);
                bool res = store.checkDatasets(normalizedCountinuousMovementDataList);
                if(res)
                {
                    Debug.Log("Mouvement : "+store.getMovementName() + " reconnu avec succes");
                    recognizedMovement();
                }
            }
        }
        if (recording)
        {


            movementData temp = new movementData(transform.position, transform.rotation, speed,direction);

            movementDataList.Add(temp);
        }
        if(update)
        {
            movementData temp = movementData.getAverageMovementDataBasedOnSpeed(movementDataList);
            cube2.transform.SetPositionAndRotation(temp.position, temp.rotation);
            cube.transform.SetPositionAndRotation(movementDataList[index].position, movementDataList[index].rotation);

            movementData temp2 = movementData.getAverageMovementDataBasedOnSpeed(normalizedMovementDataList);
            cube4.transform.SetPositionAndRotation(temp2.position, temp2.rotation);
            cube3.transform.SetPositionAndRotation(normalizedMovementDataList[index].position, normalizedMovementDataList[index].rotation);
            index = (index+1) % movementDataList.Count;
        }


        previousPosition = transform.position;
    }

    private void resetRecording()
    {
        movementDataList = new List<movementData>();
    }

    private bool wait()
    {

        if(framePassed<datasetSize)
        {
            framePassed++;
            return true;
        }
        return false;
    }

    private void recognizedMovement()
    {
        framePassed = 0;
    }
}
