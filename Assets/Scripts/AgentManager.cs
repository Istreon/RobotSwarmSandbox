using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private GameObject mapPrefab;

    [SerializeField]
    private int numberOfAgents = 40;

    [SerializeField]
    [Range(5, 20)]
    private float mapSizeX = 5.0f;

    [SerializeField]
    [Range(5, 20)]
    private float mapSizeZ = 5.0f;


    private Camera mainCamera;


    private List<GameObject> agents;
    // Start is called before the first frame update
    void Start()
    {

        GameObject map=Instantiate(mapPrefab);
        map.transform.parent = null;
        map.transform.position = new Vector3(mapSizeX / 2.0f, 0.0f, mapSizeZ / 2.0f);
        map.transform.localScale = new Vector3(mapSizeX, 1.0f, mapSizeZ);

        mainCamera = FindObjectOfType<Camera>();
        mainCamera.transform.position= new Vector3(mapSizeX / 2.0f, Mathf.Max(mapSizeZ,mapSizeX), mapSizeZ / 2.0f);

        agents = new List<GameObject>();
        for(int i=0; i<numberOfAgents; i++)
        {
            GameObject newAgent=GameObject.Instantiate(prefab);
            newAgent.transform.position = new Vector3(Random.Range(0.0f, mapSizeX), 0.1f, Random.Range(0.0f, mapSizeZ));
            newAgent.transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 359.0f), 0.0f);
            agents.Add(newAgent);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<GameObject> GetAgents()
    {
        return agents;
    }

    public float GetMapSizeX()
    {
        return mapSizeX;
    }

    public float GetMapSizeZ()
    {
        return mapSizeZ;
    }
}
