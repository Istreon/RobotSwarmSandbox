using System.Collections.Generic;
using UnityEngine;

public class Communities : Displayer
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
        colorPalette = ColorTools.GetShuffledColorPalette(10);
    }
    #endregion

    #region Methods - Displayer override
    public override void DisplayVisual(SwarmData swarmData)
    {
        ClearVisual();
        List<List<AgentData>> communities = SwarmTools.GetOrderedCommunities(swarmData);

        for (int i = 0; i < communities.Count; i++)
        {
            foreach (AgentData a in communities[i])
            {
                GameObject temp = GameObject.Instantiate(prefab);
                temp.transform.position = a.GetPosition();
                temp.GetComponent<Renderer>().material.color = colorPalette[i % 10];
                temp.transform.parent = this.transform;
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
    #endregion
}
