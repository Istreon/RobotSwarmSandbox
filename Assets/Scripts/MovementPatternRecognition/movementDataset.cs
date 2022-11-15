using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * Represent a movement, composed by multiple movement data
 */
[System.Serializable]
public class movementDataset
{
    public List<movementData> movementDataList;  //set of movement frame, respresenting the movement
    public movementData averageMovementData; //movement data, representing the average of all movement data from movementDataList


    public movementDataset(List<movementData> list)
    {
        movementDataList = list;
        averageMovementData = movementData.getAverageMovementDataBasedOnSpeed(list);
    }
}
