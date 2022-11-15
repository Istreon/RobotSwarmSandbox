using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/**
 * Represent a movement frame
 */

[System.Serializable]
public class movementData
{
    public Vector3 position;
    public Quaternion rotation;
    public float speed;
    public Vector3 direction;



    public movementData(Vector3 p, Quaternion r, float s, Vector3 d)
    {
        rotation = r;
        position = p;
        speed = s;
        direction = d;
    }
    public movementData(movementData md)
    {
        rotation = md.rotation;
        position = md.position;
        speed = md.speed;
        direction = md.direction;
    }


    /**
     * Return a movement data representing the average movement frame of the movement data set
     */
    public static movementData getAverageMovementData(List<movementData> movementDataList)
    {
        bool first = true; ;
        float speed = 0;
        Vector3 position = new Vector3();
        Vector3 direction = new Vector3();
        Quaternion rotation = new Quaternion();

        float i = 0.0f;
        foreach (movementData md in movementDataList)
        {
            i++;
            speed += md.speed;
            position += md.position;
            direction += md.direction;

            if (first)
            {
                rotation = md.rotation;
            }
            else
            {
                rotation = Quaternion.Slerp(rotation, md.rotation, 1.0f / i);
            }


            first = false;
        }
        direction /= movementDataList.Count;
        position /= movementDataList.Count;
        speed /= movementDataList.Count;
        return new movementData(position, rotation, speed, direction);
    }


    /**
     * Return a movement data representing the average movement frame of the movement data set
     * The relative position depend of the instant speed
     */
    public static movementData getAverageMovementDataBasedOnSpeed(List<movementData> movementDataList)
    {
        bool first = true; ;
        float speed = 0;
        Vector3 position = new Vector3();
        Vector3 direction = new Vector3();
        Quaternion rotation= new Quaternion();

        float i = 0.0f;
        foreach (movementData md in movementDataList)
        {
            i++;
            speed += md.speed;
            position += (md.position*md.speed);
            direction += md.direction;

            if (first)
            {
                rotation = md.rotation;
            }
            else
            {
                rotation = Quaternion.Slerp(rotation, md.rotation, 1.0f / i);
            }


            first = false;
        }
        direction /= movementDataList.Count;
        position /= speed;
        speed /= movementDataList.Count;
        return new movementData(position,rotation, speed,direction);
    }

    /**
     * Normalize the movement dataset to be comparable with other movement datasets
     */
    public static List<movementData> normalizeMovementDataset(List<movementData> movementDataList)
    {

        if (movementDataList == null) Debug.LogError("movementData.normalizeMovementDataset : Parameter can't be null"); 
        movementData averageMovementData = movementData.getAverageMovementDataBasedOnSpeed(movementDataList);
        GameObject parent = new GameObject();
        parent.transform.SetParent(null);
        parent.transform.SetPositionAndRotation(averageMovementData.position,averageMovementData.rotation);


        //Transform all movement data into gameobject as child of the average movement data
        List<GameObject> sons = new List<GameObject>();

        foreach (movementData md in movementDataList)
        {
            GameObject son = new GameObject();
            son.transform.position = md.position;
            son.transform.rotation = md.rotation;
            son.transform.SetParent(parent.transform);
            sons.Add(son);
        }

        //Update position and rotation of the parent gameobject. (Unity update automatically all his son transform)
        parent.transform.SetPositionAndRotation(new Vector3(0f,0f,0f), new Quaternion(0f,0f,0f,1f));
     


        //Transform all gameobject as movement data and delete them
        List<movementData> resultMovementDataList = new List<movementData>();
        int i = 0;
        foreach(GameObject g in sons)
        {
            g.transform.SetParent(null);
            movementData temp = new movementData(g.transform.position, g.transform.rotation, movementDataList[i].speed, movementDataList[i].direction);
            resultMovementDataList.Add(temp);
            Object.Destroy(g);
            i++;
        }
        Object.Destroy(parent);


        return resultMovementDataList;
    }
}
