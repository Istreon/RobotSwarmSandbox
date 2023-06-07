using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Communities : Displayer
{
    public GameObject prefab;
    List<GameObject> displayCube = new List<GameObject>();
    List<Color> colorPalette;

    // Start is called before the first frame update
    void Start()
    {
        colorPalette = ColorTools.GetShuffledColorPalette(10);
    }

    public override void DisplayVisual(LogClipFrame frame)
    {
        ClearVisual();
        List<List<LogAgentData>> communities = ClipTools.GetOrderedCommunities(frame);

        for (int i = 0; i < communities.Count; i++)
        {
            foreach (LogAgentData a in communities[i])
            {
                GameObject temp = GameObject.Instantiate(prefab);
                temp.transform.position = a.getPosition();
                temp.GetComponent<Renderer>().material.color = colorPalette[i % 10];
                displayCube.Add(temp);
            }
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
}
