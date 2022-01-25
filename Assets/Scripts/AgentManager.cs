using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private int numberOfAgents = 40;

    [SerializeField]
    private float maxPosition = 5.0f;


    private List<GameObject> agents;
    // Start is called before the first frame update
    void Start()
    {
        agents = new List<GameObject>();
        for(int i=0; i<numberOfAgents; i++)
        {
            GameObject newAgent=GameObject.Instantiate(prefab);
            newAgent.transform.position = new Vector3(Random.Range(0.0f, maxPosition), 0.1f, Random.Range(0.0f, maxPosition));
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

    public float GetMaxPosition()
    {
        return maxPosition;
    }
}
