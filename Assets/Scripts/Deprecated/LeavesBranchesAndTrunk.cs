using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeavesBranchesAndTrunk : Displayer
{
    #region Serialized fields
    [SerializeField]
    private GameObject prefab;
    #endregion

    #region Private fields
    private List<GameObject> displayCube = new List<GameObject>();
    private List<Color> colorPalette;
    #endregion

    #region Methods - MonoBehaviour callbacks
    // Start is called before the first frame update
    void Start()
    {
        colorPalette = ColorTools.GetColorPalette(3);
    }
    #endregion

    #region Methods - Displayer override
    public override void DisplayVisual(SwarmData swarmData)
    {
        ClearVisual();
        Tuple<List<AgentData>, List<AgentData>, List<AgentData>> tuple = SwarmTools.SeparateLeavesBranchesAndTrunk(swarmData);


        foreach (AgentData a in tuple.Item1)
        {
            GameObject temp = GameObject.Instantiate(prefab);
            temp.transform.position = a.GetPosition();
            temp.GetComponent<Renderer>().material.color = colorPalette[0];
            temp.transform.parent = this.transform;
            displayCube.Add(temp);
        }

        foreach (AgentData a in tuple.Item2)
        {
            GameObject temp = GameObject.Instantiate(prefab);
            temp.transform.position = a.GetPosition();
            temp.GetComponent<Renderer>().material.color = colorPalette[1];
            temp.transform.parent = this.transform;
            displayCube.Add(temp);
        }

        foreach (AgentData a in tuple.Item3)
        {
            GameObject temp = GameObject.Instantiate(prefab);
            temp.transform.position = a.GetPosition();
            temp.GetComponent<Renderer>().material.color = colorPalette[2];
            temp.transform.parent = this.transform;
            displayCube.Add(temp);
        }

    }

    public override void ClearVisual()
    {
        foreach (GameObject g in displayCube)
        {
            Destroy(g);
        }
        displayCube.Clear();
    }
    #endregion
}
